using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using AsteroidOutpost.Components;
using Awesomium.Core;
using Microsoft.Xna.Framework;

namespace AsteroidOutpost.Scenarios
{
	// Creative name, I know
	class MinerealCollectionScenario : Scenario
	{
		private Vector2 startingPoint;

		private static readonly TimeSpan timeBetweenWaves = TimeSpan.FromSeconds(60);
		private TimeSpan waveTimer = timeBetweenWaves;
		private int sequence = 0;


		public MinerealCollectionScenario(AOGame theGame, int playerCount)
			: base(theGame, playerCount)
		{
		}


		public override void Start()
		{
			if(playerCount != 1)
			{
				// This is single-player only
				Debugger.Break();
			}

			// Set up some forces and actors
			friendlyForce = new Force(world, world.GetNextForceID(), 1000, Team.Team1);
			Controller localController = new Controller(world, ControllerRole.Local, friendlyForce);
			world.AddForce(friendlyForce);
			world.AddController(localController);

			Force aiForce = new Force(world, world.GetNextForceID(), 1000, Team.AI);
			Controller aiController = new AIController(world, aiForce);
			world.AddForce(aiForce);
			world.AddController(aiController);

			world.PowerGrid.Add(friendlyForce.ID, new PowerGrid(world));

			startingPoint = CreateStartingBase(friendlyForce);
			world.HUD.FocusWorldPoint = startingPoint;

			GenerateAsteroidField(1000);
			base.Start();
		}


		public override void Update(TimeSpan deltaTime)
		{
			waveTimer = waveTimer.Subtract(deltaTime);

			if (waveTimer <= TimeSpan.Zero)
			{
				sequence = Math.Min(3, sequence + 1);
				waveTimer = waveTimer.Add(timeBetweenWaves);

				Vector2 enemyLocation = (Vector2.Normalize(new Vector2((float)GlobalRandom.NextDouble() - 0.5f, (float)GlobalRandom.NextDouble() - 0.5f)) * 3000) + startingPoint;
				WaveFactory.CreateWave(world, 100 * sequence, enemyLocation);
			}

			theGame.Awesomium.WebView.CallJavascriptFunction("", "UpdateTimerPanel", new JSValue(waveTimer.ToString(@"m\:ss")));
		}
	}
}
