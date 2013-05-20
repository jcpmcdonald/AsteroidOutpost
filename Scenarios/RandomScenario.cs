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
		private Force friendlyForce;


		public RandomScenario(AOGame theGame, int playerCount)
			: base(theGame, playerCount)
		{
		}


		public override void Start()
		{
			GenerateAsteroidField(1000);

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

			theGame.World.EntityDied += World_EntityDied;

			theGame.Awesomium.WebView.CallJavascriptFunction("", "MakeTimerPanel", new JSValue());
		}



		void World_EntityDied(int entityID)
		{
			// Check to see if the player has lost
			//     The player loses if they have no more power producers

			PowerProducer deadPowerProducer = world.GetNullableComponent<PowerProducer>(entityID);
			if(deadPowerProducer != null)
			{
				// A power producer has been eliminated, check to see if there is still power out there
				if(!world.GetComponents<PowerProducer>().Any(p => p != deadPowerProducer && p.PowerStateActive && world.GetOwningForce(p) == friendlyForce))
				{
					// No power sources, it is impossible to recover. You are dead, or will be very soon
					Console.WriteLine("DEAD!");
				}
			}
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

			theGame.Awesomium.WebView.CallJavascriptFunction("", "UpdateTimerPanel", new JSValue(waveTimer.ToString(@"m\:ss")));
		}
	}
}
