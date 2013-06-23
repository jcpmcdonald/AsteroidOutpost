using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AsteroidOutpost.Interfaces;
using AsteroidOutpost.Screens;
using Microsoft.Xna.Framework;

namespace AsteroidOutpost.Components
{
	class LaserWeapon : Component, IWeapon
	{
		//internal enum LaserState
		//{
		//    Idle,
		//    Shooting
		//}

		//public LaserState State { get; set; }
		public int Range { get; set; }
		public float Damage { get; set; }
		public int? Target { get; set; }
		public Color Color { get; set; }
		public bool Firing { get; set; }


		public LaserWeapon(int entityID) : base(entityID) {}
		public LaserWeapon(int entityID, int range, float damage, Color color)
			: base(entityID)
		{
			//State = LaserState.Idle;
			Range = range;
			Damage = damage;
			Color = color;
		}
	}
}
