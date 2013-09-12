using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace AsteroidOutpost.Components
{
	public class AutoHeal : Component
	{
		public float Delay { get; set; }
		public float Rate { get; set; }

		[JsonIgnore]
		public TimeSpan TimeSinceLastHit { get; set; }

		public AutoHeal(int entityID)
			: base(entityID)
		{
		}
	}
}
