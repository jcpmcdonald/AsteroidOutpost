using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteroidOutpost.Screens;

namespace AsteroidOutpost.Components
{
	class Spin : Component
	{
		public Spin(int entityID) : base(entityID) {}

		public float RotationSpeed { get; set; }
		public bool RotateFrame { get; set; }
	}
}
