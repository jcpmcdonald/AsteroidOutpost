using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace AsteroidOutpost
{
	static class ColorExtension
	{
		public static Color Blend(this Color color1, Color color2)
		{
			return new Color((int)(color1.R * (color2.R / 255f)),
							 (int)(color1.G * (color2.G / 255f)),
							 (int)(color1.B * (color2.B / 255f)),
							 (int)(color1.A * (color2.A / 255f)));
		}
	}
}
