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
		private MiningState state = MiningState.Idle;
		private TimeSpan timeSinceLastStageChange;
		
		private float miningRate = 25;          // minerals per second while mining
		private float energyUsageRate = 5;      // power per second while mining
		private int chargeTime = 3000;          // charging time in milliseconds
		private int mineTime = 900;             // mining time in milliseconds
		private int miningRange = 90;
		
		
		// Remember which asteroid we mined last so that we can mine the next asteroid
		private int miningAsteroid = -1;
		private Vector2 miningDestinationOffset = Vector2.Zero;
		private Vector2 miningSourceOffset = Vector2.Zero;

		// Collect minerals here, and only extract a whole number of minerals from asteroids
		private double partialMineralsToExtract = 0;

		private bool firstUpdate = true;
		private bool rescanForAsteroids = true;

		public readonly List<Minable> nearbyAsteroids = new List<Minable>();

		// Local event only
		public event Action<AccumulationEventArgs> AccumulationEvent;


		public LaserMiner(World world, int entityID)
			: base(world, entityID)
		{
		}


		protected LaserMiner(BinaryReader br)
			: base(br)
		{
		}


		public float MiningRate
		{
			get
			{
				return miningRate;
			}
			set
			{
				miningRate = value;
			}
		}

		public float EnergyUsageRate
		{
			get
			{
				return energyUsageRate;
			}
			set
			{
				energyUsageRate = value;
			}
		}

		public int ChargeTime
		{
			get
			{
				return chargeTime;
			}
			set
			{
				chargeTime = value;
			}
		}

		public int MineTime
		{
			get
			{
				return mineTime;
			}
			set
			{
				mineTime = value;
			}
		}

		public int MiningRange
		{
			get
			{
				return miningRange;
			}
			set
			{
				miningRange = value;
			}
		}

		public int MiningAsteroid
		{
			get
			{
				return miningAsteroid;
			}
			set
			{
				miningAsteroid = value;
			}
		}

		public Vector2 MiningDestinationOffset
		{
			get
			{
				return miningDestinationOffset;
			}
			set
			{
				miningDestinationOffset = value;
			}
		}

		
		public Vector2 MiningSourceOffset
		{
			get
			{
				return miningSourceOffset;
			}
			set
			{
				miningSourceOffset = value;
			}
		}

		public double PartialMineralsToExtract
		{
			get
			{
				return partialMineralsToExtract;
			}
			set
			{
				partialMineralsToExtract = value;
			}
		}

		public bool FirstUpdate
		{
			get
			{
				return firstUpdate;
			}
			set
			{
				firstUpdate = value;
			}
		}

		public bool RescanForAsteroids
		{
			get
			{
				return rescanForAsteroids;
			}
			set
			{
				rescanForAsteroids = value;
			}
		}

		public MiningState State
		{
			get
			{
				return state;
			}
			set
			{
				state = value;
			}
		}

		public TimeSpan TimeSinceLastStageChange
		{
			get
			{
				return timeSinceLastStageChange;
			}
			set
			{
				timeSinceLastStageChange = value;
			}
		}


		public virtual void OnAccumulate(int wholeMineralsToExtract)
		{
			if(AccumulationEvent != null)
			{
				AccumulationEvent(new AccumulationEventArgs(wholeMineralsToExtract));
			}
		}
	}
}
