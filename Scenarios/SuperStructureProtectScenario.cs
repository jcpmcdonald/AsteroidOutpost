using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using AsteroidOutpost.Components;
using AsteroidOutpost.Entities;
using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;

namespace AsteroidOutpost.Scenarios
{
	public class SuperStructureProtectScenario : Scenario
	{
		private Vector2 startingPoint;
		private Mission currentMission;

		private Mission protectSuperStation = new Mission("protect", "Protect the super station", false);

		public SuperStructureProtectScenario()
		{
			Name = "Super Station";
			SceneName = "Snow&Ice";
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

			//startingPoint = CreateStartingBase(friendlyForce);
			Vector2 startingPoint = new Vector2(world.MapWidth / 2.0f, world.MapHeight / 2.0f);

			int superStation = world.Create("Super Station", friendlyForce, new JObject{
				{ "Position", new JObject{
					{ "Center", String.Format(CultureInfo.InvariantCulture, "{0}, {1}", startingPoint.X, startingPoint.Y) },
				}},
			});

			int power1 = world.Create("Solar Station", friendlyForce, new JObject{
				{ "Position", new JObject{
					{ "Center", String.Format(CultureInfo.InvariantCulture, "{0}, {1}", startingPoint.X + 110, startingPoint.Y + 380) },
				}},
			});

			int power2 = world.Create("Solar Station", friendlyForce, new JObject{
				{ "Position", new JObject{
					{ "Center", String.Format(CultureInfo.InvariantCulture, "{0}, {1}", startingPoint.X - 110, startingPoint.Y + 380) },
				}},
			});

			int power3 = world.Create("Solar Station", friendlyForce, new JObject{
				{ "Position", new JObject{
					{ "Center", String.Format(CultureInfo.InvariantCulture, "{0}, {1}", startingPoint.X, startingPoint.Y + 200) },
				}},
			});

			// Your starting stations are not constructing
			world.DeleteComponent(world.GetComponent<Constructing>(power1));
			world.DeleteComponent(world.GetComponent<Constructing>(power2));
			world.DeleteComponent(world.GetComponent<Constructing>(power3));

			world.ConnectToPowerGrid(world.GetComponent<PowerGridNode>(superStation));
			world.ConnectToPowerGrid(world.GetComponent<PowerGridNode>(power1));
			world.ConnectToPowerGrid(world.GetComponent<PowerGridNode>(power2));
			world.ConnectToPowerGrid(world.GetComponent<PowerGridNode>(power3));

			// Upgrade them all to level 2
			UpgradeTemplate upgrade = world.GetUpgrade("Solar Station L2 Upgrade");
			world.ApplyUpgrade(power1, upgrade);
			world.ApplyUpgrade(power2, upgrade);
			world.ApplyUpgrade(power3, upgrade);

			// Fill the solar stations with power
			PowerStorage storage1 = world.GetComponent<PowerStorage>(power1);
			PowerStorage storage2 = world.GetComponent<PowerStorage>(power2);
			PowerStorage storage3 = world.GetComponent<PowerStorage>(power3);
			storage1.AvailablePower = storage1.MaxPower;
			storage2.AvailablePower = storage2.MaxPower;
			storage3.AvailablePower = storage3.MaxPower;


			world.HUD.FocusWorldPoint = startingPoint;

			currentMission = protectSuperStation;
			StartMission();

			GenerateAsteroidField(1000);
		}


		void StartMission()
		{
			//frmInstructions.Clear();
			//frmInstructions.Title = "Training (" + (section + 1) + " / 6)";

			if (currentMission == protectSuperStation)
			{
				missions.Add(protectSuperStation);
			}
		}


		public override void Update(TimeSpan deltaTime)
		{
			
		}
	}
}
