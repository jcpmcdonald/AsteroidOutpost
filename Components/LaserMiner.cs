using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AsteroidOutpost.Entities.Eventing;
using AsteroidOutpost.Screens;
using Microsoft.Xna.Framework;

namespace AsteroidOutpost.Components
{
	enum MiningState
	{
		NoPower,
		Charging,
		Mining,
		Idle
	}

	class LaserMiner : Component
	{
		public readonly List<Minable> nearbyAsteroids = new List<Minable>();

		// Local event only
		public event Action<AccumulationEventArgs> AccumulationEvent;


		public LaserMiner(int entityID)
			: base(entityID)
		{
			State = MiningState.Idle;
			RescanForAsteroids = true;
			FirstUpdate = true;
			PartialMineralsToExtract = 0;
			MiningSourceOffset = Vector2.Zero;
			MiningDestinationOffset = Vector2.Zero;
			MiningAsteroid = -1;
			MiningRate = 25;
			EnergyUsageRate = 5;
			ChargeTime = 3000;
			MineTime = 900;
			MiningRange = 90;
		}


		/// <summary>
		/// Minerals per second while mining
		/// </summary>
		public float MiningRate { get; set; }

		/// <summary>
		/// Power per second while mining
		/// </summary>
		public float EnergyUsageRate { get; set; }

		/// <summary>
		/// Charging time in milliseconds
		/// </summary>
		public int ChargeTime { get; set; }

		/// <summary>
		/// Mining time in milliseconds
		/// </summary>
		public int MineTime { get; set; }

		public int MiningRange { get; set; }

		/// <summary>
		/// The asteroid we are minning
		/// </summary>
		public int MiningAsteroid { get; set; }

		public Vector2 MiningDestinationOffset { get; set; }


		public Vector2 MiningSourceOffset { get; set; }

		/// <summary>
		/// Collect minerals here, and only extract a whole number of minerals from asteroids
		/// </summary>
		public double PartialMineralsToExtract { get; set; }

		public bool FirstUpdate { get; set; }

		public bool RescanForAsteroids { get; set; }

		public MiningState State { get; set; }

		public TimeSpan TimeSinceLastStageChange { get; set; }


		public virtual void OnAccumulate(int wholeMineralsToExtract)
		{
			if(AccumulationEvent != null)
			{
				AccumulationEvent(new AccumulationEventArgs(wholeMineralsToExtract));
			}
		}
	}
}
