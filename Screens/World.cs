using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using AsteroidOutpost.Components;
using AsteroidOutpost.Entities;
using AsteroidOutpost.Entities.Eventing;
using AsteroidOutpost.Entities.Structures;
using AsteroidOutpost.Entities.Units;
using AsteroidOutpost.Interfaces;
using AsteroidOutpost.Networking;
using AsteroidOutpost.Scenarios;
using AsteroidOutpost.Screens.HeadsUpDisplay;
using Awesomium.Core;
using AwesomiumXNA;
using C3.XNA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Console = System.Console;
using AsteroidOutpost.Systems;

namespace AsteroidOutpost.Screens
{

	public enum StreamType
	{
		RequestServerInfo = 0,
		ServerInfo,
		GameData
	}

	public class World : DrawableGameComponent, IReflectionTarget, IControllerIDProvider, IComponentList
	{
		public const UInt32 Version = 2;
		public const UInt32 StreamIdent = 0x607A0BAD;  // Get it?

		private SpriteBatch spriteBatch;

		private LayeredStarField layeredStarField;
		private QuadTree<Entity> quadTree;
		private readonly AwesomiumComponent awesomium;
		private Dictionary<int, Component> componentDictionary = new Dictionary<int, Component>(6000);		// Note: This variable must be kept thread-safe
		private Dictionary<int, Entity> entityDictionary = new Dictionary<int, Entity>(2000);		// Note: This variable must be kept thread-safe
		private Dictionary<int, PowerGrid> powerGrid = new Dictionary<int, PowerGrid>(4);
		private AOHUD hud;
		private Scenario scenario;
		private PhysicsSystem physicsSystem;

		private bool paused;
		private bool drawQuadTree = false;

		private bool isServer = true;
		private AONetwork network;

		private int nextComponentID = 0;
		private int nextForceID = 0;
		private int nextActorID = 0;


		private readonly List<Controller> controllers = new List<Controller>();
		private readonly List<Force> forces = new List<Force>();

		public event Action<EntityEventArgs> StructureStartedEventPreAuth;
		public event Action<EntityEventArgs> StructureStartedEventPostAuth;
		
		
		public World(AOGame game) : base(game)
		{
			network = new AONetwork(this);
			hud = new AOHUD(game, this);
			physicsSystem = new PhysicsSystem(game, this);
			awesomium = game.Awesomium;

			// TODO: Create this on the server, then send the size to the clients
			quadTree = new QuadTree<Entity>(0, 0, 20000, 20000);

			game.Components.Add(physicsSystem);
			game.Components.Add(hud);
		}


		public int ID
		{
			get
			{
				return AONetwork.SpecialTargetTheGame;
			}
		}


