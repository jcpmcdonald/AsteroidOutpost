using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AsteroidOutpost.Screens;
using Microsoft.Xna.Framework;

namespace AsteroidOutpost.Components
{
	class FleetMovementBehaviour : Component
	{

		public FleetMovementBehaviour(World world, int entityID)
			: base(world, entityID)
		{
			AccelerationMagnitude = 10f;
		}


		public int FleetID { get; set; }
		public float AccelerationMagnitude { get; set; }
		public Vector2 AccelerationVector { get; set; }
		public int? Target { get; set; }
	}
}
