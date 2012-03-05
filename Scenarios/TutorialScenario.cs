using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteroidOutpost.Entities;
using AsteroidOutpost.Entities.Eventing;
using AsteroidOutpost.Entities.Structures;
using AsteroidOutpost.Screens;
using C3.XNA.Controls;
using C3.XNA.Events;
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


		private Button btnNext;
		private Form frmInstructions;
		private Label lblBuild2Miners;
		private Label lblUpgradeSolar;
		private Label lblBuildLaserTowers;
		

		public TutorialScenario(AsteroidOutpostScreen theGame) : base(theGame)
		{
		}


		public override void Start()
		{

			elapsedTime = new TimeSpan(0);

			frmInstructions = new Form("", theGame.Width - 300, 0, 300, 215);
			lblBuild2Miners = new Label("- (0 / 2) Build 2 miners near asteroids", 5, 100, eAlignment.Left);
			lblUpgradeSolar = new Label("- Upgrade your solar station", 5, 120, eAlignment.Left);
			lblBuildLaserTowers = new Label("- (0 / 3) Build some laser towers", 5, 140, eAlignment.Left);


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

			// Create your starting solar station
			SolarStation startingStation = new SolarStation(theGame, theGame, localActor.PrimaryForce, new Vector2(theGame.MapWidth / 2.0f, theGame.MapHeight / 2.0f));
			theGame.AddComponent(startingStation);
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
				int size = (int)(61200 * rand.NextDouble());
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
							asteroid = new Asteroid(theGame, theGame, asteroidForce, new Vector2(x, y), size);
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
				theGame.AddComponent(asteroid);
			}


			lblBuild2Miners.Visible = false;
			lblUpgradeSolar.Visible = false;
			lblBuildLaserTowers.Visible = false;


			progress = 0;
			StartSection(progress);
			
			
			btnNext = new Button("Next", (frmInstructions.Width / 2) - 50, frmInstructions.Height - 30, 100, 20);
			btnNext.Click += btnNext_Click;
			frmInstructions.AddControl(btnNext);

			ConstructableEntity.AnyConstructionCompletedEvent += ConstructableEntity_StructureFinishedEvent;
			ConstructableEntity.AnyUpgradeCompletedEvent += ConstructableEntity_UpgradeFinishedEvent;


			Beacon minerBeacon = new Beacon(theGame, theGame, localForce, new Vector2((theGame.MapWidth / 2.0f) + 200, theGame.MapHeight / 2.0f), 40);
			beacons.Add(minerBeacon);
			theGame.AddComponent(minerBeacon);

			theGame.HUD.AddControl(frmInstructions);
		}

		void ConstructableEntity_UpgradeFinishedEvent(EntityUpgradeEventArgs e)
		{
			switch (progress)
			{
			case 1:
				if (e.Component is SolarStation)
				{
					if (e.Upgrade.Name == "Level 2")
					{
						lblUpgradeSolar.Text = "+" + lblUpgradeSolar.Text.Substring(1);
						lblUpgradeSolar.Color = Color.LightGreen;
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
				LaserMiner miner = e.Component as LaserMiner;
				if (miner != null)
				{
					// miner.ConnectedPowerSources().Count > 0 &&
					if(miner.NearbyAsteroids().Count > 0)
					{
						minersBuilt++;
						if(minersBuilt == 2)
						{
							lblBuild2Miners.Text = "+ (" + minersBuilt + " / 2) Build 2 miners near asteroids";
							lblBuild2Miners.Color = Color.LightGreen;
							progress++;
							StartSection(progress);
						}
						else
						{
							lblBuild2Miners.Text = "- (" + minersBuilt + " / 2) Build 2 miners near asteroids";
						}
					}
				}
				break;



			case 2:
				if(e.Component is LaserTower)
				{
					lasersBuilt++;
					if(lasersBuilt == 3)
					{
						lblBuildLaserTowers.Text = "+ (" + lasersBuilt + " / 3) Build some laser towers";
						lblBuildLaserTowers.Color = Color.LightGreen;
						progress++;
						StartSection(progress);
					}
					else
					{
						lblBuildLaserTowers.Text = "- (" + lasersBuilt + " / 3) Build some laser towers";
					}
				}
				break;



			default:
				break;
			}
		}


		void StartSection(int section)
		{
			frmInstructions.Clear();
			frmInstructions.Title = "Training (" + (section + 1) + " / 6)";

			switch(section)
			{
			case 0:
			{
				frmInstructions.AddControl(new Label("Welcome to training soldier! We need to get a base up and running asap. Build us two miners to get us started. Remember that they will need power if they are going to do anything.", 5, 5, frmInstructions.Width - 10, true));

				// Set up the mission text on the left
				lblBuild2Miners.Visible = true;
				lblBuild2Miners.Text = "- (" + minersBuilt + " / 2) Build 2 miners near asteroids";
				lblBuild2Miners.Color = Color.Tomato;
				theGame.HUD.AddControl(lblBuild2Miners);
				break;
			}

			case 1:
			{
				frmInstructions.AddControl(new Label("Well done! Now we will need some more power before we can fight off the alien ships. I would recommend upgrading your existing solar station to provide the additional power.", 5, 5, frmInstructions.Width - 10, true));

				lblUpgradeSolar.Visible = true;
				lblUpgradeSolar.Color = Color.Tomato;
				theGame.HUD.AddControl(lblUpgradeSolar);
				break;
			}

			case 2:
			{
				frmInstructions.AddControl(new Label("Now that we have some power, build a few laser towers where indicated to fend off the aliens.", 5, 5, frmInstructions.Width - 10, true));

				lblBuildLaserTowers.Visible = true;
				lblBuildLaserTowers.Color = Color.Tomato;
				theGame.HUD.AddControl(lblBuildLaserTowers);
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


		private void btnNext_Click(object sender, MouseButtonEventArgs e)
		{
			switch(progress)
			{
			case 0:
				progress++;
				StartSection(progress);
				break;
			}

			btnNext.Location = btnNext.Location - new Vector2(0, 20);
			frmInstructions.AddControl(btnNext);
		}


		public override void Update(TimeSpan deltaTime)
		{
			
		}
	}
}
