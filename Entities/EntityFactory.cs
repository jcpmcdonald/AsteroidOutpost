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

		public static List<Component> Create(String entityName)
		{
			int entityID = world.GetNextEntityID();
			List<Component> newComponents = new List<Component>(8);

			switch(entityName.ToLower())
			{
			case "asteroid":
				Animator animator = new Animator(world, entityID, asteroidSprite);
				animator.SpriteAnimator.CurrentSet = "Asteroid " + GlobalRandom.Next(1, 4);
				float angleStep = 360.0f / asteroidSprite.OrientationLookup.Count;
				animator.SpriteAnimator.CurrentOrientation = (angleStep * GlobalRandom.Next(0, asteroidSprite.OrientationLookup.Count - 1)).ToString();

				Position position = new Position(world, entityID, new Vector2(world.MapWidth / 2.0f, world.MapHeight / 2.0f), 40);

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
