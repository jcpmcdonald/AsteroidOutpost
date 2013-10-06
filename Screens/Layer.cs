namespace AsteroidOutpost.Screens
{
	using Microsoft.Xna.Framework;
	using Microsoft.Xna.Framework.Graphics;

	/// <summary>
	/// Layer is an image scrolling in the background.
	/// </summary>
	internal class Layer
	{
		public Texture2D Texture { get; set; }
		public float Speed { get; set; }
		public Vector2 Position { get; set; }
		public bool Tile { get; set; }
		public Rectangle TileRect { get; set; }
		public Color Tint { get; set; }
		public float Scale { get; set; }


		public Layer()
		{
			Tint = Color.White;
			Scale = 1.0f;
		}


		public void Draw(SpriteBatch spriteBatch, Vector2 offset)
		{

			if (Tile)
			{
				Rectangle viewportRect = new Rectangle(0, 0, spriteBatch.GraphicsDevice.Viewport.Width, spriteBatch.GraphicsDevice.Viewport.Height);

				int gridsHorizontal = (int)((viewportRect.Width / (TileRect.Width * Scale)) + 1) + 1;
				int gridsVertical = (int)((viewportRect.Height / (TileRect.Height * Scale)) + 1) + 1;


				Vector2 start = //world.WorldToScreen(world.HUD.FocusWorldPoint)
					new Vector2(viewportRect.Center.X, viewportRect.Center.Y)
					+ new Vector2((offset.X * Speed), (offset.Y * Speed));

				start = new Vector2(start.X % (TileRect.Width * Scale), start.Y % (TileRect.Height * Scale))
				        - new Vector2((TileRect.Width * Scale), (TileRect.Height * Scale));

				start = start + new Vector2(viewportRect.X, viewportRect.Y);

				for (int x = 0; x < gridsHorizontal; x++)
				{
					for (int y = 0; y < gridsVertical; y++)
					{
						Vector2 destinationLocation = start + new Vector2(x * TileRect.Width * Scale, y * TileRect.Height * Scale);
						//spriteBatch.DrawRectangle(destinationLocation, new Vector2(imgSize.Width, imgSize.Height), Color.Red);

						spriteBatch.Draw(Texture,
						                 destinationLocation,
						                 new Rectangle(TileRect.X, TileRect.Y, (int)(TileRect.Width * Scale), (int)(TileRect.Height * Scale)),
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
				                 Position + (offset * Speed),
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
