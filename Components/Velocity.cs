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
		public Velocity(int entityID) : base(entityID) {}
		public Velocity(int entityID, Vector2 velocity)
			: base(entityID)
		{
			this.CurrentVelocity = velocity;
		}


		/// <summary>
		/// Gets or sets the velocity
		/// </summary>
		public Vector2 CurrentVelocity { get; set; }
	}
}
