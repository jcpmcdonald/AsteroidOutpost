using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using AsteroidOutpost.Components;
using AsteroidOutpost.Entities;
using AsteroidOutpost.Networking;
using AsteroidOutpost.Screens;
using Awesomium.Core;
using Microsoft.Xna.Framework;
using System.Globalization;

namespace AsteroidOutpost.Scenarios
{
	public class RandomScenario : Scenario
	{
		private Vector2 startingPoint;

		private static readonly TimeSpan timeBetweenWaves = TimeSpan.FromSeconds(60);
		private TimeSpan waveTimer = timeBetweenWaves;
		private int sequence = 0;

		private Mission previousMission = null;
		private Mission currentMission;
		private TimeSpan timeAlive = TimeSpan.Zero;
		private int lifeGoal = 3;


		public RandomScenario()
		{
			Name = "Endless";
			SceneName = "Snow&Ice";
		}


		public override void Start(AOGame theGame, int playerCount)
		{
			base.Start(theGame, playerCount);

			// Set up the player forces and local controller
			int initialMinerals = 1000;
			for (int iPlayer = 0; iPlayer < playerCount; iPlayer++)
			{
				friendlyForce = new Force(world, world.GetNextForceID(), initialMinerals, (Team)iPlayer);
				world.AddForce(friendlyForce);
				//world.PowerGrid.Add(friendlyForce.ID, new PowerGrid(world));
				startingPoint = CreateStartingBase(friendlyForce);


				if (iPlayer == 0)
				{
					world.HUD.FocusWorldPoint = startingPoint;

					Controller controller = new Controller(world, ControllerRole.Local, friendlyForce);
					world.AddController(controller);
				}
			}


			Force aiForce = new Force(world, world.GetNextForceID(), 0, Team.AI);
			Controller aiController = new AIController(world, aiForce);
			world.AddForce(aiForce);
			world.AddController(aiController);

			currentMission = new Mission(lifeGoal + "mins", String.Format(CultureInfo.InvariantCulture, "(0 / {0}:00) Stay Alive for {0} minutes", lifeGoal), false);
			missions.Add(currentMission);

			world.ExecuteAwesomiumJS("MakeTimerPanel();");

			GenerateAsteroidField(1000);
		}



		public override void Update(TimeSpan deltaTime)
		{
			timeAlive += deltaTime;
			waveTimer = waveTimer.Subtract(deltaTime);

			if (waveTimer <= TimeSpan.Zero)
			{
				sequence++;
				waveTimer = waveTimer.Add(timeBetweenWaves);

				Vector2 enemyLocation = (Vector2.Normalize(new Vector2((float)GlobalRandom.NextDouble() - 0.5f, (float)GlobalRandom.NextDouble() - 0.5f)) * 3000) + startingPoint;
				WaveFactory.CreateWave(world, 100 * sequence, enemyLocation);
			}

			if(timeAlive.TotalMinutes >= lifeGoal)
			{
				if(previousMission != null)
				{
					deletedMissions.Add(previousMission);
				}
				currentMission.Description = String.Format(CultureInfo.InvariantCulture, "({0}:00 / {0}:00) Stay Alive for {0} minutes", lifeGoal);
				currentMission.Done = true;
				previousMission = currentMission;
				lifeGoal += 3;
				currentMission = new Mission(lifeGoal + "mins", String.Format(CultureInfo.InvariantCulture, "(0 / {0}:00) Stay Alive for {0} minutes", lifeGoal), false);
				missions.Add(currentMission);
			}

			currentMission.Description = String.Format(CultureInfo.InvariantCulture, "({1} / {0}:00) Stay Alive for {0} minutes", lifeGoal, timeAlive.ToString(@"m\:ss"));

			world.ExecuteAwesomiumJS(String.Format(CultureInfo.InvariantCulture, "UpdateTimerPanel('{0}')", waveTimer.ToString(@"m\:ss")));
		}
	}
}
