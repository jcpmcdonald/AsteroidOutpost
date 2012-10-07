using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteroidOutpost.Screens;
using Microsoft.Xna.Framework;

namespace AsteroidOutpost.Components
{
	class Velocity : Component
	{
		public Velocity(World world, int entityID, Vector2 velocity)
			: base(world, entityID)
		{
			this.CurrentVelocity = velocity;
		}


		/// <summary>
		/// Gets or sets the velocity
		/// </summary>
		public Vector2 CurrentVelocity { get; set; }
	}
}
