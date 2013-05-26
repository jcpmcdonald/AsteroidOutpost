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

namespace AsteroidOutpost.Scenarios
{
	class TutorialScenario : Scenario
	{
		private TimeSpan elapsedTime;
		private int progress = 0;
		private int minersBuilt = 0;
		private int lasersBuilt = 0;

		private Mission buildMiners = new Mission("buildMiners", "(0/2) Build 2 Miners Near Asteroids", false);
		private Mission buildLasers = new Mission("buildLasers", "(0/3) Build 3 Laser Towers", false);
		private Mission defendYourself = new Mission("defendYourself", "Defend yourself!", false);
		//private List<Beacon> beacons = new List<Beacon>(4);


		//private Button btnNext;
		//private Form frmInstructions;
		//private Label lblBuild2Miners;
		//private Label lblUpgradeSolar;
		//private Label lblBuildLaserTowers;


		public TutorialScenario(AOGame theGame, int playerCount)
			: base(theGame, playerCount)
		{
		}


		public override void Start()
		{
			if(playerCount != 1)
			{
				// Umm... A multiplayer tutorial?
				Debugger.Break();
			}

			elapsedTime = new TimeSpan(0);


			
			//missions.Add(new Mission("buildLaserTowers", "Build 3 Laser Towers", false));
			//lblBuild2Miners = new Label("- (0 / 2) Build 2 miners near asteroids", 5, 100, eAlignment.Left);
			//lblUpgradeSolar = new Label("- Upgrade your solar station", 5, 120, eAlignment.Left);
			//lblBuildLaserTowers = new Label("- (0 / 3) Build some laser towers", 5, 140, eAlignment.Left);


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

			world.HUD.FocusWorldPoint = CreateStartingBase(friendlyForce);


			//lblBuild2Miners.Visible = false;
			//lblUpgradeSolar.Visible = false;
			//lblBuildLaserTowers.Visible = false;


			progress = 0;
			StartSection();
			
			
			//btnNext = new Button("Next", (frmInstructions.Width / 2) - 50, frmInstructions.Height - 30, 100, 20);
			//btnNext.Click += btnNext_Click;
			//frmInstructions.AddControl(btnNext);

			//ConstructableEntity.AnyConstructionCompletedEvent += ConstructableEntity_StructureFinishedEvent;
			//ConstructableEntity.AnyUpgradeCompletedEvent += ConstructableEntity_UpgradeFinishedEvent;
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
			//world.Add(minerBeacon);

			//world.HUD.AddControl(frmInstructions);


			GenerateAsteroidField(1000);
			base.Start();
		}

		void ConstructableEntity_UpgradeFinishedEvent(EntityUpgradeEventArgs e)
		{
			switch (progress)
			{
			case 1:
				//if (e.Entity is SolarStation)
				//{
				//    if (e.Upgrade.Name == "Level 2")
				//    {
				//        //lblUpgradeSolar.Text = "+" + lblUpgradeSolar.Text.Substring(1);
				//        //lblUpgradeSolar.Color = Color.LightGreen;
				//        progress++;
				//        StartSection(progress);
				//    }
				//}
				break;


			default:
				break;
			}
		}


		void ConstructionSystem_ConstructionCompletedEvent(int entityID)
		{
			switch (progress)
			{
			case 0:
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
							progress++;
							StartSection();
						}
					}
				}
				break;



			case 1:
				// This is a laser tower, right? It's got lasers!
				var laserTower = world.GetNullableComponent<LaserWeapon>(entityID);
				if (laserTower != null)
				{
					lasersBuilt++;
					if (lasersBuilt < 3)
					{
						buildLasers.Description = String.Format("({0}/3) Build 3 Laser Towers", lasersBuilt);
					}
					else
					{
						buildLasers.Description = "(3/3) Build 3 Laser Towers";
						buildLasers.Done = true;
						progress++;
						StartSection();
					}
				}
				break;



			default:
				break;
			}
		}


		void StartSection()
		{
			//frmInstructions.Clear();
			//frmInstructions.Title = "Training (" + (section + 1) + " / 6)";

			switch(progress)
			{
			case 0:
			{
				theGame.ExecuteAwesomiumJS("ShowModalDialog('Welcome to training soldier! We need to get a base up and running asap. Build us two miners to get us started. Remember that they will need power if they are going to do anything.')");
				//frmInstructions.AddControl(new Label("Welcome to training soldier! We need to get a base up and running asap. Build us two miners to get us started. Remember that they will need power if they are going to do anything.", 5, 5, frmInstructions.Width - 10, true));
				missions.Add(buildMiners);
				break;
			}

			case 1:
			{
				theGame.ExecuteAwesomiumJS("ShowModalDialog('Now that we have a good income stream, we should build some defences. I heard rumours that ')");
				//frmInstructions.AddControl(new Label("Well done! Now we will need some more power before we can fight off the alien ships. I would recommend upgrading your existing solar station to provide the additional power.", 5, 5, frmInstructions.Width - 10, true));
				missions.Add(buildLasers);
				break;
			}

			case 2:
			{
				//frmInstructions.AddControl(new Label("Now that we have some power, build a few laser towers where indicated to fend off the aliens.", 5, 5, frmInstructions.Width - 10, true));
				missions.Add(defendYourself);
				break;
			}

			case 3:
			{

				break;
			}

			default:
				break;
			}
		}


		private void btnNext_Click(object sender, EventArgs e)
		{
			switch(progress)
			{
			case 0:
				progress++;
				StartSection();
				break;
			}

			//btnNext.Location = btnNext.Location - new Vector2(0, 20);
			//frmInstructions.AddControl(btnNext);
		}


		public override void Update(TimeSpan deltaTime)
		{
			
		}
	}
}
