using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsteroidOutpost.Components
{
	public class ScienceVessel : Component
	{
		public ScienceVessel(int entityID)
				: base(entityID)
		{
		}


		public float PowerConsumptionRate { get; set; }

		//public float MaxPowerStorage { get; set; }
		//public float StoredPower { get; set; }
		public float StoredPowerReplenishRate { get; set; }

		public bool Overload { get; set; }
	}
}
