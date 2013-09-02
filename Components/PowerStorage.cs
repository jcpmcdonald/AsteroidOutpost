using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace AsteroidOutpost.Components
{
	internal class PowerStorage : Component
	{
		public PowerStorage(int entityID)
			: base(entityID)
		{
		}


		public float MaxPower { get; set; }
		public float AvailablePower { get; set; }


		/// <summary>
		/// Gets the power from this producer if able. Returns true if power was consumed
		/// </summary>
		/// <param name="amount">The amount of power to consume</param>
		/// <returns>Returns true if power was consumed, false otherwise</returns>
		public bool GetPower(float amount)
		{
			if (AvailablePower >= amount)
			{
				AvailablePower -= amount;
				return true;
			}
			else
			{
				return false;
			}
		}

	}
}
