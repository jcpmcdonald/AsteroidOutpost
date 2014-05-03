

using System;
using Microsoft.Xna.Framework;

namespace AsteroidOutpost.Components
{
	class Projectile : Component
	{
		public float Damage { get; set; }
		public float AccelerationMagnitude { get; set; }
		public Vector2 AccelerationVector { get; set; }
		public int DetonationDistance { get; set; }
		public String TrailEffect { get; set; }
		public int TrailOffset { get; set; }


		public Projectile(int entityID) : base(entityID) {}
	}
}
