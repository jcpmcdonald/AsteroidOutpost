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


		public static int Create(String entityName, Dictionary<String, object> values)
		{
			int entityID = world.GetNextEntityID();
			world.SetOwningForce(entityID, (Force)values["OwningForce"]);

			Sprite sprite = null;
			float spriteScale = (float)values["Sprite.Scale"];
			String spriteSet = (String)values["Sprite.Set"];
			String spriteAnimation = (String)values["Sprite.Animation"];
			int spriteOrientation = (int)values["Sprite.Orientation"];


			switch(entityName.ToLower())
			{
			case "asteroid":
				sprite = sprites[entityName.ToLower()];
				world.AddComponent(new Minable(world, entityID, (int)values["Minerals"]));
				break;

			case "solarstation":
				sprite = sprites[entityName.ToLower()];
				PowerProducer powerProducer = new PowerProducer(world, entityID, 10, 70);
				powerProducer.PowerLinkPointRelative = new Vector2(-1, -13);
				world.AddComponent(powerProducer);
				world.AddComponent(new Constructable(world, entityID, 200));
				break;

			case "powernode":
				sprite = sprites[entityName.ToLower()];
				PowerGridNode powerNode = new PowerGridNode(world, entityID, true);
				powerNode.PowerLinkPointRelative = new Vector2(0, -16);
				world.AddComponent(powerNode);
				world.AddComponent(new Constructable(world, entityID, 50));
				break;

			case "laserminer":
				sprite = sprites[entityName.ToLower()];
				world.AddComponent(new LaserMiner(world, entityID));
				world.AddComponent(new PowerGridNode(world, entityID, false));
				world.AddComponent(new Constructable(world, entityID, 200));
				break;
			}



			Position position = new Position(world, entityID,
			                                 (Vector2)values["Transpose.Position"],
			                                 (int)values["Transpose.Radius"]);
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
