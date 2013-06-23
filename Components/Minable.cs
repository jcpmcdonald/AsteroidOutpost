using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AsteroidOutpost.Screens;
using Microsoft.Xna.Framework;

namespace AsteroidOutpost.Components
{
	class Minable : Component
	{
		private int minerals;
		private int startingMinerals;

		public Minable(int entityID) : base(entityID) { }
		public Minable(int entityID, int minerals)
			: base(entityID)
		{
			this.minerals = minerals;
			this.startingMinerals = minerals;
		}


		public int Minerals
		{
			get
			{
				return minerals;
			}
			set
			{
				minerals = Math.Max(0, value);
			}
		}


		public int StartingMinerals
		{
			get
			{
				return startingMinerals;
			}
			set
			{
				startingMinerals = Math.Max(0, value);
			}
		}
	}
}
