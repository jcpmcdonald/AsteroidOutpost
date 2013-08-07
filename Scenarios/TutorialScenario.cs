using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using AsteroidOutpost.Components;
using AsteroidOutpost.Entities;
using AsteroidOutpost.Entities.Eventing;
using AsteroidOutpost.Screens;
using Microsoft.Xna.Framework;
using System.Globalization;

namespace AsteroidOutpost.Scenarios
{
	class TutorialScenario : Scenario
	{
		private TimeSpan elapsedTime;
		private int minersBuilt = 0;
		private int lasersBuilt = 0;

		private Vector2 startingPoint;
		
		private Mission currentMission;
		private Mission buildMiners = new Mission("buildMiners", "(0/2) Build 2 Miners Near Asteroids", false);
		private Mission buildLasers = new Mission("buildLasers", "(0/3) Build 3 Laser Towers", false);
		private Mission buildMorePower = new Mission("buildMorePower", "Build an other Solar Station", false);
		private Mission defendYourself = new Mission("defendYourself", "Defend yourself!", false);


		public TutorialScenario()
		{
			Name = "Tutorial";
			Author = "John McDonald";
		}


		public override void Start(AOGame theGame, int playerCount)
		{
			base.Start(theGame, playerCount);

			if(playerCount != 1)
			{
				// Umm... A multiplayer tutorial?
				Debugger.Break();
			}

			elapsedTime = new TimeSpan(0);

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


			world.HUD.DisablePowerButton();
			world.HUD.DisableLaserTowerButton();
			world.HUD.DisableMissileTowerButton();

			currentMission = buildMiners;
			StartMission();


			world.constructionSystem.ConstructionCompletedEvent += ConstructionSystem_ConstructionCompletedEvent;

			// Create a beacon
			//EntityFactory.Create("Beacon", new Dictionary<String, object>(){
			//    { "Sprite.Scale", 0.7f },
			//    { "Sprite.Set", null },
			//    { "Sprite.Animation", "Rotate" },
			//    { "Sprite.Orientation", GlobalRandom.Next(0, 359) },
			//    { "Transpose.Position", new Vector2((world.MapWidth / 2.0f) + 200, world.MapHeight / 2.0f) },
			//    { "Transpose.Radius", 40 },
			//    { "OwningForce", localForce }
			//});



			//EntityFactory.Create("(rotate frame)", new Dictionary<String, object>(){
			//    { "Sprite.Scale", 0.7f },
			//    { "Sprite.Set", null },
			//    { "Sprite.Animation", "Rotate" },
			//    { "Sprite.Orientation", 0f },
			//    { "Transpose.Position", new Vector2((world.MapWidth / 2.0f) + 200, world.MapHeight / 2.0f) },
			//    { "Transpose.Radius", 40 },
			//    { "OwningForce", localForce }
			//});


			//EntityFactory.Create("(use frames only)", new Dictionary<String, object>(){
			//    { "Sprite.Scale", 0.7f },
			//    { "Sprite.Set", null },
			//    { "Sprite.Animation", "Rotate" },
			//    { "Sprite.Orientation", 0f },
			//    { "Transpose.Position", new Vector2((world.MapWidth / 2.0f) + 300, world.MapHeight / 2.0f) },
			//    { "Transpose.Radius", 40 },
			//    { "OwningForce", localForce }
			//});


			//Beacon minerBeacon = new Beacon(world, world, localForce, new Vector2((world.MapWidth / 2.0f) + 200, world.MapHeight / 2.0f), 40);
			//beacons.Add(minerBeacon);

			//world.HUD.AddControl(frmInstructions);


			GenerateAsteroidField(1000);
		}

