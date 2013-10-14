using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsteroidOutpost
{
	static class GlobalRandom
	{
		private static Random rand = new Random();
		
		public static Random Rand
		{
			get
			{
				return rand;
			}
		}

		public static int Next()
		{
			return rand.Next();
		}

		public static int Next(int max)
		{
			return rand.Next(max);
		}

		public static int Next(int min, int max)
		{
			return rand.Next(min, max);
		}

		public static float Next(float min, float max)
		{
			return min + ((float)rand.NextDouble() * (max - min));
		}

		public static double NextDouble()
		{
			return rand.NextDouble();
		}
	}
}
