using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using C3.XNA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AsteroidOutpost
{
	public class FrameRateCounter
	{
		private DateTime startOfUpdateTime;
		private DateTime startOfDrawTime;

		private List<double> updateTimes = new List<double>();
		private readonly int updateTimesSize = 500;

		private List<double> drawTimes = new List<double>();
		private readonly int drawTimesSize = 500;

		//private int drawCount;
		//private TimeSpan fpsElapsedTime = TimeSpan.Zero;

		//private TimeSpan averageOverPeriod = new TimeSpan(0, 0, 0, 0, 500);
		//private List<TimeSpan> frameTime = new List<TimeSpan>(2000);
		//private TimeSpan lastFrameTimestamp = TimeSpan.Zero;


		//public int FPS
		//{
		//    get; private set;
		//}

		public double MillisecondsPerFrame
		{
			get
			{
				return updateTimes.Sum() / updateTimes.Count;
			}
		}

		public double LastUpdateTime()
		{
			if(updateTimes.Count > 0)
			{
				return updateTimes.Last();
			}
			else
			{
				return 0;
			}
		}


		public double LastDrawTime()
		{
			if(drawTimes.Count > 0)
			{
				return drawTimes.Last();
			}
			else
			{
				return 0;
			}
		}


		public FrameRateCounter(Game game)
			//: base(game)
		{
		}


		public void StartOfUpdate(GameTime gameTime)
		{
			startOfUpdateTime = DateTime.Now;
		}


		public void EndOfUpdate(GameTime gameTime)
		{
			Double updateTime = (DateTime.Now - startOfUpdateTime).TotalMilliseconds;

			// Don't add times that don't make sense. This will usually mean a break point or something terribly, terribly wrong
			if(updateTime < 500)
			{
				updateTimes.Add(updateTime);
				if(updateTimes.Count >= updateTimesSize)
				{
					//Console.WriteLine(updateTimesCircularIndex + " = " + updateTime);
					updateTimes.RemoveAt(0);
				}
			}
		}

		public void StartOfDraw(GameTime gameTime)
		{
			startOfDrawTime = DateTime.Now;


			//int x = 10;
			//int y = 50;
			//int w = updateTimesCircularSize;
			//int h = 50;
			//spriteBatch.FillRectangle(new Rectangle(x, y, w, h), new Color(0, 0, 0, 150));
			//spriteBatch.DrawRectangle(new Rectangle(x, y, w, h), Color.Gray);

			//double maxUpdateTime = updateTimes.Max();
			//for(int i = 0; i < updateTimes.Count; i++)
			//{
			//    int circularArrayIndex = (updateTimesCircularIndex + i) % updateTimesCircularSize;
			//    spriteBatch.DrawLine(x + i,
			//                         y + h,
			//                         x + i,
			//                         y + h - (int)((updateTimes[circularArrayIndex] / maxUpdateTime) * h),
			//                         Color.Red);
			//}



			//drawCount += 1;

			//if (lastFrameTimestamp > gameTime.TotalGameTime)
			//{
			//    Debugger.Break();
			//}

			//frameTime.Add(gameTime.TotalGameTime - lastFrameTimestamp);
			//while (frameTimeSum > averageOverPeriod)
			//{
			//    frameTime.RemoveAt(0);
			//}

			//lastFrameTimestamp = gameTime.TotalGameTime;
		}


		public void EndOfDraw()
		{
			Double drawTime = (DateTime.Now - startOfDrawTime).TotalMilliseconds;

			// Don't add times that don't make sense. This will usually mean a break point or something terribly, terribly wrong
			if(drawTime < 500)
			{
				drawTimes.Add(drawTime);
				if(drawTimes.Count >= drawTimesSize)
				{
					drawTimes.RemoveAt(0);
				}
			}
		}


		//private TimeSpan frameTimeSum
		//{
		//    get
		//    {
		//        TimeSpan sum = TimeSpan.Zero;
		//        foreach (var timeSpan in frameTime)
		//        {
		//            sum += timeSpan;
		//        }
		//        return sum;
		//    }
		//}

	}
}
