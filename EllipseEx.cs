using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AsteroidOutpost.Screens;
using C3.XNA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace AsteroidOutpost
{
	static class EllipseEx
	{
		static Dictionary<String, Texture2D> textures = new Dictionary<string, Texture2D>(16);


		public static void LoadContent(GraphicsDevice graphicsDevice)
		{
			textures.Add("ellipse25", Texture2DEx.FromStreamWithPremultAlphas(graphicsDevice, File.OpenRead(@"..\Content\Ellipse25.png")));
			textures.Add("ellipse25bold", Texture2DEx.FromStreamWithPremultAlphas(graphicsDevice, File.OpenRead(@"..\Content\Ellipse25Bold.png")));
			textures.Add("ellipse25back", Texture2DEx.FromStreamWithPremultAlphas(graphicsDevice, File.OpenRead(@"..\Content\Ellipse25Back.png")));
			textures.Add("ellipse25front", Texture2DEx.FromStreamWithPremultAlphas(graphicsDevice, File.OpenRead(@"..\Content\Ellipse25Front.png")));
			textures.Add("ellipse50", Texture2DEx.FromStreamWithPremultAlphas(graphicsDevice, File.OpenRead(@"..\Content\Ellipse50.png")));
			textures.Add("ellipse50back", Texture2DEx.FromStreamWithPremultAlphas(graphicsDevice, File.OpenRead(@"..\Content\Ellipse50Back.png")));
			textures.Add("ellipse50front", Texture2DEx.FromStreamWithPremultAlphas(graphicsDevice, File.OpenRead(@"..\Content\Ellipse50Front.png")));
			textures.Add("ellipse100", Texture2DEx.FromStreamWithPremultAlphas(graphicsDevice, File.OpenRead(@"..\Content\Ellipse100.png")));
			textures.Add("ellipse220", Texture2DEx.FromStreamWithPremultAlphas(graphicsDevice, File.OpenRead(@"..\Content\Ellipse220.png")));
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
	}
}
