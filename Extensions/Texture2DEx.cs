using System;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace AsteroidOutpost.Extensions
{
	public static class Texture2DEx
	{
		public static Texture2D FromStreamWithPremultAlphas(GraphicsDevice graphicsDevice, Stream stream)
		{
			Texture2D texture = Texture2D.FromStream(graphicsDevice, stream);
			PreMultiplyAlphas(texture);
			return texture;
		}

		private static void PreMultiplyAlphas(Texture2D ret)
		{
			var data = new Byte4[ret.Width * ret.Height];
			ret.GetData(data);
			for (var i = 0; i < data.Length; i++)
			{
				var vec = data[i].ToVector4();
				var alpha = vec.W / 255.0f;
				var a = (Int32)(vec.W);
				var r = (Int32)(alpha * vec.X);
				var g = (Int32)(alpha * vec.Y);
				var b = (Int32)(alpha * vec.Z);
				data[i].PackedValue = (UInt32)((a << 24) + (b << 16) + (g << 8) + r);
			}
			ret.SetData(data);
		}

	}
}
