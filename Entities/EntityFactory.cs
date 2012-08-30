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
	static class EntityFactory
	{
		private static World world;

		private static Dictionary<String, Sprite> sprites = new Dictionary<String, Sprite>();

		public static void Init(World theWorld)
		{
			world = theWorld;
			sprites.Add("asteroid", new Sprite(File.OpenRead(@"..\Sprites\Asteroids.sprx"), world.GraphicsDevice));
			sprites.Add("solarstation", new Sprite(File.OpenRead(@"..\Sprites\SolarStation.sprx"), world.GraphicsDevice));
			sprites.Add("laserminer", new Sprite(File.OpenRead(@"..\Sprites\LaserMiner.sprx"), world.GraphicsDevice));
		}

		public static int Create(String entityName, Dictionary<String, object> values)
		{
			int entityID = world.GetNextEntityID();
			List<Component> newComponents = new List<Component>(8);
			world.SetOwningForce(entityID, (Force)values["OwningForce"]);

			switch(entityName.ToLower())
			{
			case "asteroid":
			{
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

				newComponents.AddRange(new List<Component>{ animator, position });
				break;
			}
			case "solarstation":
			{
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

				Constructable constructable = new Constructable(world, entityID, 200);

				newComponents.AddRange(new List<Component>{ animator, position, powerProducer, constructable });
				break;
			}
			case "laserminer":
			{
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

				PowerGridNode powerNode = new PowerGridNode(world, entityID, false);
				Constructable constructable = new Constructable(world, entityID, 200);

				newComponents.AddRange(new List<Component>{ animator, position, powerNode, constructable });
				break;
			}

			default:
				Console.WriteLine("Unrecognized Entity Name");
				Debugger.Break();
				break;
			}


			foreach(var component in newComponents)
			{
				world.AddComponent(component);
			}

			return entityID;
		}
	}
}
