using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using AsteroidOutpost.Components;
using Microsoft.Xna.Framework;
using System.Globalization;

namespace AsteroidOutpost.Scenarios
{
	// Creative name, I know
	class MinerealCollectionScenario : Scenario
	{
		private Vector2 startingPoint;

		private static readonly TimeSpan timeBetweenWaves = TimeSpan.FromSeconds(60);
		private TimeSpan waveTimer = timeBetweenWaves;
		private int sequence = 0;

		
		private Mission currentMission;
		private Mission collectMinerals = new Mission("collectMinerals", "(0/2000) Collect 2000 minerals", false);


		public MinerealCollectionScenario()
		{
			Name = "Mineral Grab";
			SceneName = "snow&ice";
		}


		public override void Start(AOGame theGame, int playerCount)
		{
			base.Start(theGame, playerCount);

			if(playerCount != 1)
			{
				// This is single-player only
				Debugger.Break();
			}

			// Set up some forces and actors
			friendlyForce = new Force(world, world.GetNextForceID(), 250, Team.Team1);
			Controller localController = new Controller(world, ControllerRole.Local, friendlyForce);
			world.AddForce(friendlyForce);
			world.AddController(localController);

			Force aiForce = new Force(world, world.GetNextForceID(), 250, Team.AI);
			Controller aiController = new AIController(world, aiForce);
			world.AddForce(aiForce);
			world.AddController(aiController);

			//world.PowerGrid.Add(friendlyForce.ID, new PowerGrid(world));

			startingPoint = CreateStartingBase(friendlyForce);
			world.HUD.FocusWorldPoint = startingPoint;

			currentMission = collectMinerals;
			StartMission();

			world.ExecuteAwesomiumJS("MakeTimerPanel()");

			GenerateAsteroidField(1000);
		}


		void StartMission()
		{
			//frmInstructions.Clear();
			//frmInstructions.Title = "Training (" + (section + 1) + " / 6)";

			if (currentMission == collectMinerals)
			{
				missions.Add(collectMinerals);
			}
		}


		public override void Update(TimeSpan deltaTime)
		{
			waveTimer = waveTimer.Subtract(deltaTime);

			collectMinerals.Description = String.Format(CultureInfo.InvariantCulture, "({0}/2000) Collect 2000 minerals", friendlyForce.GetMinerals());

			if(friendlyForce.GetMinerals() >= 2000 && !collectMinerals.Done)
			{
				collectMinerals.Done = true;
				world.GameOver(true); // Win!
			}

			if (waveTimer <= TimeSpan.Zero)
			{
				sequence = Math.Min(3, sequence + 1);
				waveTimer = waveTimer.Add(timeBetweenWaves);

				Vector2 enemyLocation = (Vector2.Normalize(new Vector2((float)GlobalRandom.NextDouble() - 0.5f, (float)GlobalRandom.NextDouble() - 0.5f)) * 3000) + startingPoint;
				WaveFactory.CreateWave(world, 100 * sequence, enemyLocation);
			}

			world.ExecuteAwesomiumJS(String.Format(CultureInfo.InvariantCulture, "UpdateTimerPanel('{0}')", waveTimer.ToString(@"m\:ss")));
		}
	}
}
