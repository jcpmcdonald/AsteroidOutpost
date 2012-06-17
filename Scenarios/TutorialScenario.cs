using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using AsteroidOutpost.Components;
using AsteroidOutpost.Entities;
using AsteroidOutpost.Entities.Eventing;
using AsteroidOutpost.Entities.Structures;
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


		private List<Beacon> beacons = new List<Beacon>(4);


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


			//lblBuild2Miners = new Label("- (0 / 2) Build 2 miners near asteroids", 5, 100, eAlignment.Left);
			//lblUpgradeSolar = new Label("- Upgrade your solar station", 5, 120, eAlignment.Left);
			//lblBuildLaserTowers = new Label("- (0 / 3) Build some laser towers", 5, 140, eAlignment.Left);


			// Set up some forces and actors
			Force localForce = new Force(world, world.GetNextForceID(), 1000, Team.Team1);
			Controller localController = new Controller(world, ControllerRole.Local, localForce);
			world.AddForce(localForce);
			world.AddController(localController);

			Force aiForce = new Force(world, world.GetNextForceID(), 1000, Team.AI);
			Controller aiController = new AIController(world, world, aiForce);
			world.AddForce(aiForce);
			world.AddController(aiController);
			

			world.CreatePowerGrid(localForce);


			world.HUD.FocusWorldPoint = new Vector2(world.MapWidth / 2f, world.MapHeight / 2f);

			// Create your starting solar station
			SolarStation startingStation = new SolarStation(world, world, localController.PrimaryForce, new Vector2(world.MapWidth / 2.0f, world.MapHeight / 2.0f));
			world.Add(startingStation);
			startingStation.StartConstruction();
			startingStation.IsConstructing = false;


			//lblBuild2Miners.Visible = false;
			//lblUpgradeSolar.Visible = false;
			//lblBuildLaserTowers.Visible = false;


			progress = 0;
			StartSection(progress);
			
			
			//btnNext = new Button("Next", (frmInstructions.Width / 2) - 50, frmInstructions.Height - 30, 100, 20);
			//btnNext.Click += btnNext_Click;
			//frmInstructions.AddControl(btnNext);

			ConstructableEntity.AnyConstructionCompletedEvent += ConstructableEntity_StructureFinishedEvent;
			ConstructableEntity.AnyUpgradeCompletedEvent += ConstructableEntity_UpgradeFinishedEvent;


			Beacon minerBeacon = new Beacon(world, world, localForce, new Vector2((world.MapWidth / 2.0f) + 200, world.MapHeight / 2.0f), 40);
			beacons.Add(minerBeacon);
			world.Add(minerBeacon);

			//world.HUD.AddControl(frmInstructions);


			GenerateAsteroidField(1000);
		}

		void ConstructableEntity_UpgradeFinishedEvent(EntityUpgradeEventArgs e)
		{
			switch (progress)
			{
			case 1:
				if (e.Entity is SolarStation)
				{
					if (e.Upgrade.Name == "Level 2")
					{
						//lblUpgradeSolar.Text = "+" + lblUpgradeSolar.Text.Substring(1);
						//lblUpgradeSolar.Color = Color.LightGreen;
						progress++;
						StartSection(progress);
					}
				}
				break;


			default:
				break;
			}
		}


		void ConstructableEntity_StructureFinishedEvent(EntityEventArgs e)
		{
			switch(progress)
			{
			case 0:
				LaserMiner miner = e.Entity as LaserMiner;
				if (miner != null)
				{
					// miner.ConnectedPowerSources().Count > 0 &&
					if(miner.NearbyAsteroids().Count > 0)
					{
						minersBuilt++;
						if(minersBuilt == 2)
						{
							//lblBuild2Miners.Text = "+ (" + minersBuilt + " / 2) Build 2 miners near asteroids";
							//lblBuild2Miners.Color = Color.LightGreen;
							progress++;
							StartSection(progress);
						}
						else
						{
							//lblBuild2Miners.Text = "- (" + minersBuilt + " / 2) Build 2 miners near asteroids";
						}
					}
				}
				break;



			case 2:
				if (e.Entity is LaserTower)
				{
					lasersBuilt++;
					if(lasersBuilt == 3)
					{
						//lblBuildLaserTowers.Text = "+ (" + lasersBuilt + " / 3) Build some laser towers";
						//lblBuildLaserTowers.Color = Color.LightGreen;
						progress++;
						StartSection(progress);
					}
					else
					{
						//lblBuildLaserTowers.Text = "- (" + lasersBuilt + " / 3) Build some laser towers";
					}
				}
				break;



			default:
				break;
			}
		}


		void StartSection(int section)
		{
			//frmInstructions.Clear();
			//frmInstructions.Title = "Training (" + (section + 1) + " / 6)";

			switch(section)
			{
			case 0:
			{
				//frmInstructions.AddControl(new Label("Welcome to training soldier! We need to get a base up and running asap. Build us two miners to get us started. Remember that they will need power if they are going to do anything.", 5, 5, frmInstructions.Width - 10, true));

				// Set up the mission text on the left
				//lblBuild2Miners.Visible = true;
				//lblBuild2Miners.Text = "- (" + minersBuilt + " / 2) Build 2 miners near asteroids";
				//lblBuild2Miners.Color = Color.Tomato;
				//world.HUD.AddControl(lblBuild2Miners);
				break;
			}

			case 1:
			{
				//frmInstructions.AddControl(new Label("Well done! Now we will need some more power before we can fight off the alien ships. I would recommend upgrading your existing solar station to provide the additional power.", 5, 5, frmInstructions.Width - 10, true));

				//lblUpgradeSolar.Visible = true;
				//lblUpgradeSolar.Color = Color.Tomato;
				//world.HUD.AddControl(lblUpgradeSolar);
				break;
			}

			case 2:
			{
				//frmInstructions.AddControl(new Label("Now that we have some power, build a few laser towers where indicated to fend off the aliens.", 5, 5, frmInstructions.Width - 10, true));

				//lblBuildLaserTowers.Visible = true;
				//lblBuildLaserTowers.Color = Color.Tomato;
				//world.HUD.AddControl(lblBuildLaserTowers);
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
				StartSection(progress);
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
