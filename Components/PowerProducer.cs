using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AsteroidOutpost.Screens;

namespace AsteroidOutpost.Components
{
	public class PowerProducer : PowerGridNode
	{
		public PowerProducer(World world)
			: base(world)
		{
		}


		protected PowerProducer(BinaryReader br)
			: base(br)
		{
		}


		public float AvailablePower { get; set; }

		public bool GetPower(float amount)
		{
			return false;
		}
	}
}
