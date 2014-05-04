using System;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace AsteroidOutpost.Components
{
	class ProjectileLauncher : Component
	{
		internal enum FiringMode
		{
			FireAtWill,
			HoldFire
		}

		public ProjectileLauncher(int entityID) : base(entityID) {}


		public String ProjectileType { get; set; }
		public int Range { get; set; }
		public float FireRate { get; set; }
		public float? Damage { get; set; }
		public float InitialVelocityMin { get; set; }
		public float InitialVelocityMax { get; set; }
		public float Spray { get; set; }

		public FiringMode FireMode { get; set; }


		[XmlIgnore]
		[JsonIgnore]
		public TimeSpan TimeSinceLastShot { get; set; }
	}
}
