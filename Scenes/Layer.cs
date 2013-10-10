using System;
using System.IO;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AsteroidOutpost.Scenes
{
	/// <summary>
	/// Layer is an image scrolling in the background.
	/// </summary>
	public class Layer
	{
		public String TexturePath { get; set; }
		public float Distance { get; set; }
		public Color Tint { get; set; }
		public float Scale { get; set; }
		
		// If set, this layer will be tiled
		public bool Tiled { get; set; }

		// The position of layer if it's not tiled
		public Vector2 Position { get; set; }


		[JsonIgnore][XmlIgnore]
		public Texture2D Texture { get; set; }



		public Layer()
		{
			Tint = Color.White;
			Scale = 1.0f;
		}


		public void LoadContent(GraphicsDevice graphicsDevice)
		{
			Texture = Texture2DEx.FromStreamWithPremultAlphas(graphicsDevice, File.OpenRead(@"..\data\scenes\" + TexturePath));
		}


		public void Draw(SpriteBatch spriteBatch, Vector2 offset)
		{

			if (Tiled)
			{
				Rectangle viewportRect = new Rectangle(0, 0, spriteBatch.GraphicsDevice.Viewport.Width, spriteBatch.GraphicsDevice.Viewport.Height);

				int gridsHorizontal = (int)((viewportRect.Width / (Texture.Width * Scale)) + 1) + 1;
				int gridsVertical = (int)((viewportRect.Height / (Texture.Height * Scale)) + 1) + 1;


				Vector2 start = //world.WorldToScreen(world.HUD.FocusWorldPoint)
					new Vector2(viewportRect.Center.X, viewportRect.Center.Y)
					+ new Vector2((offset.X / Distance), (offset.Y / Distance));

				start = new Vector2(start.X % (Texture.Width * Scale), start.Y % (Texture.Height * Scale))
				        - new Vector2((Texture.Width * Scale), (Texture.Height * Scale));

				start = start + new Vector2(viewportRect.X, viewportRect.Y);

				for (int x = 0; x < gridsHorizontal; x++)
				{
					for (int y = 0; y < gridsVertical; y++)
					{
						Vector2 destinationLocation = start + new Vector2(x * Texture.Width * Scale, y * Texture.Height * Scale);
						//spriteBatch.DrawRectangle(destinationLocation, new Vector2(imgSize.Width, imgSize.Height), Color.Red);

						spriteBatch.Draw(Texture,
						                 destinationLocation,
						                 new Rectangle(0, 0, (int)(Texture.Width * Scale), (int)(Texture.Height * Scale)),
						                 Tint,
						                 0,
						                 Vector2.Zero,
						                 Scale,
						                 SpriteEffects.None,
						                 0);
					}
				}

				//spriteBatch.DrawString(Fonts.ControlFont, "Width = " + gridsHorizontal, new Vector2(100), Color.White);
			}
			else
			{
				spriteBatch.Draw(Texture,
				                 Position + (offset / Distance),
				                 null,
				                 Tint,
				                 0,
				                 Vector2.Zero,
				                 Scale,
				                 SpriteEffects.None,
				                 0);
			}
		}
	}
}