		/// <summary>
		/// Gets all of the Controllers in the game
		/// </summary>
		public IEnumerable<Controller> Controllers
		{
			get
			{
				return controllers;
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
				awesomium.WebView.CallJavascriptFunction("", "SetPaused", new JSValue(paused));
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
			Add(obj, isServer);
		}


		/// <summary>
		/// Adds the identifiable item to the game. If the game is a client, this will send a request to the server instead
		/// </summary>
		/// <param name="obj">The identifiable item to add to the game</param>
		/// <param name="isAuthoritative">If true, indicates that this instance of the game is a server OR that we have been told to do this by the server</param>
		public void Add(IIdentifiable obj, bool isAuthoritative)
		{
			Entity entity = obj as Entity;
			if (entity != null)
			{
				Add(entity, isAuthoritative);
			}
			else
			{
				Component component = obj as Component;
				if (component != null)
				{
					Add(component, isAuthoritative);
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
				
				if (component.ID == -1)
				{
					// Assign an ID
					component.ID = PopNextComponentID();
				}
				else if(isAuthoritative)
				{
					lock(componentDictionary)
					{
						if(componentDictionary.ContainsKey(component.ID))
						{
							// This component already exists
							return;
						}
					}
				}

				if (isServer)
				{
					network.EnqueueMessage(new AOReflectiveOutgoingMessage(this.ID,
					                                                       "Add",
					                                                       new object[]{component, true}));
				}
				else if(!isAuthoritative)
				{
					network.EnqueueMessage(new AOReflectiveOutgoingMessage(this.ID,
					                                                       "Add",
					                                                       new object[]{component}));
				}

				// Tell the network to listen to anything that may happen
				network.ListenToEvents(component);

				// Add this to a dictionary for quick ID-based lookups
				lock (componentDictionary)
				{
					componentDictionary.Add(component.ID, component);
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
				if (entity.ID == -1)
				{
					// Assign an ID and replicate this object to the clients
					entity.ID = PopNextComponentID();
				}
				else if(isAuthoritative && !isServer)
				{
					lock(entityDictionary)
					{
						if (entityDictionary.ContainsKey(entity.ID))
						{
							// This entity already exists locally. Post-auth and exit
							if (StructureStartedEventPostAuth != null)
							{
								StructureStartedEventPostAuth(new EntityEventArgs(entity));
							}

							return;
						}
					}
				}

				if (isServer)
				{
					network.EnqueueMessage(new AOReflectiveOutgoingMessage(this.ID,
					                                                       "Add",
					                                                       new object[]{ entity, true }));
				}
				else if(!isAuthoritative)
				{
					network.EnqueueMessage(new AOReflectiveOutgoingMessage(this.ID,
					                                                       "Add",
					                                                       new object[]{ entity }));
				}


				// Tell the network to listen to anything that may happen
				network.ListenToEvents(entity);

				// Add this to the quad tree for rapid area-based lookups
				quadTree.Add(entity);

				// Add this to a dictionary for quick ID-based lookups
				lock (entityDictionary)
				{
					entityDictionary.Add(entity.ID, entity);
				}

				// Pre and Post-auth. Post only if we're the server, it happens later on the client
				if (StructureStartedEventPreAuth != null)
				{
					StructureStartedEventPreAuth(new EntityEventArgs(entity));
				}
				if (isServer && StructureStartedEventPostAuth != null)
				{
					StructureStartedEventPostAuth(new EntityEventArgs(entity));
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
		/// Retreives a list of components with the type specified
		/// This method is thread safe
		/// </summary>
		/// <returns>Returns a list of components of the type specified</returns>
		public List<T> GetComponents<T>() where T: Component
		{
			lock (componentDictionary)
			{
				return new List<T>(componentDictionary.Where(comp => comp.Value.GetType() == typeof(T)).Select(x => x.Value as T));
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
			return ScreenToWorld(point.X, point.Y);
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

		public bool DrawQuadTree
		{
			get
			{
				return drawQuadTree;
			}
			set
			{
				drawQuadTree = value;
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
			powerGrid.Add(force.ID, new PowerGrid(this));
			/*
			if(isServer)
			{
				network.EnqueueMessage(new AOReflectiveOutgoingMessage(ID,
				                                                       "CreatePowerGrid",
				                                                       new object[]{ force }));
			}
			*/
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
			//ScreenMan.SwitchScreens("Game");
			hud.FocusWorldPoint = new Vector2(MapWidth / 2f, MapHeight / 2f);

			AddController(new Controller(this, ControllerRole.Local, GetForcesOnTeam(Team.Team2)[0]));
		}



		public override void Initialize()
		{
			base.Initialize();
		}


		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			spriteBatch = new SpriteBatch(Game.GraphicsDevice);

			//hud.LoadContent();

			base.LoadContent();
		}



		/// <summary>
		/// UnloadContent will be called once per game and is the place to unload
		/// all content.
		/// </summary>
		protected override void UnloadContent()
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
		public override void Update(GameTime gameTime)
		{
			TimeSpan deltaTime = gameTime.ElapsedGameTime;
			network.ProcessIncomingQueue(deltaTime);
			
			if (!paused)
			{
				// Update the current scenario, if any
				if(scenario != null)
				{
					scenario.Update(deltaTime);
				}


				// Update the Actors
				foreach (Controller actor in controllers)
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


			// Update the stars
			//layeredStarField.Update(deltaTime);

			network.ProcessOutgoingQueue();

			base.Update(gameTime);
		}


		/// <summary>
		/// Draw the world
		/// </summary>
		public override void Draw(GameTime gameTime)
		{
			spriteBatch.Begin();
			if(drawQuadTree)
			{
				DrawQuad(spriteBatch, quadTree, 0);
			}

			// Draw the back of the HUD
			hud.DrawBack(spriteBatch, Color.White);

			// Draw all the visible entities
			List<Entity> visible = quadTree.GetObjects(hud.FocusScreen);
			foreach (Entity entity in visible)
			{
				entity.Draw(spriteBatch, 1, Color.White);
			}

			foreach (var grid in powerGrid.Values)
			{
				grid.Draw(spriteBatch);
			}
			
			spriteBatch.End();

			// Draw the front of the HUD
			//hud.DrawFront(spriteBatch, Color.White);
			base.Draw(gameTime);
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

		public List<Force> GetForcesOnTeam(Team team)
		{
			List<Force> forcesOnTeam = new List<Force>(4);
			foreach (Force force in forces)
			{
				if (force.Team == team)
				{
					forcesOnTeam.Add(force);
				}
			}

			return forcesOnTeam;
		}



		public int GetNextEntityID()
		{
			return nextComponentID++;
		}


		public int GetNextForceID()
		{
			return nextForceID++;
		}


		public int GetNextControllerID()
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


		public void AddController(Controller controller)
		{
			controllers.Add(controller);
			if(controller.Role == ControllerRole.Local && hud.LocalActor == null)
			{
				hud.LocalActor = controller;
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


		public int Width
		{
			get
			{
				return Game.GraphicsDevice.Viewport.Width;
			}
		}

		public int Height
		{
			get
			{
				return Game.GraphicsDevice.Viewport.Height;
			}
		}

		public void Serialize(BinaryWriter bw, bool serializeActors)
		{
			// Write the header
			bw.Write(StreamIdent);
			bw.Write(Version);
			bw.Write((byte)StreamType.GameData);


			// Serialize the forces and actors
			bw.Write(forces.Count);
			foreach (Force force in forces)
			{
				force.Serialize(bw);
			}


			// serialize the power grids
			bw.Write(powerGrid.Count);
			foreach (int gridOwner in powerGrid.Keys)
			{
				bw.Write(gridOwner);
			}


			bw.Write(entityDictionary.Count + componentDictionary.Count);
			foreach (ISerializable serializableObj in componentDictionary.Values.Concat<ISerializable>(entityDictionary.Values))
			{
				if (serializableObj == null || serializableObj.GetType().AssemblyQualifiedName == null)
				{
					Debugger.Break(); // Something is wrong here
					continue;
				}
				bw.Write(serializableObj.GetType().AssemblyQualifiedName);
				serializableObj.Serialize(bw);
			}


			if (serializeActors)
			{
				bw.Write(controllers.Count);
				foreach (Controller actor in controllers)
				{
					actor.Serialize(bw);
				}


				// Add a footer so that we can verify the integrity of this block
				bw.Write(StreamIdent);
			}
		}


		public void Deserialize(byte[] bytes)
		{
			Deserialize(new BinaryReader(new MemoryStream(bytes)));
		}


		public void Deserialize(BinaryReader br)
		{
			UInt32 handshake = br.ReadUInt32();
			if (handshake != World.StreamIdent)
			{
				String msg = "Failed handshake during game deserialization";
				Console.WriteLine(msg);
				Debugger.Break();
				throw new Exception(msg);
			}

			UInt32 version = br.ReadUInt32();
			StreamType streamType = (StreamType)br.ReadByte();

			if(streamType != StreamType.GameData)
			{
				Debugger.Break();
			}


			int forceCount = br.ReadInt32();
			for(int iForce = 0; iForce < forceCount; iForce++)
			{
				Force force = new Force(br);
				AddForce(force);
				force.PostDeserializeLink(this);
			}


			int powerGridCount = br.ReadInt32();
			for (int iGrid = 0; iGrid < powerGridCount; iGrid++)
			{
				int owningForceID = br.ReadInt32();
				CreatePowerGrid(GetForce(owningForceID));
			}


			// Unpack all of the entities and components
			int entityCount = br.ReadInt32();
			List<ISerializable> createdEntities = new List<ISerializable>(entityCount);
			for (int iEntity = 0; iEntity < entityCount; iEntity++)
			{
				String assemName = br.ReadString();

				// Use reflection to make a new entity of... whatever type was sent to us
				Type t = Type.GetType(assemName);
				ConstructorInfo entityConstructor = t.GetConstructor(new Type[] { typeof(BinaryReader) });
				Object obj = entityConstructor.Invoke(new object[]{ br });
				ISerializable serializableObj = obj as ISerializable;
				IIdentifiable identifiableObj = obj as IIdentifiable;

				if(serializableObj != null)
				{
					//createdEntities.Add(serializableObj);
					serializableObj.PostDeserializeLink(this);
				}

				if(identifiableObj != null)
				{
					Add(identifiableObj, true);
				}
			}


			// Link the entities up
			foreach (var createdEntity in createdEntities)
			{
				createdEntity.PostDeserializeLink(this);
			}


			int controllerCount = br.ReadInt32();
			for (int iController = 0; iController < controllerCount; iController++)
			{
				Controller controller = new Controller(br);
				AddController(controller);
				controller.PostDeserializeLink(this);
			}

			// Read the footer
			UInt32 footer = br.ReadUInt32();
			if (footer != World.StreamIdent)
			{
				String msg = "Failed handshake during game deserialization";
				Console.WriteLine(msg);
				Debugger.Break();
				throw new Exception(msg);
			}
		}




	}
}
