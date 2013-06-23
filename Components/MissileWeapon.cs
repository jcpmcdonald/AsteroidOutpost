﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteroidOutpost.Interfaces;
using AsteroidOutpost.Screens;

namespace AsteroidOutpost.Components
{
	class MissileWeapon : Component, IWeapon
	{
		public MissileWeapon(int entityID) : base(entityID) {}
		public MissileWeapon(int entityID, int range, float damage, float fireRate, float acceleration)
			: base(entityID)
		{
			Range = range;
			Damage = damage;
			FireRate = fireRate;
			Acceleration = acceleration;
		}


		public int Range { get; set; }
		public float Damage { get; set; }
		public float FireRate { get; set; }
		public TimeSpan TimeSinceLastShot { get; set; }
		public float Acceleration { get; set; }

		public int? Target { get; set; }
	}
}
