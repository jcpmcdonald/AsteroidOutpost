﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using AsteroidOutpost.Components;
using AsteroidOutpost.Entities;
using AsteroidOutpost.Screens;
using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;
using XNASpriteLib;

namespace AsteroidOutpost.Scenarios
{
	using System.Globalization;

	public abstract class Scenario
	{
		public String Name { get; set; }
		public String SceneName { get; set; }

		protected AOGame theGame;
		protected World world;
		protected int playerCount;

		protected Force friendlyForce;

		protected List<Mission> missions = new List<Mission>();
		protected List<Mission> deletedMissions = new List<Mission>();


		public List<Mission> Missions
		{
			get
			{
				return missions;
			}
		}

		public List<Mission> DeletedMissions
		{
			get
			{
				return deletedMissions;
			}
		}


		public virtual void Start(AOGame theGame, int playerCount)
		{
			this.theGame = theGame;
			this.world = theGame.World;
			this.playerCount = playerCount;

			theGame.World.EntityDied += World_EntityDied;

			//if(friendlyForce == null)
			//{
			//    // A friendly force is required
			//    Debugger.Break();
			//}
		}

		public virtual void End()
		{
			theGame.World.EntityDied -= World_EntityDied;
		}


		public abstract void Update(TimeSpan deltaTime);


		protected virtual void World_EntityDied(int deadID)
		{
			// Check to see if the player has lost
			//     The player loses if they have no more power producers or storage with power

			PowerProducer deadPowerProducer = world.GetNullableComponent<PowerProducer>(deadID);
			PowerStorage deadPowerStorage = world.GetNullableComponent<PowerStorage>(deadID);
			if(deadPowerProducer != null || deadPowerStorage != null)
			{
				// A power producer has been eliminated, check to see if there is still power out there
				bool producersExist = world.GetComponents<PowerProducer>().Any(p => p.EntityID != deadID && world.GetOwningForce(p) == friendlyForce && p.PowerProductionRate > 0 && world.GetNullableComponent<Constructing>(p) == null);
				bool storageExists = world.GetComponents<PowerStorage>().Any(p => p.EntityID != deadID && world.GetOwningForce(p) == friendlyForce && world.GetNullableComponent<Constructing>(p) == null && p.AvailablePower > 0);
				if (!producersExist && !storageExists)
				{
					// No power sources, it is impossible to recover. You are dead, or will be very soon
					world.GameOver(false);
				}
			}
		}


		/// <summary>
		/// Generates a random asteroid field with the given number of asteroids
		/// </summary>
		/// <param name="asteroidCount">The number of asteroids to create</param>
		protected virtual void GenerateAsteroidField(int asteroidCount)
		{
			Force asteroidForce = new Force(world, world.GetNextForceID(), 0, Team.Neutral);
			world.AddForce(asteroidForce);

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
			Random rand = new Random();
			for (int i = 0; i < asteroidCount; i++)
			{
				int minerals = (int)(61200 * rand.NextDouble());
				int x = 0;
				int y = 0;

				// Select an appropriate size for the mineral count
				int radius = 1;
				foreach (int indexedValue in asteroidSizeValueIndex)
				{
					if (minerals > indexedValue)
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
						List<int> nearbyEntities = world.EntitiesInArea(x, y, radius, true);
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


				world.Create("Asteroid", asteroidForce, new JObject{
					{ "Animator", new JObject{
						{ "Scale", scale },
						{ "CurrentSet", "Asteroid " + GlobalRandom.Next(1, 4) }
					}},
					{ "Position", new JObject{
						{ "Center", String.Format(CultureInfo.InvariantCulture, "{0}, {1}", x, y) },
						{ "Radius", radius }
					}},
					{ "Minable", new JObject{
						{ "Minerals", minerals },
						{ "StartingMinerals", minerals }
					}}
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
			Vector2 origin = new Vector2(world.MapWidth / 2.0f, world.MapHeight / 2.0f);
			Vector2 delta = Vector2.Zero;
			bool findNewHome = true;
			int attempts = 0;
			while (findNewHome)
			{
				findNewHome = false;

				// Expand the search as the number of attempts increase
				int tryDistance = 200 + (attempts * attempts * 2);
				delta = new Vector2(GlobalRandom.Next(tryDistance) + GlobalRandom.Next(tryDistance) - tryDistance,
				                    GlobalRandom.Next(tryDistance) + GlobalRandom.Next(tryDistance) - tryDistance);

				// Ensure no collisions
				// TODO: Look this radius up
				int radius = 40;
				List<int> nearbyEntities = world.EntitiesInArea(origin + delta, radius, true);
				foreach (int nearbyEntity in nearbyEntities)
				{
					Position position = world.GetComponent<Position>(nearbyEntity);
					if (position.IsIntersecting(origin + delta, radius))
					{
						findNewHome = true;
						attempts++;
						break;
					}
				}
			}

			// Create the solar station
			int solarStationID = world.Create("Solar Station", force, new JObject{
				{ "Position", new JObject{
					{ "Center", String.Format(CultureInfo.InvariantCulture, "{0}, {1}", origin.X + delta.X, origin.Y + delta.Y) },
				}},
			});

			// Remove the constructing component for the starting solar station
			world.DeleteComponent(world.GetComponent<Constructing>(solarStationID));

			// Fill it up with power
			PowerStorage storage = world.GetComponent<PowerStorage>(solarStationID);
			storage.AvailablePower = storage.MaxPower;

			// Hook it up to the grid
			PowerGridNode powerNode = world.GetComponent<PowerGridNode>(solarStationID);
			world.ConnectToPowerGrid(powerNode);

			// Return the starting location
			return origin + delta;
		}

	}
}
