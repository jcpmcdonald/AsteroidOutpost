using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using AsteroidOutpost.Components;
using AsteroidOutpost.Screens;
using Microsoft.Xna.Framework;
using XNASpriteLib;

namespace AsteroidOutpost.Entities
{
	internal static class EntityFactory
	{
		private static World world;

		private static Dictionary<String, Sprite> sprites = new Dictionary<String, Sprite>();


		public static void Init(World theWorld)
		{
			world = theWorld;
			sprites.Add("asteroid", new Sprite(File.OpenRead(@"..\Sprites\Asteroids.sprx"), world.GraphicsDevice));
			sprites.Add("solarstation", new Sprite(File.OpenRead(@"..\Sprites\SolarStation.sprx"), world.GraphicsDevice));
			sprites.Add("laserminer", new Sprite(File.OpenRead(@"..\Sprites\LaserMiner.sprx"), world.GraphicsDevice));
			sprites.Add("powernode", new Sprite(File.OpenRead(@"..\Sprites\PowerNode.sprx"), world.GraphicsDevice));
		}


		public static int CreateAsteroid(Dictionary<String, object> values)
		{
			int entityID = world.GetNextEntityID();
			world.SetOwningForce(entityID, (Force)values["OwningForce"]);

			Sprite asteroidSprite = sprites["asteroid"];
			float angleStep = 360.0f / asteroidSprite.OrientationLookup.Count;

			Animator animator = new Animator(world,
			                                 entityID,
			                                 asteroidSprite,
			                                 (float)values["Sprite.Scale"],
			                                 (String)values["Sprite.Set"],
			                                 (String)values["Sprite.Animation"],
			                                 angleStep * ((int)values["Sprite.Orientation"] % asteroidSprite.OrientationLookup.Count));

			Position position = new Position(world, entityID,
			                                 (Vector2)values["Transpose.Position"],
			                                 (int)values["Transpose.Radius"]);

			Minable minable = new Minable(world, entityID, (int)values["Minerals"]);


			world.AddComponent(animator);
			world.AddComponent(position);
			world.AddComponent(minable);
			return entityID;
		}


		public static int CreateSolarStation(Dictionary<String, object> values)
		{
			int entityID = world.GetNextEntityID();
			world.SetOwningForce(entityID, (Force)values["OwningForce"]);

			Sprite solarStationSprite = sprites["solarstation"];
			float angleStep = 360.0f / solarStationSprite.OrientationLookup.Count;
			float spriteScale = (float)values["Sprite.Scale"];
			String spriteSet = (String)values["Sprite.Set"];
			String spriteAnimation = (String)values["Sprite.Animation"];
			int spriteOrientation = (int)values["Sprite.Orientation"];

			Animator animator = new Animator(world,
			                                 entityID,
			                                 solarStationSprite,
			                                 spriteScale,
			                                 spriteSet,
			                                 spriteAnimation,
			                                 angleStep * (spriteOrientation % solarStationSprite.OrientationLookup.Count));

			Position position = new Position(world, entityID,
			                                 (Vector2)values["Transpose.Position"],
			                                 (int)values["Transpose.Radius"]);

			PowerProducer powerProducer = new PowerProducer(world, entityID, 10, 70);
			powerProducer.PowerLinkPointRelative = new Vector2(-1, -13);
			Constructable constructable = new Constructable(world, entityID, 200);

			world.AddComponent(animator);
			world.AddComponent(position);
			world.AddComponent(powerProducer);
			world.AddComponent(constructable);
			return entityID;
		}


		public static int CreateLaserMiner(Dictionary<String, object> values)
		{
			int entityID = world.GetNextEntityID();
			world.SetOwningForce(entityID, (Force)values["OwningForce"]);

			Sprite laserMinerSprite = sprites["laserminer"];
			float angleStep = 360.0f / laserMinerSprite.OrientationLookup.Count;
			float spriteScale = (float)values["Sprite.Scale"];
			String spriteSet = (String)values["Sprite.Set"];
			String spriteAnimation = (String)values["Sprite.Animation"];
			int spriteOrientation = (int)values["Sprite.Orientation"];

			Animator animator = new Animator(world,
			                                 entityID,
			                                 laserMinerSprite,
			                                 spriteScale,
			                                 spriteSet,
			                                 spriteAnimation,
			                                 angleStep * (spriteOrientation % laserMinerSprite.OrientationLookup.Count));

			Position position = new Position(world, entityID,
			                                 (Vector2)values["Transpose.Position"],
			                                 (int)values["Transpose.Radius"]);

			LaserMiner laserMiner = new LaserMiner(world, entityID);
			PowerGridNode powerNode = new PowerGridNode(world, entityID, false);
			Constructable constructable = new Constructable(world, entityID, 200);

			world.AddComponent(animator);
			world.AddComponent(position);
			world.AddComponent(powerNode);
			world.AddComponent(constructable);
			world.AddComponent(laserMiner);
			return entityID;
		}


		public static int CreatePowerNode(Dictionary<String, object> values)
		{
			int entityID = world.GetNextEntityID();
			world.SetOwningForce(entityID, (Force)values["OwningForce"]);

			Sprite sprite = sprites["powernode"];
			float angleStep = 360.0f / sprite.OrientationLookup.Count;
			float spriteScale = (float)values["Sprite.Scale"];
			String spriteSet = (String)values["Sprite.Set"];
			String spriteAnimation = (String)values["Sprite.Animation"];
			int spriteOrientation = (int)values["Sprite.Orientation"];

			Animator animator = new Animator(world,
			                                 entityID,
			                                 sprite,
			                                 spriteScale,
			                                 spriteSet,
			                                 spriteAnimation,
			                                 angleStep * (spriteOrientation % sprite.OrientationLookup.Count));

			Position position = new Position(world, entityID,
			                                 (Vector2)values["Transpose.Position"],
			                                 (int)values["Transpose.Radius"]);

			PowerGridNode powerNode = new PowerGridNode(world, entityID, true);
			powerNode.PowerLinkPointRelative = new Vector2(0, -16);
			Constructable constructable = new Constructable(world, entityID, 50);

			world.AddComponent(animator);
			world.AddComponent(position);
			world.AddComponent(powerNode);
			world.AddComponent(constructable);
			return entityID;
		}

	}
}
