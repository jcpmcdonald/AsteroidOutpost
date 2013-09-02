using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AsteroidOutpost.Screens;
using Microsoft.Xna.Framework;

namespace AsteroidOutpost.Components
{
	public class PowerProducer : Component
	{
		public PowerProducer(int entityID)
			: base(entityID)
		{
		}


		/// <summary>
		/// The rate at which this can produce power at, in power units per second
		/// </summary>
		public float PowerProductionRate { get; set; }
	}
}