		//void ConstructableEntity_UpgradeFinishedEvent(EntityUpgradeEventArgs e)
		//{
		//    switch (progress)
		//    {
		//    case 1:
		//        //if (e.Entity is SolarStation)
		//        //{
		//        //    if (e.Upgrade.Name == "Level 2")
		//        //    {
		//        //        //lblUpgradeSolar.Text = "+" + lblUpgradeSolar.Text.Substring(1);
		//        //        //lblUpgradeSolar.Color = Color.LightGreen;
		//        //        progress++;
		//        //        StartSection(progress);
		//        //    }
		//        //}
		//        break;


		//    default:
		//        break;
		//    }
		//}


		protected override void World_EntityDied(int entityID)
		{
			Flocking fleetMovement = world.GetNullableComponent<Flocking>(entityID);
			if(fleetMovement != null)
			{
				// Yay! The enemy has died
				world.GameOver(true);
			}

			base.World_EntityDied(entityID);
		}


		void ConstructionSystem_ConstructionCompletedEvent(int entityID)
		{
			if(currentMission == buildMiners)
			{
				var miner = world.GetNullableComponent<LaserMiner>(entityID);
				if (miner != null)
				{
					// miner.ConnectedPowerSources().Count > 0 &&
					if (miner.nearbyAsteroids.Count > 0)
					{
						minersBuilt++;
						if (minersBuilt < 2)
						{
							buildMiners.Description = "(1/2) Build 2 Miners Near Asteroids";
						}
						else
						{
							buildMiners.Description = "(2/2) Build 2 Miners Near Asteroids";
							buildMiners.Done = true;

							currentMission = buildLasers;
							StartMission();
						}
					}
				}


			}
			else if(currentMission == buildLasers)
			{
				// This is a laser tower, right? It's got lasers!
				var laserTower = world.GetNullableComponent<LaserWeapon>(entityID);
				if (laserTower != null)
				{
					lasersBuilt++;
					if (lasersBuilt < 3)
					{
						buildLasers.Description = String.Format(CultureInfo.InvariantCulture, "({0}/3) Build 3 Laser Towers", lasersBuilt);
					}
					else
					{
						buildLasers.Description = "(3/3) Build 3 Laser Towers";
						buildLasers.Done = true;

						currentMission = buildMorePower;
						StartMission();
					}
				}


			}
			else if(currentMission == buildMorePower)
			{
				var solarStation = world.GetNullableComponent<PowerProducer>(entityID);
				if (solarStation != null)
				{
					buildMorePower.Done = true;
					currentMission = defendYourself;
					StartMission();
				}
			}
		}


		void StartMission()
		{
			//frmInstructions.Clear();
			//frmInstructions.Title = "Training (" + (section + 1) + " / 6)";

			if(currentMission == buildMiners)
			{
				world.HUD.ShowModalDialog("Welcome to training soldier! We need to get a base up and running asap. Build us two miners to get us started. Remember that they'll need power if they are going to do anything.");
				missions.Add(buildMiners);


			}
			else if(currentMission == buildLasers)
			{
				world.HUD.ShowModalDialog("Now that we have a good income stream, we should build some defences. Enemies can come from anywhere, so build 3 laser towers, each guarding a separate part of the base.");
				world.HUD.EnableLaserTowerButton();
				missions.Add(buildLasers);


			}
			else if(currentMission == buildMorePower)
			{
				world.HUD.ShowModalDialog("Good Job! Let's build an other solar station so that our lasers will always have power. Power consumers can only be connected to a single power source, so you will need to build this next to home.");
				world.HUD.EnablePowerButton();
				missions.Add(buildMorePower);


			}
			else if(currentMission == defendYourself)
			{
				world.HUD.ShowModalDialog("An enemy ship has been detected! Time to test your defences. Now is a good time to fill any gaps.");
				missions.Add(defendYourself);
				Vector2 enemyLocation = (Vector2.Normalize(new Vector2((float)GlobalRandom.NextDouble() - 0.5f, (float)GlobalRandom.NextDouble() - 0.5f)) * 2000) + startingPoint;
				WaveFactory.CreateWave(world, 100, enemyLocation);

			}
		}


		public override void Update(TimeSpan deltaTime)
		{
			
		}
	}
}
