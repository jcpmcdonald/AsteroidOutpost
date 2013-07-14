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

namespace AsteroidOutpost.Scenarios
{
	public class RandomScenario : Scenario
	{
		private static readonly TimeSpan timeBetweenWaves = TimeSpan.FromSeconds(60);
		private TimeSpan waveTimer = timeBetweenWaves;
		private int sequence = 0;


		public RandomScenario(AOGame theGame, int playerCount)
			: base(theGame, playerCount)
		{
		}


		public override void Start()
		{
			// Set up the player forces and local controller
			int initialMinerals = 1000;
			for (int iPlayer = 0; iPlayer < playerCount; iPlayer++)
			{
				friendlyForce = new Force(world, world.GetNextForceID(), initialMinerals, (Team)iPlayer);
				world.AddForce(friendlyForce);
				world.PowerGrid.Add(friendlyForce.ID, new PowerGrid(world));
				Vector2 focusPoint = CreateStartingBase(friendlyForce);


				if (iPlayer == 0)
				{
					world.HUD.FocusWorldPoint = focusPoint;

					Controller controller = new Controller(world, ControllerRole.Local, friendlyForce);
					world.AddController(controller);
				}
			}


			Force aiForce = new Force(world, world.GetNextForceID(), 0, Team.AI);
			Controller aiController = new AIController(world, aiForce);
			world.AddForce(aiForce);
			world.AddController(aiController);

			missions.Add(new Mission("Alive", "Stay Alive", false));

			world.ExecuteAwesomiumJS("MakeTimerPanel();");

			GenerateAsteroidField(1000);
			base.Start();
		}



		public override void Update(TimeSpan deltaTime)
		{
			waveTimer = waveTimer.Subtract(deltaTime);

			if (waveTimer <= TimeSpan.Zero)
			{
				sequence++;
				waveTimer = waveTimer.Add(timeBetweenWaves);

				WaveFactory.CreateWave(world, 100 * sequence, new Vector2(world.MapWidth / 2.0f, world.MapHeight / 2.0f) + new Vector2(3000, -3000));
			}

			world.ExecuteAwesomiumJS(String.Format("UpdateTimerPanel('{0}')", waveTimer.ToString(@"m\:ss")));
		}
	}
}
