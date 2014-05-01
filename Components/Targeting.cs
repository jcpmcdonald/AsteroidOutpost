using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsteroidOutpost.Components
{
	public class Targeting : Component
	{
		public Targeting(int entityID)
			: base(entityID)
		{
		}

		public String TargetingType { get; set; }
		public int? Target{ get; set; }
	}
}
