using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using AsteroidOutpost.Entities;
using AsteroidOutpost.Entities.Structures;
using AsteroidOutpost.Entities.Units;
using AsteroidOutpost.Networking;
using AsteroidOutpost.Screens;
using Microsoft.Xna.Framework;

namespace AsteroidOutpost.Scenarios
{
	public class RandomScenario : Scenario
	{
		private TimeSpan elapsedTime;
		private int sequence = 0;


		public RandomScenario(AsteroidOutpostScreen theGame) : base(theGame)
		{
		}


		public override void Start()
		{
			elapsedTime = new TimeSpan(0);

			// Set up some forces and actors
			Force localForce = new Force(theGame, theGame.GetNextForceID(), 1000, Team.Team1);
			Actor localActor = new Actor(theGame, theGame.GetNextActorID(), ActorRole.Local, localForce);
			theGame.AddForce(localForce);
			theGame.AddActor(localActor);

			Force aiForce = new Force(theGame, theGame.GetNextForceID(), 1000, Team.AI);
			Actor aiActor = new AIActor(theGame, theGame, aiForce);
			theGame.AddForce(aiForce);
			theGame.AddActor(aiActor);

			// Send both actors to the clients
			// Make a copy of the actor so that we can set the role to Remote
			Actor localActorCopy = new Actor(theGame, localActor.ID, ActorRole.Remote, localActor.PrimaryForce);
			for (int i = 1; i < localActor.Forces.Count; i++)
			{
				localActorCopy.Forces.Add(localActor.Forces[i]);
			}

			Actor aiActorCopy = new Actor(theGame, aiActor.ID, ActorRole.Remote, aiActor.PrimaryForce);
			for (int i = 1; i < aiActor.Forces.Count; i++)
			{
				aiActorCopy.Forces.Add(aiActor.Forces[i]);
			}

			theGame.CreatePowerGrid(localForce);


			theGame.HUD.FocusWorldPoint = new Vector2(theGame.MapWidth / 2f, theGame.MapHeight / 2f);
			//hud.LocalActor = localActor;


			// Create your starting solar station
			SolarStation startingStation = new SolarStation(theGame, theGame, localActor.PrimaryForce, new Vector2(theGame.MapWidth / 2.0f, theGame.MapHeight / 2.0f));
			theGame.Add(startingStation);
			startingStation.StartConstruction();
			startingStation.IsConstructing = false;



			Force asteroidForce = new Force(theGame, theGame.GetNextForceID(), 0, Team.Neutral);
			theGame.AddForce(asteroidForce);

			// create a random asteroid field
			int minX = 0;		// MapX;?
			int minY = 0;		// MapY;?
			int maxX = theGame.MapWidth;
			int maxY = theGame.MapHeight;
			Random rand = new Random();
			Asteroid asteroid;
			for (int i = 0; i < 1000; i++)
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
				//int size = (int)((200 * rand.NextDouble()) + (200 * rand.NextDouble())+ (200 * rand.NextDouble()) + (5000 * rand.NextDouble()));
				theGame.Add(asteroid);
			}
			
		}


		public override void Update(TimeSpan deltaTime)
		{
			elapsedTime = elapsedTime.Add(deltaTime);

			if ((int)(elapsedTime.TotalMinutes) > sequence)
			{
				sequence++;

				WaveFactory.CreateWave(theGame, 100 * sequence, new Vector2(theGame.MapWidth / 2.0f, theGame.MapHeight / 2.0f) + new Vector2(3000, -3000));
			}
		}
	}
}
