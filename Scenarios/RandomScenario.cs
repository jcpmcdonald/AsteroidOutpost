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


		public RandomScenario(AsteroidOutpostScreen theGame, int playerCount)
			: base(theGame, playerCount)
		{
		}


		public override void Start()
		{
			elapsedTime = new TimeSpan(0);

			GenerateAsteroidField(1000);

			// Set up the player forces and local controller
			int initialMinerals = 1000;
			for (int iPlayer = 0; iPlayer < playerCount; iPlayer++)
			{
				Force force = new Force(theGame, theGame.GetNextForceID(), initialMinerals, (Team)iPlayer);
				theGame.AddForce(force);
				theGame.CreatePowerGrid(force);
				Vector2 focusPoint = CreateStartingBase(force);


				if (iPlayer == 0)
				{
					theGame.HUD.FocusWorldPoint = focusPoint;

					Controller controller = new Controller(theGame, ControllerRole.Local, force);
					theGame.AddController(controller);
				}
			}


			Force aiForce = new Force(theGame, theGame.GetNextForceID(), 0, Team.AI);
			Controller aiController = new AIController(theGame, theGame, aiForce);
			theGame.AddForce(aiForce);
			theGame.AddController(aiController);
			
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
