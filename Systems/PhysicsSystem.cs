using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using AsteroidOutpost.Components;
using AsteroidOutpost.Screens;

namespace AsteroidOutpost.Systems
{
	public class PhysicsSystem : GameComponent
	{
		private readonly World world;

		public PhysicsSystem(AOGame game, World world)
			: base(game)
		{
			this.world = world;
		}

		public override void Update(GameTime gameTime)
		{
			TimeSpan deltaTime = gameTime.ElapsedGameTime;

			foreach (Velocity velocity in world.GetComponents<Velocity>())
			{
				Position position = world.GetComponent<Position>(velocity);
				if (velocity.CurrentVelocity != Vector2.Zero)
				{
					// Move!
					position.Center += velocity.CurrentVelocity * (float)deltaTime.TotalSeconds;
				}
			}

			base.Update(gameTime);
		}
	}
}
