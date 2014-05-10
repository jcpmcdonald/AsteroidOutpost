using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using AsteroidOutpost.Components;
using AsteroidOutpost.Entities;
using AsteroidOutpost.Eventing;
using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;

namespace AsteroidOutpost.Scenarios
{
	public class SuperStructureProtectScenario : Scenario
	{
		private Vector2 startingPoint;

		private TimeSpan waveTimer = TimeSpan.FromSeconds(20);
		private TimeSpan timeAlive = TimeSpan.Zero;

		private Mission currentMission;
		private int superStation;
		private bool overloading = false;
		private TimeSpan timeBetweenOverloadExplosions = TimeSpan.FromSeconds(0.3);
		private TimeSpan nextOverloadExplosion = TimeSpan.Zero;
		private bool epicExplosionDone = false;
		private List<Vector2> overloadFires = new List<Vector2>();

		public delegate bool OneOffCondition();
		public delegate void OneOffAction();
		private List<Tuple<OneOffCondition, OneOffAction>> oneOffEvents = new List<Tuple<OneOffCondition,OneOffAction>>();

		private Mission protectSuperStation = new Mission("protect", "Protect the super station", false);
		private Mission prepareForRampUp = new Mission("prepare1", "Prepare for additional power needs", false);
		private Mission prepareForRampUp2 = new Mission("prepare2", "More power!", false);
		private Mission maintainPowerLevel = new Mission("maintain", "Maintain power level", false);

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
			friendlyForce = new Force(world, world.GetNextForceID(), 800, Team.Team1);
			Controller localController = new Controller(world, ControllerRole.Local, friendlyForce);
			world.AddForce(friendlyForce);
			world.AddController(localController);

			Force aiForce = new Force(world, world.GetNextForceID(), 250, Team.AI);
			Controller aiController = new AIController(world, aiForce);
			world.AddForce(aiForce);
			world.AddController(aiController);

			//startingPoint = CreateStartingBase(friendlyForce);
			startingPoint = new Vector2(world.MapWidth / 2.0f, world.MapHeight / 2.0f);

