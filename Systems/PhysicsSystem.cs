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

		public override void Initialize()
		{

			base.Initialize();
		}

		public override void Update(GameTime gameTime)
		{
			TimeSpan deltaTime = gameTime.ElapsedGameTime;

			foreach (Position position in world.GetComponents<Position>())
			{
				if (position.Velocity != Vector2.Zero)
				{
					// Move!
					position.Center += position.Velocity * (float)deltaTime.TotalSeconds;
				}
			}

			base.Update(gameTime);
		}
	}
}
