using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteroidOutpost.Screens;

namespace AsteroidOutpost.Components
{
	class Spin : Component
	{
		public Spin(World world, int entityID, float rotationSpeed)
			: base(world, entityID)
		{
			this.RotationSpeed = rotationSpeed;
		}

		public float RotationSpeed { get; private set; }
	}
}
