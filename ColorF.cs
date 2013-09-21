using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace AsteroidOutpost
{
	public struct ColorF
	{
		private float r;
		private float g;
		private float b;
		private float a;

		public ColorF(Color c)
		{
			r = MathHelper.Clamp(c.R, 0, 255);
			g = MathHelper.Clamp(c.G, 0, 255);
			b = MathHelper.Clamp(c.B, 0, 255);
			a = MathHelper.Clamp(c.A, 0, 255);
		}

		public ColorF(float r, float g, float b, float a = 255)
		{
			this.r = MathHelper.Clamp(r, 0, 255);
			this.g = MathHelper.Clamp(g, 0, 255);
			this.b = MathHelper.Clamp(b, 0, 255);
			this.a = MathHelper.Clamp(a, 0, 255);
		}


		public float R
		{
			get
			{
				return r;
			}
			set
			{
				r = MathHelper.Clamp(value, 0, 255);
			}
		}

		public float G
		{
			get
			{
				return g;
			}
			set
			{
				g = MathHelper.Clamp(value, 0, 255);
			}
		}

		public float B
		{
			get
			{
				return b;
			}
			set
			{
				b = MathHelper.Clamp(value, 0, 255);
			}
		}

		public float A
		{
			get
			{
				return a;
			}
			set
			{
				a = MathHelper.Clamp(value, 0, 255);
			}
		}

		public Color Color
		{
			get
			{
				return new Color((int)R, (int)G, (int)B, (int)A);
			}
			set
			{
				R = value.R;
				G = value.G;
				B = value.B;
				A = value.A;
			}
		}
	}
}
