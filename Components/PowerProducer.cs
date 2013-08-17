﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AsteroidOutpost.Screens;
using Microsoft.Xna.Framework;

namespace AsteroidOutpost.Components
{
	public class PowerProducer : PowerGridNode
	{
		private float availablePower;

		public PowerProducer(int entityID) : base(entityID, true)
		{
			ProducesPower = true;
		}
		public PowerProducer(int entityID, int powerProductionRate, int maxPower)
			: base(entityID, true)
		{
			ProducesPower = true;
			PowerProductionRate = powerProductionRate;
			MaxPower = maxPower;
		}
		
		public float MaxPower { get; set; }
		public float AvailablePower
		{
			get
			{
				return availablePower;
			}
			set
			{
				availablePower = MathHelper.Clamp(value, 0, MaxPower);
			}
		}


		/// <summary>
		/// The rate at which this can produce power at, in power units per second
		/// </summary>
		public float PowerProductionRate { get; set; }


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
