namespace AsteroidOutpost.Screens
{
	using Microsoft.Xna.Framework;
	using Microsoft.Xna.Framework.Graphics;

	/// <summary>
	/// Layer is an image scrolling in the background.
	/// </summary>
	internal class Layer
	{
		private Texture2D texture;
		private float speed;
		private Rectangle imgSize;
		private Vector2 position;
		private bool looping;
		private Color tint = Color.White;
		private float scale = 1.0f;


		/// <summary>
		/// Initializes a new instance of the <see cref="Layer"/> class.
		/// This constructor creates 'looping' type background layer, which fills viewport with texture.
		/// </summary>
		/// <param name="texture">The texture for layer.</param>
		/// <param name="speed">The speed of layer movement.</param>
		/// <param name="imgSize">Rectangle of the image in the texture.</param>
		public Layer(Texture2D texture, float speed, Rectangle imgSize)
		{
			init(texture, speed, imgSize, Vector2.Zero, true);
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="Layer"/> class.
		/// This constructor creates 'non-looping' type background layer, which is drawn at a given position.
		/// </summary>
		/// <param name="texture">The texture for layer.</param>
		/// <param name="speed">The speed of layer movement.</param>
		/// <param name="position">Positon of the texture.</param>
		public Layer(Texture2D texture, float speed, Vector2 position)
		{
			init(texture, speed, texture.Bounds, position, false);
		}


		private void init(Texture2D texture, float speed, Rectangle imgSize, Vector2 position, bool looping)
		{
			this.texture = texture;
			this.speed = speed;
			this.imgSize = imgSize;
			this.position = position;
			this.looping = looping;
		}


		/// <summary>
		/// Gets the rectange of a texture used for drawing.
		/// </summary>
		public Rectangle ImgSize
		{
			get
			{
				return imgSize;
			}
		}

		public float Speed
		{
			get
			{
				return speed;
			}
		}

		public Texture2D Texture
		{
			get
			{
				return texture;
			}
		}


		public Vector2 Position
		{
			get
			{
				return position;
			}
		}

		public Color Tint
		{
			get
			{
				return tint;
			}
			set
			{
				tint = value;
			}
		}

		public float Scale
		{
			get
			{
				return scale;
			}
			set
			{
				scale = value;
			}
		}


		public void Draw(SpriteBatch spriteBatch, Color tint, Vector2 offset)
		{

			if (looping)
			{
				Rectangle viewportRect = new Rectangle(0, 0, spriteBatch.GraphicsDevice.Viewport.Width, spriteBatch.GraphicsDevice.Viewport.Height);

				int gridsHorizontal = (int)((viewportRect.Width / (imgSize.Width * scale)) + 1) + 1;
				int gridsVertical = (int)((viewportRect.Height / (imgSize.Height * scale)) + 1) + 1;


				Vector2 start = //world.WorldToScreen(world.HUD.FocusWorldPoint)
					new Vector2(viewportRect.Center.X, viewportRect.Center.Y)
					+ new Vector2((offset.X * speed), (offset.Y * speed));

				start = new Vector2(start.X % (imgSize.Width * scale), start.Y % (imgSize.Height * scale))
				        - new Vector2((imgSize.Width * scale), (imgSize.Height * scale));

				start = start + new Vector2(viewportRect.X, viewportRect.Y);

				for (int x = 0; x < gridsHorizontal; x++)
				{
					for (int y = 0; y < gridsVertical; y++)
					{
						Vector2 destinationLocation = start + new Vector2(x * imgSize.Width * scale, y * imgSize.Height * scale);
						//spriteBatch.DrawRectangle(destinationLocation, new Vector2(imgSize.Width, imgSize.Height), Color.Red);

						spriteBatch.Draw(texture,
						                 destinationLocation,
						                 new Rectangle(imgSize.X, imgSize.Y, (int)(imgSize.Width * scale), (int)(imgSize.Height * scale)),
						                 tint.Blend(this.tint),
						                 0,
						                 Vector2.Zero,
						                 scale,
						                 SpriteEffects.None,
						                 0);
					}
				}

				//spriteBatch.DrawString(Fonts.ControlFont, "Width = " + gridsHorizontal, new Vector2(100), Color.White);
			}
			else
			{
				spriteBatch.Draw(texture,
				                 position + (offset * speed),
				                 null,
				                 tint.Blend(this.tint),
				                 0,
				                 Vector2.Zero,
				                 scale,
				                 SpriteEffects.None,
				                 0);
			}
		}
	}
}
