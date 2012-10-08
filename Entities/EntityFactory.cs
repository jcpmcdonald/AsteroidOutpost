using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using AsteroidOutpost.Components;
using AsteroidOutpost.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNASpriteLib;

namespace AsteroidOutpost.Entities
{
	internal static class EntityFactory
	{
		private static World world;
		private static Dictionary<String, Sprite> sprites = new Dictionary<String, Sprite>();
		private static object loadSync = new object();


		public static void LoadContent(GraphicsDevice graphicsDevice)
		{
			// Note: The world doesn't exist yet, so don't use it here
			sprites.Add("asteroid", new Sprite(File.OpenRead(@"..\Sprites\Asteroids.sprx"), graphicsDevice));
			sprites.Add("solar station", new Sprite(File.OpenRead(@"..\Sprites\SolarStation.sprx"), graphicsDevice));
			sprites.Add("laser miner", new Sprite(File.OpenRead(@"..\Sprites\LaserMiner.sprx"), graphicsDevice));
			sprites.Add("power node", new Sprite(File.OpenRead(@"..\Sprites\PowerNode.sprx"), graphicsDevice));
			sprites.Add("laser tower", new Sprite(File.OpenRead(@"..\Sprites\LaserTower.sprx"), graphicsDevice));
			sprites.Add("missile tower", new Sprite(File.OpenRead(@"..\Sprites\MissileTower.sprx"), graphicsDevice));
			sprites.Add("space ship", new Sprite(File.OpenRead(@"..\Sprites\Spaceship128.sprx"), graphicsDevice));
			sprites.Add("beacon", new Sprite(File.OpenRead(@"..\Sprites\Beacon.sprx"), graphicsDevice));

			lock (LoadSync)
			{
				LoadProgress = 100;
			}
		}

		public static int LoadProgress { get; private set; }
		public static object LoadSync
		{
			get
			{
				return loadSync;
			}
			private set
			{
				loadSync = value;
			}
		}


		public static void Init(World theWorld)
		{
			world = theWorld;
		}


		public static int Create(String entityName, Dictionary<String, object> values)
		{
			int entityID = world.GetNextEntityID();
			world.SetOwningForce(entityID, (Force)values["OwningForce"]);

			Sprite sprite = null;
			float spriteScale = (float)values["Sprite.Scale"];
			String spriteSet = (String)values["Sprite.Set"];
			String spriteAnimation = (String)values["Sprite.Animation"];
			int spriteOrientation = (int)values["Sprite.Orientation"];


			Position position = new Position(world, entityID,
			                                 (Vector2)values["Transpose.Position"],
			                                 (int)values["Transpose.Radius"]);

			switch(entityName.ToLower())
			{
			case "asteroid":
				sprite = sprites[entityName.ToLower()];
				world.AddComponent(new EntityName(world, entityID, entityName));
				world.AddComponent(new Minable(world, entityID, (int)values["Minerals"]));
				break;

			case "solar station":
				sprite = sprites[entityName.ToLower()];
				world.AddComponent(new EntityName(world, entityID, entityName));
				PowerProducer powerProducer = new PowerProducer(world, entityID, 10, 70);
				powerProducer.PowerLinkPointRelative = new Vector2(-1, -13);
				world.AddComponent(powerProducer);
				world.AddComponent(new Constructable(world, entityID, 200));
				world.AddComponent(new HitPoints(world, entityID, 250));
				break;

			case "power node":
				sprite = sprites[entityName.ToLower()];
				world.AddComponent(new EntityName(world, entityID, entityName));
				PowerGridNode powerNode = new PowerGridNode(world, entityID, true);
				powerNode.PowerLinkPointRelative = new Vector2(0, -16);
				world.AddComponent(powerNode);
				world.AddComponent(new Constructable(world, entityID, 50));
				world.AddComponent(new HitPoints(world, entityID, 50));
				break;

			case "laser miner":
				sprite = sprites[entityName.ToLower()];
				world.AddComponent(new EntityName(world, entityID, entityName));
				LaserMiner laserMiner = new LaserMiner(world, entityID);
				world.AddComponent(laserMiner);
				world.AddComponent(new PowerGridNode(world, entityID, false));
				world.AddComponent(new Constructable(world, entityID, 200));
				Accumulator accumulator = new Accumulator(world, entityID, new Vector2(-5, -4), new Vector2(0, -26), Color.DarkGreen, 50);
				laserMiner.AccumulationEvent += accumulator.Accumulate;
				world.AddComponent(accumulator);
				world.AddComponent(new HitPoints(world, entityID, 150));
				break;

			case "laser tower":
				sprite = sprites[entityName.ToLower()];
				world.AddComponent(new EntityName(world, entityID, entityName));
				world.AddComponent(new PowerGridNode(world, entityID, false));
				world.AddComponent(new Constructable(world, entityID, 150));
				world.AddComponent(new HitPoints(world, entityID, 150));
				world.AddComponent(new LaserWeapon(world, entityID, 150, 15, Color.Red));
				break;

			case "missile tower":
				sprite = sprites[entityName.ToLower()];
				world.AddComponent(new EntityName(world, entityID, entityName));
				world.AddComponent(new PowerGridNode(world, entityID, false));
				world.AddComponent(new Constructable(world, entityID, 200));
				world.AddComponent(new HitPoints(world, entityID, 100));
				world.AddComponent(new MissileWeapon(world, entityID, 300, 25, 500, 5));
				break;

			case "space ship":
				sprite = sprites[entityName.ToLower()];
				world.AddComponent(new EntityName(world, entityID, entityName));
				world.AddComponent(new HitPoints(world, entityID, 150));
				world.AddComponent(new LaserWeapon(world, entityID, 150, 10, Color.DarkRed));
				world.AddComponent(new FleetMovementBehaviour(world, entityID));
				world.AddComponent(new Velocity(world, entityID, Vector2.Zero));
				break;

			case "beacon":
				sprite = sprites[entityName.ToLower()];
				world.AddComponent(new EntityName(world, entityID, entityName));
				world.AddComponent(new Spin(world, entityID, 90f));
				position.Solid = false;
				break;




			case "space ship (rotate frame)":
				sprite = sprites["space ship"];
				world.AddComponent(new EntityName(world, entityID, entityName));
				world.AddComponent(new Spin(world, entityID, 15f, true));
				world.AddComponent(new Velocity(world, entityID, Vector2.Zero));
				break;

			case "space ship (use frames only)":
				sprite = sprites["space ship"];
				world.AddComponent(new EntityName(world, entityID, entityName));
				world.AddComponent(new Spin(world, entityID, 15f, false));
				world.AddComponent(new Velocity(world, entityID, Vector2.Zero));
				break;

			default:
				Console.WriteLine("Unrecognized Entity Name");
				Debugger.Break();
				break;
			}



			world.AddComponent(position);

			if(sprite != null)
			{
				float angleStep = 360.0f / sprite.OrientationLookup.Count;
				Animator animator = new Animator(world,
				                                 entityID,
				                                 sprite,
				                                 spriteScale,
				                                 spriteSet,
				                                 spriteAnimation,
				                                 angleStep * (spriteOrientation % sprite.OrientationLookup.Count));
				world.AddComponent(animator);
			}

			return entityID;
		}

	}
}
