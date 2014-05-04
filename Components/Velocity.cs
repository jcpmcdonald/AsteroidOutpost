using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using AsteroidOutpost.Screens;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace AsteroidOutpost.Components
{
	class Velocity : Component
	{
		public Velocity(int entityID)
				: base(entityID)
		{
			AccelerationMagnitude = 10f;
		}


		/// <summary>
		/// Gets or sets the velocity
		/// </summary>
		public Vector2 CurrentVelocity { get; set; }

		public float AccelerationMagnitude { get; set; }
		public Vector2 AccelerationVector { get; set; }


		////////////////////////////////
		// Helper functions
		////////////////////////////////
		
		public float MinDistanceToStop()
		{
			return (float)(0.5 * AccelerationMagnitude * Math.Pow(CurrentVelocity.Length() / AccelerationMagnitude, 2.0));
		}

		public void Decelerate(GameTime gameTime)
		{
			if (CurrentVelocity != Vector2.Zero)
			{
				if ((AccelerationMagnitude * (float)gameTime.ElapsedGameTime.TotalSeconds) >= CurrentVelocity.Length())
				{
					CurrentVelocity = Vector2.Zero;
				}
				else
				{
					CurrentVelocity -= Vector2.Normalize(CurrentVelocity) * AccelerationMagnitude * (float)gameTime.ElapsedGameTime.TotalSeconds;
				}
			}
		}
	}
}
