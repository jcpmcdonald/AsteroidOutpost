using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using AsteroidOutpost.Entities.Eventing;
using AsteroidOutpost.Screens;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

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
		private int miningRange;

		// Local event only
		public event Action<AccumulationEventArgs> AccumulationEvent;


		public LaserMiner(int entityID)
			: base(entityID)
		{
			State = MiningState.Idle;
			RescanForAsteroids = true;
			FirstUpdate = true;
			PartialMineralsToExtract = 0;
			MiningDestinationOffset = Vector2.Zero;
			MiningAsteroid = -1;

			// Now set in JSON
			//MiningSourceOffset = Vector2.Zero;
			//MiningRate = 25;
			//EnergyUsageRate = 5;
			//ChargeTime = 3000;
			//MineTime = 900;
			//MiningRange = 90;
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


		public int MiningRange
		{
			get
			{
				return miningRange;
			}
			set
			{
				miningRange = value;
				RescanForAsteroids = true;
			}
		}

		/// <summary>
		/// The asteroid we are minning
		/// </summary>
		[XmlIgnore]
		[JsonIgnore]
		public int MiningAsteroid { get; set; }

		public Vector2 MiningDestinationOffset { get; set; }


		public Vector2 MiningSourceOffset { get; set; }

		/// <summary>
		/// Collect minerals here, and only extract a whole number of minerals from asteroids
		/// </summary>
		[XmlIgnore]
		[JsonIgnore]
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
