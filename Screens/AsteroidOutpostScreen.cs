using System;
using System.Collections.Generic;
using System.Diagnostics;
using AsteroidOutpost.Entities;
using AsteroidOutpost.Entities.Eventing;
using AsteroidOutpost.Entities.Structures;
using AsteroidOutpost.Entities.Units;
using AsteroidOutpost.Interfaces;
using AsteroidOutpost.Networking;
using AsteroidOutpost.Scenarios;
using AsteroidOutpost.Screens.HeadsUpDisplay;
using C3.XNA;
using C3.XNA.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace AsteroidOutpost.Screens
{
	public class AsteroidOutpostScreen : Screen, IReflectionTarget, IActorIDProvider, IComponentList
	{
		private LayeredStarField layeredStarField;
		private QuadTree<Entity> quadTree;
		private Dictionary<int, Component> componentDictionary = new Dictionary<int, Component>(6000);		// Note: This variable must be kept thread-safe
		private Dictionary<int, Entity> entityDictionary = new Dictionary<int, Entity>(2000);		// Note: This variable must be kept thread-safe
		private Dictionary<int, PowerGrid> powerGrid = new Dictionary<int, PowerGrid>(4);
		private AOHUD hud;
		private Scenario scenario;

		private bool paused;
		private bool drawQuadTree = false;

		private bool isServer = true;
		private AONetwork network;

		private int nextComponentID = 0;
		private int nextForceID = 0;
		private int nextActorID = 0;


		private readonly List<Actor> actors = new List<Actor>();
		private readonly List<Force> forces = new List<Force>();

		public event Action<EntityEventArgs> StructureStartedEventPreAuth;
		public event Action<EntityEventArgs> StructureStartedEventPostAuth;
		
		
		// Private because this is a singleton. Make an instance and use this class though the following two functions
		public AsteroidOutpostScreen(ScreenManager theScreenManager) : base(theScreenManager)
		{
			network = new AONetwork(this);
		}


		public int ID
		{
			get
			{
				return AONetwork.SpecialTargetTheGame;
			}
		}


		/// <summary>
		/// Gets all of the Actors in the game
		/// </summary>
		public IEnumerable<Actor> Actors
		{
			get
			{
				return actors;
			}
		}


		/// <summary>
		/// Gets all of the Forces in the game
		/// </summary>
		public IEnumerable<Force> Forces
		{
			get
			{
				return forces;
			}
		}


		/// <summary>
		/// Gets or Sets the paused state of the game. While paused, drawing will still occur, but updates will be paused
		/// </summary>
		public bool Paused
		{
			get { return paused; }
			set
			{
				// TODO: Handle this over the network. Should clients be allowed to pause the server?
				paused = value;
			}
		}


		/// <summary>
		/// Adds the component to the game. If the game is a client, this will send a request to the server instead
		/// </summary>
		/// <param name="component">The component to add to the game</param>
		public void AddComponent(Component component)
		{
			Add(component, isServer);
		}


		/// <summary>
		/// Adds the identifiable item to the game. If the game is a client, this will send a request to the server instead
		/// </summary>
		/// <param name="obj">The identifiable item to add to the game</param>
		public void Add(IIdentifiable obj)
		{
			Entity entity = obj as Entity;
			if (entity != null)
			{
				Add(entity, isServer);
			}
			else
			{
				Component component = obj as Component;
				if (component != null)
				{
					Add(component, isServer);
				}
				else
				{
					// We were unable to add the item to the game
					Debugger.Break();
				}
			}
		}


		/// <summary>
		/// Adds the component to the game. If isAuthoritative is set to false, this will send a request to the server instead
		/// </summary>
		/// <param name="component">The component to add to the game</param>
		/// <param name="isAuthoritative">If true, indicates that this instance of the game is a server OR that we have been told to do this by the server</param>
		public void Add(Component component, bool isAuthoritative)
		{
			if (component != null)
			{
				if (isAuthoritative)
				{
					if (isServer)
					{
						if (component.ID == -1)
						{
							// Assign an ID and replicate this object to the clients
							component.ID = PopNextComponentID();
						}

						network.EnqueueMessage(new AOReflectiveOutgoingMessage(this.ID,
						                                                       "Add",
																			   new object[] { component, true }));
					}
					// Tell the network to listen to anything that may happen
					network.ListenToEvents(component);

					// Add this to a dictionary for quick ID-based lookups
					lock (componentDictionary)
					{
						componentDictionary.Add(component.ID, component);
					}
				}
				else
				{
					// Assign an ID
					component.ID = PopNextComponentID();

					// Ask the server to make it
					network.EnqueueMessage(new AOReflectiveOutgoingMessage(this.ID,
					                                                       "Add",
																		   new object[] { component }));
				}
			}
		}


		/// <summary>
		/// Adds the entity to the game. If isAuthoritative is set to false, this will send a request to the server instead
		/// </summary>
		/// <param name="entity">The entity to add to the game</param>
		/// <param name="isAuthoritative">If true, indicates that this instance of the game is a server OR that we have been told to do this by the server</param>
		public void Add(Entity entity, bool isAuthoritative)
		{
			if (entity != null)
			{
				if (isAuthoritative)
				{
					if (isServer)
					{
						if (entity.ID == -1)
						{
							// Assign an ID and replicate this object to the clients
							entity.ID = PopNextComponentID();
						}

						network.EnqueueMessage(new AOReflectiveOutgoingMessage(this.ID,
																			   "Add",
																			   new object[] { entity, true }));

						// If we are the server, we do both the pre and post auth events back to back  (post auth is below)
						if (StructureStartedEventPreAuth != null)
						{
							StructureStartedEventPreAuth(new EntityEventArgs(entity));
						}
					}
					// Tell the network to listen to anything that may happen
					network.ListenToEvents(entity);

					// Add this to the quad tree for rapid area-based lookups
					quadTree.Add(entity);

					// Add this to a dictionary for quick ID-based lookups
					lock (componentDictionary)
					{
						entityDictionary.Add(entity.ID, entity);
					}
					if (StructureStartedEventPostAuth != null)
					{
						StructureStartedEventPostAuth(new EntityEventArgs(entity));
					}
				}
				else
				{
					// Assign an ID
					entity.ID = PopNextComponentID();

					// Ask the server to make it
					network.EnqueueMessage(new AOReflectiveOutgoingMessage(this.ID,
																		   "Add",
																		   new object[] { entity }));
					if (StructureStartedEventPreAuth != null)
					{
						StructureStartedEventPreAuth(new EntityEventArgs(entity));
					}
				}
			}
		}

		private int PopNextComponentID()
		{
			return nextComponentID++;
		}


		/// <summary>
		/// Gets a complete list of the Entities that are in the game
		/// </summary>
		public ICollection<Entity> Entities
		{
			get
			{
				return quadTree;
			}
		}

		/// <summary>
		/// Gets a list of entities that are intersecting with the search area
		/// </summary>
		/// <param name="rect">The search area</param>
		/// <returns>A list of entities that are intersecting with the search area</returns>
		public List<Entity> EntitiesInArea(Rectangle rect)
		{
			return quadTree.GetObjects(rect);
		}

		/// <summary>
		/// Gets a list of entities that are intersecting with the search area
		/// </summary>
		/// <param name="x">The search area's X coordinate</param>
		/// <param name="y">The search area's Y coordinate</param>
		/// <param name="w">The search area's Width</param>
		/// <param name="h">The search area's Height</param>
		/// <returns>A list of entities that are intersecting with the search area</returns>
		public List<Entity> EntitiesInArea(int x, int y, int w, int h)
		{
			return EntitiesInArea(new Rectangle(x, y, w, h));
		}


		/// <summary>
		/// Looks up a entity by ID
		/// This method is thread safe
		/// </summary>
		/// <param name="id">The ID to look up</param>
		/// <returns>The entity with the given ID, or null if the entity is not found</returns>
		public Entity GetEntity(int id)
		{
			lock (entityDictionary)
			{
				if (entityDictionary.ContainsKey(id))
				{
					return entityDictionary[id];
				}

				//Debugger.Break();
				return null;
			}
		}


		/// <summary>
		/// Looks up a component by ID
		/// This method is thread safe
		/// </summary>
		/// <param name="id">The ID to look up</param>
		/// <returns>The component with the given ID, or null if the component is not found</returns>
		public Component GetComponent(int id)
		{
			lock (componentDictionary)
			{
				if (componentDictionary.ContainsKey(id))
				{
					return componentDictionary[id];
				}

				//Debugger.Break();
				return null;
			}
		}


		/// <summary>
		/// Looks up a component by ID
		/// This method is thread safe
		/// </summary>
		/// <param name="id">The ID to look up</param>
		/// <returns>The component with the given ID, or null if the component is not found</returns>
		public IReflectionTarget GetTarget(int id)
		{
			Component component = GetComponent(id);
			if(component != null)
			{
				return component;
			}
			else
			{
				Entity entity = GetEntity(id);
				if(entity != null)
				{
					return entity;
				}
				else
				{
					Debugger.Break();
					return null;
				}
			}
		}


		/// <summary>
		/// Gets the width of the configured playing area. This is not a hard boundary
		/// </summary>
		public int MapWidth
		{
			get { return quadTree.QuadRect.Width; }
		}


		/// <summary>
		/// Gets the height of the configured playing area. This is not a hard boundary
		/// </summary>
		public int MapHeight
		{
			get { return quadTree.QuadRect.Height; }
		}


		/// <summary>
		/// Converts a screen location to a world location
		/// </summary>
		/// <param name="point">The Screen location</param>
		/// <returns>The World Location</returns>
		public Vector2 ScreenToWorld(Vector2 point)
		{
			return hud.ScreenToWorld(point.X, point.Y);
		}


		/// <summary>
		/// Converts a screen location to a world location
		/// </summary>
		/// <param name="x">The Screen location's X</param>
		/// <param name="y">The Screen location's Y</param>
		/// <returns>The World Location</returns>
		public Vector2 ScreenToWorld(float x, float y)
		{
			return hud.ScreenToWorld(x, y);
		}


		/// <summary>
		/// Converts a world location to a screen location
		/// </summary>
		/// <param name="point">The World location</param>
		/// <returns>The Screen location</returns>
		public Vector2 WorldToScreen(Vector2 point)
		{
			return hud.WorldToScreen(point);
		}


		/// <summary>
		/// Converts a world location to a screen location
		/// </summary>
		/// <param name="x">The World location's X</param>
		/// <param name="y">The World location's Y</param>
		/// <returns>The Screen location</returns>
		public Vector2 WorldToScreen(float x, float y)
		{
			return hud.WorldToScreen(x, y);
		}


		/// <summary>
		/// Gets of sets the scale factor for the game. Smaller scale zooms in
		/// </summary>
		public float ScaleFactor
		{
			get
			{
				return hud.ScaleFactor;
			}
			set
			{
				hud.ScaleFactor = value;
			}
		}

		public float Scale(float value)
		{
			return hud.Scale(value);
		}
		public Vector2 Scale(Vector2 value)
		{
			return hud.Scale(value);
		}


		/// <summary>
		/// Gets whether this instance of the game is a server
		/// </summary>
		public bool IsServer
		{
			get { return isServer; }
			set
			{
				isServer = value;
			}
		}

		public AOHUD HUD
		{
			get
			{
				return hud;
			}
		}

		internal AONetwork Network
		{
			get
			{
				return network;
			}
		}


		internal PowerGrid PowerGrid(Force force)
		{
			return powerGrid[force.ID];
		}

		internal PowerGrid PowerGrid(int forceID)
		{
			return powerGrid[forceID];
		}


		public void CreatePowerGrid(Force force)
		{
			powerGrid.Add(force.ID, new PowerGrid(this, this));
			if(isServer)
			{
				network.EnqueueMessage(new AOReflectiveOutgoingMessage(-2,
				                                                       "CreatePowerGrid",
				                                                       new object[]{ force }));
			}
		}


		/// <summary>
		/// Starts this instance of the game as a server
		/// </summary>
		public void StartServer(Scenario theScenario)
		{
			if(scenario != null)
			{
				scenario.End();
			}
			scenario = theScenario;

			isServer = true;
			scenario.Start();
			network.StartGame();
		}



		/// <summary>
		/// Starts this instance of the game as a client
		/// </summary>
		public void StartClient(int startingComponentID)
		{
			nextComponentID = startingComponentID;
			isServer = false;
			ScreenMan.SwitchScreens("Game");
			hud.FocusWorldPoint = new Vector2(MapWidth / 2f, MapHeight / 2f);
		}



		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		public override void LoadContent(SpriteBatch spriteBatch, ContentManager content)
		{
			Ship1.LoadContent(spriteBatch, content);
			SolarStation.LoadContent(spriteBatch, content);
			Asteroid.LoadContent(spriteBatch, content);
			LaserMiner.LoadContent(spriteBatch, content);
			LaserTower.LoadContent(spriteBatch, content);
			PowerNode.LoadContent(spriteBatch, content);
			Beacon.LoadContent(spriteBatch, content);

			TextureDictionary.Add("Sprites\\Power", "power");
			TextureDictionary.Add("Sprites\\Asteroids", "asteroids");
			TextureDictionary.Add("Sprites\\Miners", "miners");
			TextureDictionary.Add("Sprites\\SolarStation", "solarStation");
			TextureDictionary.Add("Sprites\\Spaceship", "spaceship");

			TextureDictionary.Add("Ellipse25", "ellipse25");
			TextureDictionary.Add("Ellipse25Bold", "ellipse25bold");
			TextureDictionary.Add("Ellipse25Back", "ellipse25back");
			TextureDictionary.Add("Ellipse25Front", "ellipse25front");
			TextureDictionary.Add("Ellipse50", "ellipse50");
			TextureDictionary.Add("Ellipse50Back", "ellipse50back");
			TextureDictionary.Add("Ellipse50Front", "ellipse50front");
			TextureDictionary.Add("Ellipse100", "ellipse100");
			TextureDictionary.Add("Ellipse220", "ellipse220");

			TextureDictionary.Add("powerline", "powerline");

			// TODO: Create this on the server, then send the size to the clients
			quadTree = new QuadTree<Entity>(0, 0, 20000, 20000);

			hud = new AOHUD(ScreenMan, this);
			hud.LoadContent(spriteBatch, content);

		}



		/// <summary>
		/// UnloadContent will be called once per game and is the place to unload
		/// all content.
		/// </summary>
		public override void UnloadContent()
		{
			// Unload any non ContentManager content here
			if (network != null)
			{
				network.Dispose();
			}
		}


		/// <summary>
		/// Update the world
		/// </summary>
		/// <param name="deltaTime">The elapsed time</param>
		/// <param name="theMouse"></param>
		/// <param name="theKeyboard"></param>
		public override void Update(TimeSpan deltaTime, EnhancedMouseState theMouse, EnhancedKeyboardState theKeyboard)
		{

			network.ProcessIncomingQueue(deltaTime);
			
			if (!paused)
			{
				// Update the current scenario, if any
				if(scenario != null)
				{
					scenario.Update(deltaTime);
				}


				// Update the Actors
				foreach (Actor actor in actors)
				{
					actor.Update(deltaTime);
				}


				// Update the components and entities
				List<Component> deadComponents = new List<Component>();
				foreach (Component component in componentDictionary.Values)
				{
					if (!component.IsDead())
					{
						component.Update(deltaTime);
					}
					if (component.IsDead())
					{
						deadComponents.Add(component);
					}
				}

				List<Entity> deadEntities = new List<Entity>();
				foreach (Entity entity in entityDictionary.Values)
				{
					if (!entity.IsDead())
					{
						entity.Update(deltaTime);
					}
					if (entity.IsDead())
					{
						deadEntities.Add(entity);
					}


					// TODO: Find a way to listen for move events instead of always updating the position
					quadTree.Move(entity);
				}

				// Delete any components or entities that need to be deleted
				foreach (Component deadComponent in deadComponents)
				{
					lock (componentDictionary)
					{
						componentDictionary.Remove(deadComponent.ID);
					}
				}

				foreach (Entity deadEntity in deadEntities)
				{
					quadTree.Remove(deadEntity);

					lock (componentDictionary)
					{
						componentDictionary.Remove(deadEntity.ID);
					}
				}

			}

			// Update the HUD
			hud.Update(deltaTime, theMouse, theKeyboard);


			// Update the stars
			layeredStarField.Update(deltaTime);

			network.ProcessOutgoingQueue();

			base.Update(deltaTime, theMouse, theKeyboard);
		}


		/// <summary>
		/// Draw the world
		/// </summary>
		/// <param name="spriteBatch"></param>
		/// <param name="tint"></param>
		public override void Draw(SpriteBatch spriteBatch, Color tint)
		{
			if(drawQuadTree)
			{
				DrawQuad(spriteBatch, quadTree, 0);
			}

			// Draw the back of the HUD
			hud.DrawBack(spriteBatch, tint);
			

			// Draw all the visible entities
			List<Entity> visible = quadTree.GetObjects(hud.FocusScreen);
			foreach (Entity entity in visible)
			{
				entity.Draw(spriteBatch, 1, tint);
			}

			foreach (var grid in powerGrid.Values)
			{
				grid.Draw(spriteBatch);
			}

			// Draw the front of the HUD
			hud.DrawFront(spriteBatch, tint);

			base.Draw(spriteBatch, tint);
		}



		/// <summary>
		/// Gets the force with the given ID, or null if the force is not found
		/// </summary>
		/// <param name="owningForceID"></param>
		/// <returns></returns>
		public Force GetForce(int owningForceID)
		{
			foreach(Force force in forces)
			{
				if(force.ID == owningForceID)
				{
					return force;
				}
			}

			// All forces should be found. What is going on?
			Debugger.Break();
			return null;
		}



		public int GetNextEntityID()
		{
			return nextComponentID++;
		}


		public int GetNextForceID()
		{
			return nextForceID++;
		}


		public int GetNextActorID()
		{
			return nextActorID++;
		}


		public void AddForce(Force force)
		{
			forces.Add(force);

			if (isServer)
			{
				network.EnqueueMessage(new AOReflectiveOutgoingMessage(ID,
				                                                       "AddForce",
				                                                       new object[]{force}));
			}

			network.ListenToEvents(force);
		}


		public void AddActor(Actor actor)
		{
			actors.Add(actor);
			if(actor.Role == ActorRole.Local && hud.LocalActor == null)
			{
				hud.LocalActor = actor;
			}

		}


		// TODO: Pass these events straight to the force instead of using (aka cheating) the game as a middle-man
		public void SetForceMinerals(int forceID, int minerals)
		{
			GetForce(forceID).SetMinerals(minerals);
		}
		public void SetForceMinerals(int forceID, int minerals, bool authoritative)
		{
			GetForce(forceID).SetMinerals(minerals, authoritative);
		}


		#region Draw QuadTree

		private void DrawQuad(SpriteBatch spriteBatch, QuadTree<Entity> quad, int depth)
		{
			if (quad != null)
			{
				DrawQuad(spriteBatch, quad.RootQuad, depth);
			}
		}

		private void DrawQuad(SpriteBatch spriteBatch, QuadTreeNode<Entity> quad, int depth)
		{
			if (quad != null)
			{

				Rectangle rect = quad.QuadRect;

				Color drawColor;
				switch (depth)
				{
				default:
					goto case 0;
				case 0:
					drawColor = Color.White;
					break;
				case 1:
					drawColor = Color.Red;
					break;
				case 2:
					drawColor = Color.Green;
					break;
				case 3:
					drawColor = Color.Blue;
					break;
				case 4:
					drawColor = Color.Gray;
					break;
				case 5:
					drawColor = Color.DarkRed;
					break;
				case 6:
					drawColor = Color.DarkGreen;
					break;
				case 7:
					drawColor = Color.DarkBlue;
					break;
				}

				Vector2 screenTopLeft = WorldToScreen(rect.X, rect.Y);
				Vector2 screenBottomLeft = WorldToScreen(rect.Right, rect.Bottom);

				rect = new Rectangle((int)screenTopLeft.X,
				                     (int)screenTopLeft.Y,
				                     (int)(screenBottomLeft.X - screenTopLeft.X),
				                     (int)(screenBottomLeft.Y - screenTopLeft.Y));
				spriteBatch.DrawRectangle(rect, drawColor, 1);

				DrawQuad(spriteBatch, quad.TopLeftChild, depth + 1);
				DrawQuad(spriteBatch, quad.TopRightChild, depth + 1);
				DrawQuad(spriteBatch, quad.BottomLeftChild, depth + 1);
				DrawQuad(spriteBatch, quad.BottomRightChild, depth + 1);
			}
		}
		#endregion



		public void SetScene(LayeredStarField starField)
		{
			this.layeredStarField = starField;
		}


		public void SetFocus(Vector2 focus)
		{
			hud.FocusWorldPoint = focus;
		}
	}
}
