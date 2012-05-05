using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteroidOutpost.Entities;
using AsteroidOutpost.Entities.Structures;
using AsteroidOutpost.Screens;
using Microsoft.Xna.Framework;

namespace AsteroidOutpost.Scenarios
{
	public abstract class Scenario
	{
		private String name;
		private String author;

		protected AsteroidOutpostScreen theGame;
		protected int playerCount;


		protected Scenario(AsteroidOutpostScreen theGame, int playerCount)
		{
			this.theGame = theGame;
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
			Force asteroidForce = new Force(theGame, theGame.GetNextForceID(), 0, Team.Neutral);
			theGame.AddForce(asteroidForce);

			// create a random asteroid field
			int minX = 0;		// MapX;?
			int minY = 0;		// MapY;?
			int maxX = theGame.MapWidth;
			int maxY = theGame.MapHeight;
			Random rand = new Random();
			Asteroid asteroid;
			for (int i = 0; i < asteroidCount; i++)
			{
				int minerals = (int)(61200 * rand.NextDouble());
				int x = 0;
				int y = 0;
				asteroid = null;

				bool findNewHome = true;
				while (findNewHome)
				{
					x = (int)((maxX - minX) * rand.NextDouble()) + minX;
					// Prefer y's that are closer to the x value
					//y = (int)((maxY - minY) * rand.NextDouble()) + minY;
					y = x + (int)(((rand.NextDouble() * 10.0) - 5.0) * ((rand.NextDouble() * 10.0) - 5.0) * (theGame.MapWidth / 30.0));

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
						if (asteroid == null)
						{
							asteroid = new Asteroid(theGame, theGame, asteroidForce, new Vector2(x, y), minerals);
						}
						else
						{
							asteroid.Position.Center = new Vector2(x, y);
						}

						// Make sure we aren't intersecting with other asteroids
						List<Entity> nearbyEntities = theGame.EntitiesInArea(asteroid.Rect);
						foreach (Entity nearbyEntity in nearbyEntities)
						{
							if (asteroid.Radius.IsIntersecting(nearbyEntity.Radius))
							{
								findNewHome = true;
							}
						}
					}
				}
				theGame.Add(asteroid);
			}
		}


		/// <summary>
		/// Make a solar station at a random position near the origin
		/// </summary>
		/// <param name="force">The owning force</param>
		/// <returns>Returns a location that the camera should be centered on</returns>
		protected virtual Vector2 CreateStartingBase(Force force)
		{
			SolarStation startingStation = new SolarStation(theGame, theGame, force, Vector2.Zero);
			startingStation.StartConstruction();
			startingStation.IsConstructing = false;


			Vector2 origin = new Vector2(theGame.MapWidth / 2.0f, theGame.MapHeight / 2.0f);
			bool findNewHome = true;
			int attempts = 0;
			while (findNewHome)
			{
				int tryDistance = 200 + (attempts * attempts * 2);
				Vector2 delta = new Vector2(GlobalRandom.Next(tryDistance) + GlobalRandom.Next(tryDistance) - tryDistance,
											GlobalRandom.Next(tryDistance) + GlobalRandom.Next(tryDistance) - tryDistance);
				startingStation.Position.Center = origin + delta;
				findNewHome = false;

				// Ensure no collisions
				List<Entity> nearbyEntities = theGame.EntitiesInArea(startingStation.Rect);
				foreach (Entity nearbyEntity in nearbyEntities)
				{
					if (startingStation.Radius.IsIntersecting(nearbyEntity.Radius))
					{
						findNewHome = true;
						attempts++;
						continue;
					}
				}
			}

			theGame.Add(startingStation);

			return startingStation.Position.Center;
		}

	}
}
