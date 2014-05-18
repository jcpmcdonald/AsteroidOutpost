using System;
using System.Collections.Generic;
using System.IO;
using C3.XNA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AsteroidOutpost.Extensions
{
	static class SpriteBatchEx
	{
		static Dictionary<String, Texture2D> textures = new Dictionary<string, Texture2D>(16);


		static Texture2D laserTexture;


		public static void LoadContent(GraphicsDevice graphicsDevice)
		{
			textures.Add("ellipse25", Texture2DEx.FromStreamWithPremultAlphas(graphicsDevice, File.OpenRead(@"..\data\images\Ellipse25.png")));
			textures.Add("ellipse25bold", Texture2DEx.FromStreamWithPremultAlphas(graphicsDevice, File.OpenRead(@"..\data\images\Ellipse25Bold.png")));
			textures.Add("ellipse25back", Texture2DEx.FromStreamWithPremultAlphas(graphicsDevice, File.OpenRead(@"..\data\images\Ellipse25Back.png")));
			textures.Add("ellipse25front", Texture2DEx.FromStreamWithPremultAlphas(graphicsDevice, File.OpenRead(@"..\data\images\Ellipse25Front.png")));
			textures.Add("ellipse50", Texture2DEx.FromStreamWithPremultAlphas(graphicsDevice, File.OpenRead(@"..\data\images\Ellipse50.png")));
			textures.Add("ellipse50back", Texture2DEx.FromStreamWithPremultAlphas(graphicsDevice, File.OpenRead(@"..\data\images\Ellipse50Back.png")));
			textures.Add("ellipse50front", Texture2DEx.FromStreamWithPremultAlphas(graphicsDevice, File.OpenRead(@"..\data\images\Ellipse50Front.png")));
			textures.Add("ellipse100", Texture2DEx.FromStreamWithPremultAlphas(graphicsDevice, File.OpenRead(@"..\data\images\Ellipse100.png")));
			textures.Add("ellipse220", Texture2DEx.FromStreamWithPremultAlphas(graphicsDevice, File.OpenRead(@"..\data\images\Ellipse220.png")));

			laserTexture = Texture2DEx.FromStreamWithPremultAlphas(graphicsDevice, File.OpenRead(@"..\data\images\WhitePowerBeam.png"));
		}


		public static void DrawEllipseBack(this SpriteBatch spriteBatch, Vector2 center, float radius, Color color)
		{
			float textureEllipseWidth;
			float textureWidth;

			if (radius < 35)
			{
				textureEllipseWidth = 25f;
				textureWidth = textureEllipseWidth + 10f;
			}
			else// if (radius < 80)
			{
				textureEllipseWidth = 50f;
				textureWidth = textureEllipseWidth + 10f;
			}


			float scale = (radius / textureEllipseWidth);
			spriteBatch.Draw(textures["ellipse" + textureEllipseWidth + "back"],
			                 center - (new Vector2(textureWidth * (float)Math.Sqrt(3), textureWidth) * scale),
			                 null,
			                 color,
			                 0,
			                 Vector2.Zero,
			                 scale,
			                 SpriteEffects.None,
			                 0);
		}


		public static void DrawEllipseFront(this SpriteBatch spriteBatch, Vector2 center, float radius, Color color, bool drawGuides = false)
		{
			float textureEllipseWidth;
			float textureWidth;

			if (radius < 35)
			{
				textureEllipseWidth = 25f;
				textureWidth = textureEllipseWidth + 10f;
				//float scale = (radius / 45f) * 2;
			}
			else// if (radius < 100)
			{
				textureEllipseWidth = 50f;
				textureWidth = textureEllipseWidth + 10f;
			}


			float scale = (radius / textureEllipseWidth);
			spriteBatch.Draw(textures["ellipse" + textureEllipseWidth + "front"],
			                 center - (new Vector2(textureWidth * (float)Math.Sqrt(3), textureWidth) * scale),
			                 null,
			                 color,
			                 0,
			                 Vector2.Zero,
			                 scale,
			                 SpriteEffects.None,
			                 0);


			if(drawGuides)
			{
				// Draw a bunch of elliptical guides for debugging purposes
				for (float theta = 0; theta < Math.PI * 2; theta += (float)Math.PI / 6f)
				{
					spriteBatch.DrawLine(center,
					                     center + new Vector2((float)Math.Sin(theta) * (float)Math.Sqrt(3),
					                                          (float)Math.Cos(theta)) * radius,
					                     Color.White);
				}
			}
		}


		public static void DrawEllipse(this SpriteBatch spriteBatch, Vector2 center, float radius, Color color, bool drawGuides = false)
		{
			float textureEllipseWidth;
			float textureWidth;

			if (radius < 35)
			{
				textureEllipseWidth = 25f;
				textureWidth = textureEllipseWidth + 10f;
				//float scale = (radius / 45f) * 2;
			}
			else if (radius < 80)
			{
				textureEllipseWidth = 50f;
				textureWidth = textureEllipseWidth + 10f;
			}
			else if (radius <= 130)
			{
				textureEllipseWidth = 100f;
				textureWidth = textureEllipseWidth + 10f;
			}
			else// if(Radius <= 250)
			{
				textureEllipseWidth = 220f;
				textureWidth = textureEllipseWidth + 10f;
			}


			float scale = (radius / textureEllipseWidth);
			spriteBatch.Draw(textures["ellipse" + textureEllipseWidth],
			                 center - (new Vector2(textureWidth * (float)Math.Sqrt(3), textureWidth) * scale),
			                 null,
			                 color,
			                 0,
			                 Vector2.Zero,
			                 scale,
			                 SpriteEffects.None,
			                 0);


			if(drawGuides)
			{
				// Draw a bunch of elliptical guides for debugging purposes
				for (float theta = 0; theta < Math.PI * 2; theta += (float)Math.PI / 6f)
				{
					spriteBatch.DrawLine(center,
					                     center + new Vector2((float)Math.Sin(theta) * (float)Math.Sqrt(3),
					                                          (float)Math.Cos(theta)) * radius,
					                     Color.White);
				}
			}
		}


		/// <summary>
		///  This defines what a scaling function should look like
		/// </summary>
		/// <param name="val">The value to be scaled</param>
		/// <returns>The scaled value</returns>
		internal delegate float ScaleFunction(float val);


		/// <summary>
		/// Draw a laser beam
		/// </summary>
		/// <param name="spriteBatch">The spritebatch to use. This method can be invoked directly from the sprite batch</param>
		/// <param name="start">The starting vector</param>
		/// <param name="end">The ending vector</param>
		/// <param name="color">The color of the beam</param>
		/// <param name="scaleDelegate">The scaling function to use (tip: world.Scale)</param>
		/// <param name="width">The width of the line. 1 is "normal" width, 0.5 is half that, 2 is twice that, etc</param>
		/// <param name="percentLength">The percentage of the way between Start and End to draw. Defaults to 1  (100%)</param>
		public static void DrawLaser(this SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, ScaleFunction scaleDelegate, float width, float percentLength = 1f)
		{
			spriteBatch.Draw(laserTexture,
			                 start,
			                 null,
			                 color,
			                 (float)Math.Atan2(end.Y - start.Y, end.X - start.X),
			                 new Vector2(0f, (float)laserTexture.Height / 2),
			                 new Vector2((Vector2.Distance(start, end) * percentLength) / laserTexture.Width, scaleDelegate(0.25f)),
			                 SpriteEffects.None,
			                 0f);
		}
	}
}
