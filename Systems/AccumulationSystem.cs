using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteroidOutpost.Components;
using AsteroidOutpost.Screens;
using Microsoft.Xna.Framework;

namespace AsteroidOutpost.Systems
{
	public class AccumulationSystem : GameComponent
	{
		private readonly World world;

		private TimeSpan timeSinceLastPost = new TimeSpan(0);
		private int postFrequency;

		public AccumulationSystem(AOGame game, World world, int postFrequency)
			: base(game)
		{
			this.world = world;
			this.postFrequency = postFrequency;
		}


		public override void Update(GameTime gameTime)
		{
			timeSinceLastPost += gameTime.ElapsedGameTime;

			if (timeSinceLastPost.TotalMilliseconds > postFrequency)
			{
				timeSinceLastPost = timeSinceLastPost.Subtract(new TimeSpan(0, 0, 0, 0, postFrequency));

				List<Accumulator> accumulators = world.GetComponents<Accumulator>();
				foreach(Accumulator accumulator in accumulators)
				{
					if (accumulator.Value != 0)
					{
						// Be your own position
						//Position textPos = new Position(world, componentList, owningForce, position.Center, velocity);
						
						// Follow the parent's position
						//Position textPos = new PositionOffset(world, componentList, position, Vector2.Zero, velocity);

						FreeText freeText = new FreeText(world, accumulator.Offset, accumulator.Value.ToString("+0;-0;+0"), accumulator.Color, accumulator.FadeRate);
						world.HUD.AddComponent(freeText);

						accumulator.Value = 0;
					}
				}
			}

			base.Update(gameTime);
		}
	}
}
