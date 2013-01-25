using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteroidOutpost.Screens;

namespace AsteroidOutpost.Components
{
	class MissileProjectile : Component
	{
		public int? Target { get; set; }
		public int Damage { get; set; }
		public float Acceleration { get; set; }
		public int DetonationDistance { get; set; }


		public MissileProjectile(World world, int entityID, int damage, float acceleration, int detonationDistance, int? target)
			: base(world, entityID)
		{
			Target = target;
			Damage = damage;
			Acceleration = acceleration;
			DetonationDistance = detonationDistance;
		}
	}
}
