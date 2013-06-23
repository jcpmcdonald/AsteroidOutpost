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
		public Spin(int entityID, float rotationSpeed, bool rotateFrame = false)
			: base(entityID)
		{
			RotationSpeed = rotationSpeed;
			RotateFrame = rotateFrame;
		}


		public float RotationSpeed { get; private set; }
		public bool RotateFrame { get; set; }
	}
}
