using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace AsteroidOutpost.Components
{
	public class AIStrafe : Component
	{
		public enum StrafeState
		{
			Approach,
			ShootMissiles,
			GetClose,
			FireLasers
		}

		public AIStrafe(int entityID)
			: base(entityID)
		{
			MissileCount = 6;
		}


		public StrafeState State { get; set; }
		public int MissileCount { get; set; }
	}
}
