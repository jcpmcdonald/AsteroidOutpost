using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace AsteroidOutpost.Components
{
	public class AIStrafe : Component
	{
		public enum StrafeState
		{
			Approach,
			ShootMissiles,
			GetClose,
			FireLasers
		}

		public AIStrafe(int entityID)
			: base(entityID)
		{
			MissileCount = 6;
		}


		public StrafeState State { get; set; }
		public int MissileCount { get; set; }

		public Vector2 StrafeVelocity { get; set; }
		public float StrafeAccelerationMagnitude { get; set; }



		public void DampenStrafeVelocity(GameTime gameTime)
		{
			if (StrafeVelocity != Vector2.Zero)
			{
				if ((StrafeAccelerationMagnitude * (float)gameTime.ElapsedGameTime.TotalSeconds) >= StrafeVelocity.Length())
				{
					StrafeVelocity = Vector2.Zero;
				}
				else
				{
					StrafeVelocity -= Vector2.Normalize(StrafeVelocity) * StrafeAccelerationMagnitude * 10f * (float)gameTime.ElapsedGameTime.TotalSeconds;
				}
			}
		}
	}
}
