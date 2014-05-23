using Microsoft.Xna.Framework;

namespace AsteroidOutpost.Extensions
{
	static class ColorEx
	{
		public static Color Blend(this Color color1, Color color2)
		{
			return new Color((int)(color1.R * (color2.R / 255f)),
			                 (int)(color1.G * (color2.G / 255f)),
			                 (int)(color1.B * (color2.B / 255f)),
			                 (int)(color1.A * (color2.A / 255f)));
		}


		public static Color Lerp(this Color color1, Color color2, float amount)
		{
			return new Color((int)(MathHelper.Lerp(color1.R, color2.R, amount)),
			                 (int)(MathHelper.Lerp(color1.G, color2.G, amount)),
			                 (int)(MathHelper.Lerp(color1.B, color2.B, amount)),
			                 (int)(MathHelper.Lerp(color1.A, color2.A, amount)));
		}


		public static Color Slerp(this Color color1, Color color2, float amount)
		{
			return new Color((int)(MathHelper.SmoothStep(color1.R, color2.R, amount)),
			                 (int)(MathHelper.SmoothStep(color1.G, color2.G, amount)),
			                 (int)(MathHelper.SmoothStep(color1.B, color2.B, amount)),
			                 (int)(MathHelper.SmoothStep(color1.A, color2.A, amount)));
		}
	}
}
