using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AsteroidOutpost.Components;
using AsteroidOutpost.Entities;
using AsteroidOutpost.Screens;
using Microsoft.Xna.Framework;
using XNASpriteLib;

namespace AsteroidOutpost.Scenarios
{
	public abstract class Scenario
	{
		private String name;
		private String author;

		protected AOGame theGame;
		protected World world;
		protected int playerCount;


		protected Scenario(AOGame theGame, int playerCount)
		{
			this.theGame = theGame;
			this.world = theGame.World;
			this.playerCount = playerCount;
		}


		public abstract void Start();
		public void End() { }

		public abstract void Update(TimeSpan deltaTime);



		/// <summary>
		/// Generates a random asteroid field with the given number of asteroids
		/// </summary>
		/// <param name="asteroidCount">The number of asteroids to create</param>
		protected virtual void GenerateAsteroidField(int asteroidCount)
		{
			//Force asteroidForce = new Force(world, world.GetNextForceID(), 0, Team.Neutral);
			//world.AddForce(asteroidForce);

			//EntityFactory.Create("Asteroid", new Dictionary<String, object>(){
			//    { "Sprite.Set", "Asteroid " + GlobalRandom.Next(1, 4) },
			//    { "Sprite.Animation", null },
			//    { "Transpose.Position", new Vector2(world.MapWidth / 2.0f, world.MapHeight / 2.0f) },
			//    { "Transpose.Radius", 40 },
			//    { "Minerals", 500 }
			//});

			//int entityID = world.GetNextEntityID();
			//Sprite asteroidSprite = new Sprite(File.OpenRead(@"..\Sprites\Asteroids.sprx"), world.GraphicsDevice);
			//Animator animator = new Animator(world, entityID, asteroidSprite);
			//animator.SpriteAnimator.CurrentSet = "Asteroid " + GlobalRandom.Next(1, 4);
			//animator.SpriteAnimator.CurrentOrientation = "0";//(angleStep * GlobalRandom.Next(0, sprite.OrientationLookup.Count - 1)).ToString();

			////world.AddComponent(new Position(world, entityID, new Vector2(world.MapWidth / 2.0f, world.MapHeight / 2.0f), 40));
			//world.AddComponent(new Position(world, entityID, new Vector2(0, 0), 40));
			//world.AddComponent(animator);

			// create a random asteroid field
			int[] asteroidSizeValueIndex = new[]{ 200, 400, 800, 1600, 3200, 6400, 12800, 25600, 51200 };
			int minX = 0; // MapX;?
			int minY = 0; // MapY;?
			int maxX = world.MapWidth;
			int maxY = world.MapHeight;
			int startingMinerals;
			Random rand = new Random();
			for (int i = 0; i < asteroidCount; i++)
			{
				int minerals = (int)(61200 * rand.NextDouble());
				int x = 0;
				int y = 0;

				// Select an appropriate size for the mineral count
				int radius = 1;
				foreach(int indexedValue in asteroidSizeValueIndex)
				{
					if(minerals > indexedValue)
					{
						radius++;
					}
				}
				radius = radius * 10;
				radius = (int)((radius / 100.0f) * 25f);
				float scale = (radius / 25f) * 0.5f;

				bool findNewHome = true;
				while (findNewHome)
				{
					x = (int)((maxX - minX) * rand.NextDouble()) + minX;
					// Prefer y's that are closer to the x value
					//y = (int)((maxY - minY) * rand.NextDouble()) + minY;
					y = x + (int)(((rand.NextDouble() * 10.0) - 5.0) * ((rand.NextDouble() * 10.0) - 5.0) * (world.MapWidth / 30.0));

					findNewHome = false;
					if (y < minY || y > maxY)
					{
						// Due to the way we calculate the location of Y, it could be off the map
						// if it is off the map, we want to make an entirely new location for the asteroid in order to
						// avoid placement bias
						findNewHome = true;
					}
					else
					{
						//if (asteroid == null)
						//{
						//    asteroid = new Asteroid(world, world, asteroidForce, new Vector2(x, y), minerals);
						//}
						//else
						//{
						//    asteroid.Position.Center = new Vector2(x, y);
						//}


						// Make sure we aren't intersecting with other asteroids
						List<int> nearbyEntities = world.EntitiesInArea(x, y, radius);
						foreach (int nearbyEntity in nearbyEntities)
						{
							Position position = world.GetComponent<Position>(nearbyEntity);
							if (position.IsIntersecting(new Vector2(x, y), radius))
							{
								findNewHome = true;
							}
						}
					}
				}
				EntityFactory.Create("Asteroid",
				                     new Dictionary<String, object>(){
					                     { "Sprite.Scale", scale },
					                     { "Sprite.Set", "Asteroid " + GlobalRandom.Next(1, 4) },
					                     { "Sprite.Animation", null },
					                     { "Transpose.Position", new Vector2(x, y) },
					                     { "Transpose.Radius", radius },
					                     { "Minerals", minerals }
				                     });
			}
		}


		/// <summary>
		/// Make a solar station at a random position near the origin
		/// </summary>
		/// <param name="force">The owning force</param>
		/// <returns>Returns a location that the camera should be centered on</returns>
		protected virtual Vector2 CreateStartingBase(Force force)
		{
			//SolarStation startingStation = new SolarStation(world, world, force, Vector2.Zero);
			//startingStation.StartConstruction();
			//startingStation.IsConstructing = false;


			//Vector2 origin = new Vector2(world.MapWidth / 2.0f, world.MapHeight / 2.0f);
			//bool findNewHome = true;
			//int attempts = 0;
			//while (findNewHome)
			//{
			//    int tryDistance = 200 + (attempts * attempts * 2);
			//    Vector2 delta = new Vector2(GlobalRandom.Next(tryDistance) + GlobalRandom.Next(tryDistance) - tryDistance,
			//                                GlobalRandom.Next(tryDistance) + GlobalRandom.Next(tryDistance) - tryDistance);
			//    startingStation.Position.Center = origin + delta;
			//    findNewHome = false;

			//    // Ensure no collisions
			//    List<Entity> nearbyEntities = world.EntitiesInArea(startingStation.Rect);
			//    foreach (Entity nearbyEntity in nearbyEntities)
			//    {
			//        if (startingStation.Position.IsIntersecting(nearbyEntity.Position))
			//        {
			//            findNewHome = true;
			//            attempts++;
			//            continue;
			//        }
			//    }
			//}

			//world.Add(startingStation);

			//return startingStation.Position.Center;
			return new Vector2(world.MapWidth / 2.0f, world.MapHeight / 2.0f);
		}

	}
}
