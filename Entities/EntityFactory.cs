﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using AsteroidOutpost.Components;
using AsteroidOutpost.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using XNASpriteLib;
using fastJSON;

namespace AsteroidOutpost.Entities
{
	internal static class EntityFactory
	{
		private static World world;
		private static Dictionary<String, Sprite> sprites = new Dictionary<String, Sprite>();
		private static object loadSync = new object();


		private static EntityTemplate asteroidTemplate;


		public static void LoadContent(GraphicsDevice graphicsDevice)
		{
			String entityJson = File.ReadAllText(@"..\entities\entity.json");
			EntityTemplate baseEntity = JsonConvert.DeserializeObject<EntityTemplate>(entityJson);

			asteroidTemplate = (EntityTemplate)baseEntity.Clone();
			String asteroidJson = File.ReadAllText(@"..\entities\asteroid.json");
			asteroidTemplate.ExtendRecursivelyWith(asteroidJson);

			//Console.WriteLine(JsonConvert.SerializeObject(baseEntity, Formatting.Indented));
			Console.WriteLine(JsonConvert.SerializeObject(asteroidTemplate, Formatting.Indented));

			//List<Component> astComponents = asteroidTemplate.Instantiate(-5, );


			//JSON.Instance.Parameters.EnableAnonymousTypes = true;

			//EntityTemplate baseEntity = new EntityTemplate();
			//String json = File.ReadAllText(@"..\entities\entity.json");
			//JSON.Instance.FillObject(baseEntity, json);
			//Console.WriteLine(JSON.Instance.Beautify(JSON.Instance.ToJSON(baseEntity)));


			//EntityTemplate asteroidTemplate = new EntityTemplate();
			//asteroidTemplate.SpriteName = "asteroid";
			//asteroidTemplate.componentDefaults.Add("Minable", new Dictionary<string, object>());
			//asteroidTemplate.componentDefaults["Minable"].Add("Minerals", 130);
			//asteroidTemplate.componentDefaults["Minable"].Add("StartingMinerals", 130);
			//String json = JSON.Instance.Beautify(JSON.Instance.ToJSON(asteroidTemplate));
			//Console.WriteLine(json);

			// Note: The world doesn't exist yet, so don't use it here
			sprites.Add("asteroid", new Sprite(File.OpenRead(@"..\Sprites\Asteroids.sprx"), graphicsDevice));
			sprites.Add("solar station", new Sprite(File.OpenRead(@"..\Sprites\SolarStation.sprx"), graphicsDevice));
			sprites.Add("laser miner", new Sprite(File.OpenRead(@"..\Sprites\LaserMiner.sprx"), graphicsDevice));
			sprites.Add("power node", new Sprite(File.OpenRead(@"..\Sprites\PowerNode.sprx"), graphicsDevice));
			sprites.Add("laser tower", new Sprite(File.OpenRead(@"..\Sprites\LaserTower.sprx"), graphicsDevice));
			sprites.Add("missile tower", new Sprite(File.OpenRead(@"..\Sprites\MissileTower.sprx"), graphicsDevice));
			sprites.Add("missile", new Sprite(File.OpenRead(@"..\Sprites\Missile.sprx"), graphicsDevice));
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

		public static int Create(String entityName, Force owningForce, String jsonValues)
		{
			int entityID = world.GetNextEntityID();
			world.SetOwningForce(entityID, owningForce);

			var components = asteroidTemplate.Instantiate(entityID, jsonValues, sprites);
			foreach(var component in components)
			{
				world.AddComponent(component);
			}

			return entityID;
		}

		public static int Create(String entityName, Dictionary<String, object> values)
		{
			int entityID = world.GetNextEntityID();
			world.SetOwningForce(entityID, (Force)values["OwningForce"]);

			Sprite sprite = null;
			float spriteScale = (float)values["Sprite.Scale"];
			String spriteSet = (String)values["Sprite.Set"];
			String spriteAnimation = (String)values["Sprite.Animation"];
			float spriteOrientation = (float)values["Sprite.Orientation"];
			bool spriteRotateFrame = values.ContainsKey("Sprite.RotateFrame");


			Position position = new Position(entityID,
			                                 (Vector2)values["Transpose.Position"],
			                                 (int)values["Transpose.Radius"]);

			switch(entityName.ToLower())
			{
			case "asteroid":
				var components = asteroidTemplate.Instantiate(entityID, values, sprites);
				foreach(var component in components)
				{
					world.AddComponent(component);
				}

//                sprite = sprites[entityName.ToLower()];
//                world.AddComponent(new EntityName(entityID, entityName));
//                //world.AddComponent(new Minable(world, entityID, (int)values["Minerals"]));
//                Minable minable = new Minable(entityID);
//                JSON.Instance.FillObject(minable, String.Format( @"{{
//						""Minerals"" : {0},
//						""StartingMinerals"" : {0},
//					}}", (int)values["Minerals"]));
//                world.AddComponent(minable);
				break;

			case "solar station":
				sprite = sprites[entityName.ToLower()];
				world.AddComponent(new EntityName(entityID, entityName));
				PowerProducer powerProducer = new PowerProducer(world, entityID, 10, 70);
				powerProducer.PowerLinkPointRelative = new Vector2(-1, -13);
				world.AddComponent(powerProducer);
				world.AddComponent(new Constructible(entityID, 200));
				world.AddComponent(new HitPoints(entityID, 50));
				break;

			case "power node":
				sprite = sprites[entityName.ToLower()];
				world.AddComponent(new EntityName(entityID, entityName));
				PowerGridNode powerNode = new PowerGridNode(world, entityID, true);
				powerNode.PowerLinkPointRelative = new Vector2(0, -16);
				world.AddComponent(powerNode);
				world.AddComponent(new Constructible(entityID, 50));
				world.AddComponent(new HitPoints(entityID, 50));
				break;

			case "laser miner":
				sprite = sprites[entityName.ToLower()];
				world.AddComponent(new EntityName(entityID, entityName));
				LaserMiner laserMiner = new LaserMiner(entityID);
				world.AddComponent(laserMiner);
				world.AddComponent(new PowerGridNode(world, entityID, false));
				world.AddComponent(new Constructible(entityID, 200));
				Accumulator accumulator = new Accumulator(entityID, new Vector2(-5, -4), new Vector2(0, -26), Color.DarkGreen, 50);
				laserMiner.AccumulationEvent += accumulator.Accumulate;
				world.AddComponent(accumulator);
				world.AddComponent(new HitPoints(entityID, 150));
				break;

			case "laser tower":
				sprite = sprites[entityName.ToLower()];
				world.AddComponent(new EntityName(entityID, entityName));
				world.AddComponent(new PowerGridNode(world, entityID, false));
				world.AddComponent(new Constructible(entityID, 150));
				world.AddComponent(new HitPoints(entityID, 150));
				world.AddComponent(new LaserWeapon(entityID, 150, 15, Color.Green));
				break;

			case "missile tower":
				sprite = sprites[entityName.ToLower()];
				world.AddComponent(new EntityName(entityID, entityName));
				world.AddComponent(new PowerGridNode(world, entityID, false));
				world.AddComponent(new Constructible(entityID, 200));
				world.AddComponent(new HitPoints(entityID, 100));
				world.AddComponent(new MissileWeapon(entityID, 300, 25, 1500, 50));
				break;

			case "missile":
				sprite = sprites[entityName.ToLower()];
				world.AddComponent(new EntityName(entityID, entityName));
				world.AddComponent(new HitPoints(entityID, 100));
				world.AddComponent(new MissileProjectile(entityID,
				                                         25,
				                                         50,
				                                         100,
				                                         values.ContainsKey("TargetEntityID") ? (int?)values["TargetEntityID"] : null));
				world.AddComponent(new Velocity(entityID, Vector2.Zero));
				break;

			case "space ship":
				sprite = sprites[entityName.ToLower()];
				world.AddComponent(new EntityName(entityID, entityName));
				world.AddComponent(new HitPoints(entityID, 150));
				world.AddComponent(new LaserWeapon(entityID, 150, 10, Color.DarkRed));
				world.AddComponent(new FleetMovementBehaviour(entityID));
				world.AddComponent(new Velocity(entityID, Vector2.Zero));
				break;

			case "beacon":
				sprite = sprites[entityName.ToLower()];
				world.AddComponent(new EntityName(entityID, entityName));
				world.AddComponent(new Spin(entityID, 90f));
				position.Solid = false;
				break;




			case "(rotate frame)":
				sprite = sprites["missile"];
				world.AddComponent(new EntityName(entityID, entityName));
				world.AddComponent(new Spin(entityID, 15f, true));
				world.AddComponent(new Velocity(entityID, Vector2.Zero));
				break;

			case "(use frames only)":
				sprite = sprites["missile"];
				world.AddComponent(new EntityName(entityID, entityName));
				world.AddComponent(new Spin(entityID, 15f, false));
				world.AddComponent(new Velocity(entityID, Vector2.Zero));
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
				Animator animator = new Animator(entityID,
				                                 sprite,
				                                 spriteScale,
				                                 spriteSet,
				                                 spriteAnimation,
				                                 //spriteOrientation,
				                                 angleStep * (spriteOrientation % sprite.OrientationLookup.Count),
				                                 spriteRotateFrame);

				world.AddComponent(animator);
			}

			return entityID;
		}

	}
}
