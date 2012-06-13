using System;
using Microsoft.Xna.Framework;

namespace AsteroidOutpost
{
	public class FrameRateCounter : DrawableGameComponent
	{
		private Int32 counter;

		private TimeSpan elapsedTime = TimeSpan.Zero;

		public FrameRateCounter(Game game) : base(game) { }

		public Int32 FPS { get; private set; }

		public override void Update(GameTime gameTime)
		{
			elapsedTime += gameTime.ElapsedGameTime;

			if (elapsedTime.TotalSeconds >= 0.1)
			{
				elapsedTime -= TimeSpan.FromSeconds(0.1);
				FPS = counter * 10;
				counter = 0;
			}
		}

		public override void Draw(GameTime gameTime)
		{
			counter += 1;
		}
	}
}
