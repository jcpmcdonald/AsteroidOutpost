using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AsteroidOutpost.Interfaces;
using AsteroidOutpost.Screens;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace AsteroidOutpost.Components
{
	class LaserWeapon : Component, IWeapon
	{
		public int Range { get; set; }
		public float Damage { get; set; }
		public Color Color { get; set; }
		public float PowerUsageRate { get; set; }

		[JsonIgnore]
		public bool HasPower { get; set; }

		public LaserWeapon(int entityID) : base(entityID) {}
	}
}
