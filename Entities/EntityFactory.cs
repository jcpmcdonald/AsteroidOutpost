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

		private static Sprite asteroidSprite;

		public static void Init(World theWorld)
		{
			world = theWorld;
			asteroidSprite = new Sprite(File.OpenRead(@"..\Sprites\Asteroids.sprx"), world.GraphicsDevice);
		}

		public static List<Component> Create(String entityName, Dictionary<String, object> values)
		{
			int entityID = world.GetNextEntityID();
			List<Component> newComponents = new List<Component>(8);

			switch(entityName.ToLower())
			{
			case "asteroid":
				float angleStep = 360.0f / asteroidSprite.OrientationLookup.Count;
				Animator animator = new Animator(world,
				                                 entityID,
				                                 asteroidSprite,
				                                 (float)values["Sprite.Scale"],
				                                 (String)values["Sprite.Set"],
				                                 (String)values["Sprite.Animation"],
				                                 angleStep * GlobalRandom.Next(0, asteroidSprite.OrientationLookup.Count - 1));

				Position position = new Position(world, entityID,
				                                 (Vector2)values["Transpose.Position"],
				                                 (int)values["Transpose.Radius"]);

				newComponents.AddRange(new List<Component>{ animator, position });
				break;


			default:
				Console.WriteLine("Unrecognized Entity Name");
				Debugger.Break();
				break;
			}

			foreach(var component in newComponents)
			{
				world.AddComponent(component);
			}

			return newComponents;
		}
	}
}