			superStation = world.Create("Super Station", friendlyForce, new JObject{
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

			var superStationHitPoints = world.GetComponent<HitPoints>(superStation);
			superStationHitPoints.ArmourChanged += SuperStationHitPointsOnArmourChanged;

			world.HUD.FocusWorldPoint = startingPoint;

			currentMission = protectSuperStation;
			StartMission();

			GenerateAsteroidField(1000);
		}


		


		private void StartMission()
		{
			var scienceVessel = world.GetComponent<ScienceVessel>(superStation);
			if (currentMission == protectSuperStation)
			{
				world.HUD.ShowConversation("You see this giant thing? It needs LOTS of power, so keep the power coming. We need this to win the war, so protect it with your life.");
				missions.Add(protectSuperStation);

				oneOffEvents.Add(new Tuple<OneOffCondition, OneOffAction>(
						                 () => timeAlive.TotalSeconds > 30,
						                 delegate()
						                 {
							                 currentMission = prepareForRampUp;
							                 StartMission();
						                 }));

				oneOffEvents.Add(new Tuple<OneOffCondition, OneOffAction>(
						                 () => scienceVessel.Overload,
						                 delegate()
						                 {
							                 world.HUD.ShowConversation("THE REACTOR IS OVERLOADING!!!! GET OUT OF HERE!");
							                 overloading = true;
						                 }));

			}
			else if (currentMission == prepareForRampUp)
			{
				world.HUD.ShowConversation("Scientists are having a difficult time maintaining core temperatures and will require additional power right away. Our lives depend on this.");
				missions.Add(prepareForRampUp);

				oneOffEvents.Add(new Tuple<OneOffCondition, OneOffAction>(
						                 () => timeAlive.TotalSeconds > 50,
						                 delegate()
						                 {
							                 scienceVessel.PowerConsumptionRate *= 1.10f;
						                 }));

				oneOffEvents.Add(new Tuple<OneOffCondition, OneOffAction>(
						                 () => timeAlive.TotalSeconds > 70,
						                 delegate()
						                 {
							                 scienceVessel.PowerConsumptionRate *= 1.20f;
						                 }));

				oneOffEvents.Add(new Tuple<OneOffCondition, OneOffAction>(
						                 () => timeAlive.TotalSeconds > 90,
						                 delegate()
						                 {
							                 prepareForRampUp.Done = true;
							                 currentMission = prepareForRampUp2;
							                 missions.Add(currentMission);
							                 world.HUD.ShowConversation("Good job so far, but we need even more power. Keep the power flowing!");
							                 scienceVessel.PowerConsumptionRate *= 1.40f;
						                 }));

				oneOffEvents.Add(new Tuple<OneOffCondition, OneOffAction>(
						                 () => timeAlive.TotalSeconds > 110,
						                 delegate()
						                 {
							                 scienceVessel.PowerConsumptionRate *= 1.5f;
						                 }));

				oneOffEvents.Add(new Tuple<OneOffCondition, OneOffAction>(
						                 () => timeAlive.TotalSeconds > 130,
						                 delegate()
						                 {
							                 world.HUD.ShowConversation("The science vessel is opperating at maximum capacity. Maintain this and we can all go home early.");
							                 scienceVessel.PowerConsumptionRate *= 1.5f;
							                 prepareForRampUp2.Done = true;
							                 currentMission = maintainPowerLevel;
							                 missions.Add(currentMission);
						                 }));
			}
		}


		public override void Update(TimeSpan deltaTime)
		{
			timeAlive += deltaTime;
			waveTimer = waveTimer.Subtract(deltaTime);

			for (int i = oneOffEvents.Count - 1; i >= 0 ; i--)
			{
				if (oneOffEvents[i].Item1())
				{
					oneOffEvents[i].Item2();
					oneOffEvents.RemoveAt(i);
				}
			}

			if (overloadFires.Count > 0)
			{
				var scienceVessel = world.GetNullableComponent<ScienceVessel>(superStation);
				if (scienceVessel != null)
				{
					ParticleEffectManager particleEffectManager = (ParticleEffectManager)theGame.Services.GetService(typeof (ParticleEffectManager));
					var position = world.GetNullableComponent<Position>(scienceVessel);
					foreach (var firePosition in overloadFires)
					{
						particleEffectManager.Trigger("Fire", position.Center + firePosition);
					}
				}
			}
		}


		private void SuperStationHitPointsOnArmourChanged(EntityArmourChangedEventArgs entityArmourChangedEventArgs)
		{
			//HitPoints hitPoints = (HitPoints)entityArmourChangedEventArgs.Component;
			int desiredFires = Math.Max(0, (int)((250f - entityArmourChangedEventArgs.NewArmour) / 15f));
			while (overloadFires.Count < desiredFires)
			{
				overloadFires.Add(new Vector2(GlobalRandom.Next(-50, 50), GlobalRandom.Next(-50, 50)));
			}
			while (overloadFires.Count > desiredFires)
			{
				overloadFires.RemoveAt(0);
			}

			if (entityArmourChangedEventArgs.NewArmour < 100 && !epicExplosionDone)
			{
				world.GetComponent<ScienceVessel>(superStation).Overload = true;
				epicExplosionDone = true;
				ParticleEffectManager particleEffectManager = (ParticleEffectManager)theGame.Services.GetService(typeof(ParticleEffectManager));
				var position = world.GetComponent<Position>(superStation);
				particleEffectManager.Trigger("Epic Explosion", position.Center);
			}
		}


		protected override void World_EntityDied(int deadID)
		{
			if (deadID == superStation)
			{
				// You failed to protect the super station
				oneOffEvents.Clear();

				world.GameOver(false);
			}
			base.World_EntityDied(deadID);
		}
	}
}
