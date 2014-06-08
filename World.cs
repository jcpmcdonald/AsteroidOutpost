using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using AsteroidOutpost.Components;
using AsteroidOutpost.Entities;
using AsteroidOutpost.Eventing;
using AsteroidOutpost.Interfaces;
using AsteroidOutpost.Networking;
using AsteroidOutpost.Scenarios;
using AsteroidOutpost.Scenes;
using AsteroidOutpost.Screens;
using AsteroidOutpost.Screens.HeadsUpDisplay;
using Awesomium.Core;
using AwesomiumXNA;
using C3.XNA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using AsteroidOutpost.Systems;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AsteroidOutpost
{
	public enum StreamType
	{
		RequestServerInfo = 0,
		ServerInfo,
		GameData
	}

	public class World : DrawableGameComponent, IReflectionTarget
	{
		public const UInt32 Version = 2;
		public const UInt32 StreamIdent = 0x607A0BAD; // Have you got it?

		private AOGame theGame;
		private readonly EntityFactory entityFactory;
		private readonly UpgradeFactory upgradeFactory;

		private SpriteBatch spriteBatch;
		private QuadTree<Position> quadTree;

		private AwesomiumComponent awesomium;

		[JsonProperty]
		private readonly Dictionary<int, List<Component>> entityDictionary = new Dictionary<int, List<Component>>(2000); // Note: This variable must be kept thread-safe
		private readonly Dictionary<Type, List<Component>> componentDictionary = new Dictionary<Type, List<Component>>(10); // Note: This variable must be kept thread-safe
		private readonly List<Component> deadComponents = new List<Component>();

		[JsonProperty]
		private readonly Dictionary<int, Force> owningForces = new Dictionary<int, Force>();
		private AOHUD hud; // TODO: Why is this part of the world? Shouldn't it be part of the game?

		[JsonProperty]
		private Scenario scenario;

		private readonly AnimationSystem animationSystem;
		private readonly AnimationSystemLayer2 animationSystemLayer2;
		private readonly PhysicsSystem physicsSystem;
		private readonly RenderQuadTreeSystem renderQuadTreeSystem;
		private readonly PowerGridSystem powerGridSystem;
		private readonly PowerStorageSystem powerStorageSystem;
		private readonly PowerProductionSystem powerProductionSystem;
		internal readonly ConstructionSystem constructionSystem;
		private readonly LaserMinerSystem laserMinerSystem;
		private readonly AccumulationSystem accumulationSystem;
		private readonly LaserWeaponSystem laserWeaponSystem;
		private readonly ProjectileLauncherSystem projectileLauncherSystem;
		private readonly ProjectileSystem projectileSystem;
		private readonly FlockingSystem movementSystem;
		private readonly HitPointSystem hitPointSystem;
		private readonly SelectionSystem selectionSystem;
		private readonly ParticleEngine particleEngine;
		private readonly AutoHealSystem autoHealSystem;
		private readonly AIBasicSystem aiBasicSystem;
		private readonly AIStrafeSystem aiStrafeSystem;
		private readonly TargetingSystem targetingSystem;
		private readonly ScienceVesselSystem scienceVesselSystem;

		private MissionSystem missionSystem; // Created when world starts, instead of the world is created

		private bool paused;
		private float timeMultiplier = 1.0f;

		private bool isServer = true;
		private AONetwork network;

		private int nextComponentID = 0;
		private int nextForceID = 0;
		private int nextActorID = 0;


		private readonly List<Controller> controllers = new List<Controller>();
		private readonly List<Force> forces = new List<Force>();
		private bool gameOver = false;

		public event Action<bool> PauseToggledEvent;
		//public event Action<EntityEventArgs> StructureStartedEventPreAuth;
		//public event Action<EntityEventArgs> StructureStartedEventPostAuth;

		public event Action<int> EntityDied;


		internal World(AOGame game, EntityFactory entityFactory, UpgradeFactory upgradeFactory)
			: base(game)
		{
			theGame = game;
			this.entityFactory = entityFactory;
			this.upgradeFactory = upgradeFactory;

			network = new AONetwork(this);
			selectionSystem = new SelectionSystem(game, this);
			hud = new AOHUD(game, this, selectionSystem);

			animationSystem = new AnimationSystem(game, this);
			animationSystemLayer2 = new AnimationSystemLayer2(game, this, animationSystem);
			physicsSystem = new PhysicsSystem(game, this);
			renderQuadTreeSystem = new RenderQuadTreeSystem(game, this);
			hitPointSystem = new HitPointSystem(game, this);

			powerGridSystem = new PowerGridSystem(game, this);
			powerStorageSystem = new PowerStorageSystem(game, this);
			powerProductionSystem = new PowerProductionSystem(game, this, powerGridSystem);
			constructionSystem = new ConstructionSystem(game, this, powerGridSystem);
			laserMinerSystem = new LaserMinerSystem(game, this, powerGridSystem);
			accumulationSystem = new AccumulationSystem(game, this, 250);
			laserWeaponSystem = new LaserWeaponSystem(game, this, powerGridSystem, hitPointSystem);
			projectileLauncherSystem = new ProjectileLauncherSystem(game, this);
			projectileSystem = new ProjectileSystem(game, this, hitPointSystem);
			movementSystem = new FlockingSystem(game, this);
			particleEngine = new ParticleEngine(game, this);
			autoHealSystem = new AutoHealSystem(game, this, hitPointSystem);
			aiBasicSystem = new AIBasicSystem(game, this);
			aiStrafeSystem = new AIStrafeSystem(game, this, projectileLauncherSystem);
			targetingSystem = new TargetingSystem(game, this);
			scienceVesselSystem = new ScienceVesselSystem(game, this, powerGridSystem, hitPointSystem);

			awesomium = game.Awesomium;

			// TODO: Create this on the server, then send the size to the clients
			quadTree = new QuadTree<Position>(0, 0, 20000, 20000);

			targetingSystem.UpdateOrder = 1;


			animationSystem.DrawOrder = 1000;
			movementSystem.DrawOrder = 1001;
			constructionSystem.DrawOrder = 1002;
			laserMinerSystem.DrawOrder = 1025;
			particleEngine.DrawOrder = 1050;
			accumulationSystem.DrawOrder = 1100;
			powerGridSystem.DrawOrder = 1200;
			animationSystemLayer2.DrawOrder = 2000;

			game.Components.Add(animationSystem);
			game.Components.Add(animationSystemLayer2);
			game.Components.Add(physicsSystem);
			game.Components.Add(hud);
			game.Components.Add(renderQuadTreeSystem);
			game.Components.Add(powerStorageSystem);
			game.Components.Add(powerProductionSystem);
			game.Components.Add(powerGridSystem);
			game.Components.Add(constructionSystem);
			game.Components.Add(laserMinerSystem);
			game.Components.Add(accumulationSystem);
			game.Components.Add(laserWeaponSystem);
			game.Components.Add(projectileLauncherSystem);
			game.Components.Add(projectileSystem);
			game.Components.Add(movementSystem);
			game.Components.Add(hitPointSystem);
			game.Components.Add(selectionSystem);
			game.Components.Add(particleEngine);
			game.Components.Add(autoHealSystem);
			game.Components.Add(aiBasicSystem);
			game.Components.Add(aiStrafeSystem);
			game.Components.Add(targetingSystem);
			game.Components.Add(scienceVesselSystem);
		}


		protected override void Dispose(bool disposing)
		{
			network = null;
			awesomium = null;
			quadTree = null;

			Game.Components.Remove(hud);
			hud = null;

			// Remove all the systems from the component list
			var systems = this.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
			foreach (var system in systems)
			{
				if (system.GetValue(this) is GameComponent)
				{
					Game.Components.Remove((GameComponent)system.GetValue(this));
				}
			}

			base.Dispose(disposing);
		}


		public int EntityID
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
		[JsonIgnore]
		public bool Paused
		{
			get
			{
				return paused;
			}
			set
			{
				if (!gameOver)
				{
					// TODO: Handle this over the network. Should clients be allowed to pause the server?
					paused = value;
					OnPauseToggle();
					//awesomium.WebView.CallJavascriptFunction("", "SetPaused", new JSValue(paused));
				}
				else
				{
					Console.WriteLine("The game is over and can not be paused/unpaused");
				}
			}
		}


		public void OnPauseToggle()
		{
			if (PauseToggledEvent != null)
			{
				PauseToggledEvent(paused);
			}
		}


		public float TimeMultiplier
		{
			get
			{
				return timeMultiplier;
			}
			set
			{
				timeMultiplier = MathHelper.Clamp(value, 0.1f, 10);
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
		//public void Add(IIdentifiable obj)
		//{
		//    Add(obj, isServer);
		//}


		/// <summary>
		/// Adds the identifiable item to the game. If the game is a client, this will send a request to the server instead
		/// </summary>
		/// <param name="obj">The identifiable item to add to the game</param>
		/// <param name="isAuthoritative">If true, indicates that this instance of the game is a server OR that we have been told to do this by the server</param>
		//public void Add(IIdentifiable obj, bool isAuthoritative)
		//{
		//    Entity entity = obj as Entity;
		//    if (entity != null)
		//    {
		//        Add(entity, isAuthoritative);
		//    }
		//    else
		//    {
		//        Component component = obj as Component;
		//        if (component != null)
		//        {
		//            Add(component, isAuthoritative);
		//        }
		//        else
		//        {
		//            // We were unable to add the item to the game
		//            Debugger.Break();
		//        }
		//    }
		//}


		/// <summary>
		/// Adds the component to the game. If isAuthoritative is set to false, this will send a request to the server instead
		/// </summary>
		/// <param name="component">The component to add to the game</param>
		/// <param name="isAuthoritative">If true, indicates that this instance of the game is a server OR that we have been told to do this by the server</param>
		public void Add(Component component, bool isAuthoritative)
		{
			if (component != null)
			{

				if (component.EntityID == -1)
				{
					// Assign an ID
					component.EntityID = PopNextComponentID();
				}
				else if (isAuthoritative)
				{
					//lock(componentDictionary)
					//{
					//    if(componentDictionary.ContainsKey(component.EntityID))
					//    {
					//        // This component already exists
					//        return;
					//    }
					//}
				}

				if (isServer)
				{
					network.EnqueueMessage(new AOReflectiveOutgoingMessage(this.EntityID,
					                                                       "Add",
					                                                       new object[]{ component, true }));
				}
				else if (!isAuthoritative)
				{
					network.EnqueueMessage(new AOReflectiveOutgoingMessage(this.EntityID,
					                                                       "Add",
					                                                       new object[]{ component }));
				}

				// Tell my network object to listen to anything that may happen
				network.ListenToEvents(component);

				// Add this to a dictionary for quick ID-based lookups
				lock (entityDictionary)
				{
					if (entityDictionary.ContainsKey(component.EntityID))
					{
						entityDictionary[component.EntityID].Add(component);
					}
					else
					{
						entityDictionary.Add(component.EntityID, new List<Component>(){ component });
					}
					if (componentDictionary.ContainsKey(component.GetType()))
					{
						componentDictionary[component.GetType()].Add(component);
					}
					else
					{
						componentDictionary.Add(component.GetType(), new List<Component>(){ component });
					}
				}

				Position position = component as Position;
				if (position != null)
				{
					quadTree.Add(position);
				}


				Perishable perishable = component as Perishable;
				if (perishable != null)
				{
					perishable.Perishing += PerishableOnPerishing;
				}

			}
		}


		public int Create(String entityName, Force owningForce, JObject jsonValues)
		{
			return entityFactory.Create(this, entityName, owningForce, jsonValues);
		}

		public void StartUpgrade(int entityID, String upgradeName)
		{
			upgradeFactory.ApplyOnStartPayload(this, entityID, upgradeName);
			selectionSystem.UpdateContextMenu();
		}

		public void UpgradeComplete(int entityID)
		{
			upgradeFactory.ApplyOnCompletePayload(this, entityID);
		}

		public void ApplyUpgrade(int entityID, UpgradeTemplate upgrade)
		{
			upgradeFactory.ApplyPayload(this, entityID, upgrade.OnCompletePayload);
		}


		internal Dictionary<String, EntityTemplate> EntityTemplates
		{
			get
			{
				return entityFactory.templates;
			}
		}

		internal Dictionary<String, UpgradeTemplate> UpgradeTemplates
		{
			get
			{
				return upgradeFactory.Upgrades;
			}
		}

		internal UpgradeTemplate GetUpgrade(String name)
		{
			return upgradeFactory.Upgrades[name.ToLowerInvariant()];
		}


		private void PerishableOnPerishing(EntityPerishingEventArgs args)
		{
			if (EntityDied != null)
			{
				EntityDied(args.EntityID);
			}
			args.Perishable.Perishing -= PerishableOnPerishing;
		}


		private int PopNextComponentID()
		{
			return nextComponentID++;
		}


		/// <summary>
		/// Gets a list of entities that are intersecting with the search area
		/// </summary>
		/// <param name="rect">The search area</param>
		/// <returns>A list of entities that are intersecting with the search area</returns>
		public List<int> EntitiesInArea(Rectangle rect, bool onlySolids = false)
		{
			if (onlySolids)
			{
				return quadTree.GetObjects(rect).Where(x => !x.CanBuildOn || !x.CanConductThrough || x.CanMoveThrough).Select(x => x.EntityID).ToList();
			}
			else
			{
				return quadTree.GetObjects(rect).Select(x => x.EntityID).ToList();
			}
		}


		/// <summary>
		/// Gets a list of entities that are intersecting with the search area
		/// </summary>
		/// <param name="x">The search area's X coordinate</param>
		/// <param name="y">The search area's Y coordinate</param>
		/// <param name="w">The search area's Width</param>
		/// <param name="h">The search area's Height</param>
		/// <returns>A list of entities that are intersecting with the search area</returns>
		public List<int> EntitiesInArea(int x, int y, int w, int h, bool onlySolids = false)
		{
			return EntitiesInArea(new Rectangle(x, y, w, h), onlySolids);
		}


		/// <summary>
		/// Gets a list of entities that are intersecting with the search area
		/// </summary>
		/// <param name="x">The centre of the search area</param>
		/// <param name="y">The centre of the search area</param>
		/// <param name="radius">The radius of the search area</param>
		/// <returns>A list of entities that are intersecting with the search area</returns>
		public List<int> EntitiesInArea(int x, int y, int radius, bool onlySolids = false)
		{
			return EntitiesInArea(new Rectangle((int)(x - (radius / 2f)), (int)(y - (radius / 2f)), radius * 2, radius * 2), onlySolids);
		}


		/// <summary>
		/// Gets a list of entities that are intersecting with the search area
		/// </summary>
		/// <param name="location">The centre of the search area</param>
		/// <param name="radius">The radius of the search area</param>
		/// <returns>A list of entities that are intersecting with the search area</returns>
		public List<int> EntitiesInArea(Vector2 location, int radius, bool onlySolids = false)
		{
			return EntitiesInArea(new Rectangle((int)(location.X - radius), (int)(location.Y - radius), radius * 2, radius * 2), onlySolids);
		}


		/// <summary>
		/// Looks up a component by entityID
		/// This method is thread safe
		/// </summary>
		/// <param name="entityID">The entityID to look up</param>
		/// <returns>A list of components for the given entityID and type, or null if the entity is not found</returns>
		public IEnumerable<T> GetComponents<T>(int entityID) where T : Component
		{
			lock (entityDictionary)
			{
				if (entityDictionary.ContainsKey(entityID))
				{
					return entityDictionary[entityID].OfType<T>().ToList();
				}

				//Debugger.Break();
				return null;
			}
		}


		/// <summary>
		/// Looks up a component by entityID
		/// This method is thread safe
		/// </summary>
		/// <param name="referenceComponent">A component on the same entity</param>
		/// <returns>A list of T:Components that are attached to the same entity as the reference, or null if the entity is not found</returns>
		public IEnumerable<T> GetComponents<T>(Component referenceComponent) where T : Component
		{
			return GetComponents<T>(referenceComponent.EntityID);
		}


		/// <summary>
		/// Looks up a component by entityID
		/// This method is thread safe
		/// </summary>
		/// <param name="entityID">The entityID to look up</param>
		/// <returns>A single T:Component for the given entityID and type, or null if the entity is not found</returns>
		public Component GetComponent(int entityID, Type type)
		{
			lock (entityDictionary)
			{
				List<Component> entity;
				if (entityDictionary.TryGetValue(entityID, out entity))
				{
					IEnumerator<Component> matches = entity.Where(x => x.GetType() == type).GetEnumerator();
					Component firstMatch = null;
					if (matches.MoveNext())
					{
						firstMatch = matches.Current;

						if (matches.MoveNext())
						{
							// There were two or more records, error
							Debugger.Break();
						}
					}
					return firstMatch;
				}

				//Debugger.Break();
				return null;
			}
		}


		/// <summary>
		/// Looks up a component by entityID
		/// This method is thread safe
		/// </summary>
		/// <param name="entityID">The entityID to look up</param>
		/// <returns>A single T:Component for the given entityID and type, or null if the entity is not found</returns>
		public T GetComponent<T>(int entityID) where T : Component
		{
			T nullableComponent = GetNullableComponent<T>(entityID);
			if (nullableComponent == null)
			{
				// If you are expecting this, use GetNullableComponent instead
				Debugger.Break();
			}
			return nullableComponent;
		}


		/// <summary>
		/// Looks up a component by entityID
		/// This method is thread safe
		/// </summary>
		/// <param name="referenceComponent">A component on the same entity</param>
		/// <returns>A single T:Component that is attached to the same entity as the reference, or null if the entity is not found</returns>
		public T GetComponent<T>(Component referenceComponent) where T : Component
		{
			return GetComponent<T>(referenceComponent.EntityID);
		}


		/// <summary>
		/// Looks up a component by entityID
		/// This method is thread safe
		/// </summary>
		/// <param name="entityID">The entityID to look up</param>
		/// <returns>A single T:Component for the given entityID and type, or null if the entity is not found</returns>
		public T GetNullableComponent<T>(int entityID) where T : Component
		{
			lock (entityDictionary)
			{
				List<Component> entity;
				if (entityDictionary.TryGetValue(entityID, out entity))
				{
					IEnumerator<T> matches = entity.OfType<T>().GetEnumerator();
					T firstMatch = null;
					if (matches.MoveNext())
					{
						firstMatch = matches.Current;

						if (matches.MoveNext())
						{
							// There were two or more records, error
							Debugger.Break();
						}
					}
					return firstMatch;
				}

				//Debugger.Break();
				return null;
			}
		}


		/// <summary>
		/// Looks up a component by entityID
		/// This method is thread safe
		/// </summary>
		/// <param name="referenceComponent">A component on the same entity</param>
		/// <returns>A single T:Component that is attached to the same entity as the reference, or null if the entity is not found</returns>
		public T GetNullableComponent<T>(Component referenceComponent) where T : Component
		{
			return GetNullableComponent<T>(referenceComponent.EntityID);
		}


		/// <summary>
		/// Retreives a list of components with the type specified
		/// This method is thread safe
		/// </summary>
		/// <returns>Returns a list of components of the type specified</returns>
		public IEnumerable<T> GetComponents<T>() where T : Component
		{
			lock (componentDictionary)
			{
				if (componentDictionary.ContainsKey(typeof (T)))
				{
					return componentDictionary[typeof (T)].Select(x => x as T);
				}
				else
				{
					return new List<T>(0);
				}
			}
		}


		/// <summary>
		/// Looks up weapons by entityID
		/// This method is thread safe
		/// </summary>
		/// <param name="entityID">The entityID to look up</param>
		/// <returns>A list of IWeapons associated with the entity</returns>
		public List<IWeapon> GetWeapons(int entityID)
		{
			lock (entityDictionary)
			{
				if (entityDictionary.ContainsKey(entityID))
				{
					return entityDictionary[entityID].OfType<IWeapon>().ToList();
				}

				//Debugger.Break();
				return null;
			}
		}


		/// <summary>
		/// Looks up weapons by a reference component
		/// This method is thread safe
		/// </summary>
		/// <param name="referenceComponent">A component that is attached to the entity in question</param>
		/// <returns>A list of IWeapons associated with the reference component's entity</returns>
		public List<IWeapon> GetWeapons(Component referenceComponent)
		{
			return GetWeapons(referenceComponent.EntityID);
		}


		public void SetOwningForce(int entityID, Force force)
		{
			if (owningForces.ContainsKey(EntityID))
			{
				owningForces[entityID] = force;
			}
			else
			{
				owningForces.Add(entityID, force);
			}
		}


		public Force GetOwningForce(Component referenceComponent)
		{
			return GetOwningForce(referenceComponent.EntityID);
		}


		public Force GetOwningForce(int entityID)
		{
			return owningForces[entityID];
		}


		public IEnumerable<int> GetEntitiesOwnedBy(Force force)
		{
			return owningForces.Where(pair => pair.Value == force).Select(pair => pair.Key);
		}


		/// <summary>
		/// Gets the width of the configured playing area. This is not a hard boundary
		/// </summary>
		[JsonIgnore]
		public int MapWidth
		{
			get
			{
				return quadTree.QuadRect.Width;
			}
		}


		/// <summary>
		/// Gets the height of the configured playing area. This is not a hard boundary
		/// </summary>
		[JsonIgnore]
		public int MapHeight
		{
			get
			{
				return quadTree.QuadRect.Height;
			}
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


		[JsonIgnore]
		public Matrix WorldToScreenTransform
		{
			get
			{
				return hud.WorldToScreenTransform;
			}
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


		public Vector2 Scale(float x, float y)
		{
			return hud.Scale(x, y);
		}


		/// <summary>
		/// Gets whether this instance of the game is a server
		/// </summary>
		public bool IsServer
		{
			get
			{
				return isServer;
			}
			set
			{
				isServer = value;
			}
		}

		[JsonIgnore]
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


		// TODO: Not sure I should have this helper method here to pass a message to a system. Think about removing this
		public bool DrawQuadTree
		{
			get
			{
				return renderQuadTreeSystem.DrawQuadTree;
			}
			set
			{
				renderQuadTreeSystem.DrawQuadTree = value;
			}
		}


		//internal Dictionary<int, PowerGrid> PowerGrid
		//{
		//    get
		//    {
		//        return powerGrid;
		//    }
		//}



		/// <summary>
		/// Starts this instance of the game as a server
		/// </summary>
		public void StartServer(Scenario scenario)
		{
			awesomium.WebView.Source = (Environment.CurrentDirectory + @"\..\data\HUD\HUD.html").ToUri();
			if (this.scenario != null)
			{
				// Is this normal?
				Debugger.Break();
				this.scenario.End();
			}
			this.scenario = scenario;

			missionSystem = new MissionSystem((AOGame)Game, this, this.scenario);
			Game.Components.Add(missionSystem);

			isServer = true;
			this.scenario.Start((AOGame)Game, 1);
			SetScene(scenario.SceneName);
			network.StartGame();
		}



		///// <summary>
		///// Starts this instance of the game as a client
		///// </summary>
		//public void StartClient(int startingComponentID)
		//{
		//    nextComponentID = startingComponentID;
		//    isServer = false;
		//    //ScreenMan.SwitchScreens("Game");
		//    hud.FocusWorldPoint = new Vector2(MapWidth / 2f, MapHeight / 2f);

		//    AddController(new Controller(this, ControllerRole.Local, GetForcesOnTeam(Team.Team2)[0]));
		//}


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
				if (scenario != null)
				{
					scenario.Update(deltaTime);
				}


				// Update the Actors
				foreach (Controller actor in controllers)
				{
					actor.Update(deltaTime);
				}


				//// First tell people about dead entities
				//if (EntityDied != null && deadComponents.Count > 0)
				//{
				//    foreach(var deadEntity in deadComponents.Select(x => x.EntityID).Distinct())
				//    {
				//        EntityDied(deadEntity);
				//    }
				//}

				// Then clean up the dead entities
				foreach (Component deadComponent in deadComponents)
				{
					Position deadPosition = deadComponent as Position;
					if (deadPosition != null)
					{
						quadTree.Remove(deadPosition);
					}

					PowerGridNode powerGridNode = deadComponent as PowerGridNode;
					if (powerGridNode != null)
					{
						powerGridSystem.Disconnect(powerGridNode);
					}

					lock (entityDictionary)
					{
						if (entityDictionary[deadComponent.EntityID].Count == 1)
						{
							entityDictionary.Remove(deadComponent.EntityID);
							owningForces.Remove(deadComponent.EntityID);
						}
						else
						{
							entityDictionary[deadComponent.EntityID].Remove(deadComponent);
						}

						// I don't think it matters if we leave some empty lists behind
						componentDictionary[deadComponent.GetType()].Remove(deadComponent);
					}
				}
				deadComponents.Clear();

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
			// Draw the back of the HUD before we draw most everything else
			spriteBatch.Begin();
			selectionSystem.DrawBack(spriteBatch, Color.White);
			spriteBatch.End();

			base.Draw(gameTime);
		}



		/// <summary>
		/// Gets the force with the given ID, or null if the force is not found
		/// </summary>
		/// <param name="owningForceID"></param>
		/// <returns></returns>
		public Force GetForceByID(int owningForceID)
		{
			foreach (Force force in forces)
			{
				if (force.ID == owningForceID)
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
				network.EnqueueMessage(new AOReflectiveOutgoingMessage(EntityID,
				                                                       "AddForce",
				                                                       new object[]{ force }));
			}

			network.ListenToEvents(force);
		}


		public void AddController(Controller controller)
		{
			controllers.Add(controller);
			if (controller.Role == ControllerRole.Local && hud.LocalActor == null)
			{
				hud.LocalActor = controller;
			}

		}



		public void SetScene(String name)
		{
			theGame.sceneManager.SetScene(name);
		}


		public void SetFocus(Vector2 focus)
		{
			hud.FocusWorldPoint = focus;
		}


		[JsonIgnore]
		public int Width
		{
			get
			{
				return Game.GraphicsDevice.Viewport.Width;
			}
		}

		[JsonIgnore]
		public int Height
		{
			get
			{
				return Game.GraphicsDevice.Viewport.Height;
			}
		}

		[JsonIgnore]
		public QuadTree<Position> QuadTree
		{
			get
			{
				return quadTree;
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
			//bw.Write(powerGrid.Count);
			//foreach (int gridOwner in powerGrid.Keys)
			//{
			//    bw.Write(gridOwner);
			//}


			//bw.Write(entityDictionary.Count + componentDictionary.Count);
			//foreach (ISerializable serializableObj in componentDictionary.Values.Concat<ISerializable>(entityDictionary.Values))
			//{
			//    if (serializableObj == null || serializableObj.GetType().AssemblyQualifiedName == null)
			//    {
			//        Debugger.Break(); // Something is wrong here
			//        continue;
			//    }
			//    bw.Write(serializableObj.GetType().AssemblyQualifiedName);
			//    serializableObj.Serialize(bw);
			//}


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

			if (streamType != StreamType.GameData)
			{
				Debugger.Break();
			}


			int forceCount = br.ReadInt32();
			for (int iForce = 0; iForce < forceCount; iForce++)
			{
				Force force = new Force(br);
				AddForce(force);
				force.PostDeserializeLink(this);
			}


			//int powerGridCount = br.ReadInt32();
			//for (int iGrid = 0; iGrid < powerGridCount; iGrid++)
			//{
			//    int owningForceID = br.ReadInt32();
			//    powerGrid.Add(owningForceID, new PowerGrid(this));
			//}


			// Unpack all of the entities and components
			int entityCount = br.ReadInt32();
			List<ISerializable> createdEntities = new List<ISerializable>(entityCount);
			for (int iEntity = 0; iEntity < entityCount; iEntity++)
			{
				String assemName = br.ReadString();

				// Use reflection to make a new entity of... whatever type was sent to us
				Type t = Type.GetType(assemName);
				ConstructorInfo entityConstructor = t.GetConstructor(new Type[]{ typeof (BinaryReader) });
				Object obj = entityConstructor.Invoke(new object[]{ br });
				ISerializable serializableObj = obj as ISerializable;
				IIdentifiable identifiableObj = obj as IIdentifiable;

				if (serializableObj != null)
				{
					//createdEntities.Add(serializableObj);
					serializableObj.PostDeserializeLink(this);
				}

				//if(identifiableObj != null)
				//{
				//    Add(identifiableObj, true);
				//}
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


		public void DeleteComponents(int entityID)
		{
			foreach (var component in GetComponents<Component>(entityID))
			{
				deadComponents.Add(component);
			}
		}


		public void DeleteComponent(Component component)
		{
			deadComponents.Add(component);
		}


		public void GameOver(bool win)
		{
			if (win)
			{
				((AOGame)Game).ActiveProfile.ScenarioCompleted(scenario);
			}

			gameOver = true;
			awesomium.WebView.ExecuteJavascript(String.Format(CultureInfo.InvariantCulture, "GameOver({0})", win.ToString().ToLower()));
		}




		public void ExecuteAwesomiumJS(String js)
		{
			((AOGame)Game).ExecuteAwesomiumJS(js);
		}


		public void ConnectToPowerGrid(PowerGridNode powerNode)
		{
			powerGridSystem.ConnectToPowerGrid(powerNode);
		}
	}
}
