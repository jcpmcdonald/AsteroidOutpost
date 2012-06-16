using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace AsteroidOutpost
{
	public class FrameRateCounter : DrawableGameComponent
	{
		private int drawCount;
		private TimeSpan fpsElapsedTime = TimeSpan.Zero;

		private TimeSpan averageOverPeriod = new TimeSpan(0, 0, 0, 0, 500);
		private List<TimeSpan> frameTime = new List<TimeSpan>(2000);
		private TimeSpan lastFrameTimestamp = TimeSpan.Zero;


		public int FPS
		{
			get; private set;
		}

		public double MillisecondsPerFrame
		{
			get
			{
				return frameTimeSum.TotalMilliseconds / frameTime.Count;
			}
		}

		public FrameRateCounter(Game game)
			: base(game)
		{
		}


		public override void Update(GameTime gameTime)
		{
			fpsElapsedTime += gameTime.ElapsedGameTime;

			if (fpsElapsedTime.TotalSeconds >= 0.1)
			{
				fpsElapsedTime -= TimeSpan.FromSeconds(0.1);
				FPS = drawCount * 10;
				drawCount = 0;
			}
		}

		public override void Draw(GameTime gameTime)
		{
			drawCount += 1;

			if (lastFrameTimestamp > gameTime.TotalGameTime)
			{
				Debugger.Break();
			}

			frameTime.Add(gameTime.TotalGameTime - lastFrameTimestamp);
			while (frameTimeSum > averageOverPeriod)
			{
				frameTime.RemoveAt(0);
			}

			lastFrameTimestamp = gameTime.TotalGameTime;
		}


		private TimeSpan frameTimeSum
		{
			get
			{
				TimeSpan sum = TimeSpan.Zero;
				foreach (var timeSpan in frameTime)
				{
					sum += timeSpan;
				}
				return sum;
			}
		}

	}
}
