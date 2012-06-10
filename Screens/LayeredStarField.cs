using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using AsteroidOutpost.Screens.HeadsUpDisplay;
using C3.XNA;
using C3.XNA.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace AsteroidOutpost.Screens
{
	public class LayeredStarField : DrawableGameComponent
	{
		List<Layer> starLayers = new List<Layer>();
		List<Layer> anomalies = new List<Layer>();
		List<Layer> distantAnomalies = new List<Layer>();
		private Vector2 offset = Vector2.Zero;
		private Vector2? lastFocusPoint;


		public LayeredStarField(Game game) : base(game)
		{
		}


		protected override void LoadContent()
		{
			/*
			starLayers.Add(new Tuple<Texture2D, float, Rectangle>(content.Load<Texture2D>("Scenes\\stars1"),
			                                                      0.06f,
			                                                      new Rectangle(GlobalRandom.Next(64), GlobalRandom.Next(64), GlobalRandom.Next(128, 512 - 64), GlobalRandom.Next(128, 512 - 64))));
			*/
			starLayers.Add(new Layer(Game.Content.Load<Texture2D>("Scenes\\stars1"),
			                         0.04f,
			                         new Rectangle(GlobalRandom.Next(64), GlobalRandom.Next(64), GlobalRandom.Next(128, 512 - 64), GlobalRandom.Next(128, 512 - 64))));

			starLayers.Add(new Layer(Game.Content.Load<Texture2D>("Scenes\\stars1"),
			                         0.025f,
			                         new Rectangle(GlobalRandom.Next(64), GlobalRandom.Next(64), GlobalRandom.Next(128, 512 - 64), GlobalRandom.Next(128, 512 - 64))));
			
			starLayers.Add(new Layer(Game.Content.Load<Texture2D>("Scenes\\stars1"),
			                         0.0125f,
			                         new Rectangle(GlobalRandom.Next(64), GlobalRandom.Next(64), GlobalRandom.Next(128, 512 - 64), GlobalRandom.Next(128, 512 - 64))));


			Layer nebula = new Layer(Game.Content.Load<Texture2D>("Scenes\\SpaceNebula"),
			                         0.03f,
			                         new Vector2(-1500, -1500));
			nebula.Tint = new Color(200, 200, 200, 100);
			nebula.Scale = 2f;
			distantAnomalies.Add(nebula);

			anomalies.Add(new Layer(Game.Content.Load<Texture2D>("Scenes\\IcePlanet"),
			                        0.04f,
			                        new Vector2(Game.GraphicsDevice.Viewport.Width * 0.3f, 300f)));
		}


		public override void Update(GameTime gameTime)
		{
			//if (lastFocusPoint == null)
			//{
			//    // can't really do anything
			//}
			//else
			//{
			//    Vector2 delta = lastFocusPoint.Value - world.HUD.FocusWorldPoint;
			//    Move(delta);
			//}

			//lastFocusPoint = world.HUD.FocusWorldPoint;
		}


		


		public void Draw(SpriteBatch spriteBatch, Color tint)
		{
			spriteBatch.GraphicsDevice.Clear(ColorPalette.ApplyTint(Color.Black, tint));

			

			foreach (var starLayer in starLayers)
			{
				starLayer.Draw(spriteBatch, tint, offset);
			}

			foreach (var distantAnomaly in distantAnomalies)
			{
				distantAnomaly.Draw(spriteBatch, tint, offset);
				//spriteBatch.Draw(distantAnomaly.Item1, distantAnomaly.Item3 + (offset * distantAnomaly.Item2), null, ColorPalette.ApplyTint(tint, new Color(200, 200, 200, 100)), 0, Vector2.Zero, 1f, SpriteEffects.None, 0);
			}

			foreach (var anomaly in anomalies)
			{
				anomaly.Draw(spriteBatch, tint, offset);
				//spriteBatch.Draw(anomaly.Item1, anomaly.Item3 + (offset * anomaly.Item2), null, tint, 0, Vector2.Zero, 1f, SpriteEffects.None, 0);
			}
		}

		public void Move(float x, float y)
		{
			Move(new Vector2(x, y));
		}

		public void Move(Vector2 delta)
		{
			offset += delta;
		}
	}




	class Layer
	{
		private Texture2D texture;
		private float speed;
		private Rectangle imgSize;
		private Vector2 position;
		private bool looping;
		private Color tint = Color.White;
		private float scale = 1.0f;


		public Layer(Texture2D texture, float speed, Rectangle imgSize)
		{
			init(texture, speed, imgSize, Vector2.Zero, true);
		}


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
#if true
				Rectangle viewportRect = new Rectangle(0, 0, spriteBatch.GraphicsDevice.Viewport.Width, spriteBatch.GraphicsDevice.Viewport.Height);
#else
				Rectangle viewportRect = new Rectangle(200, 200, 640, 480);
				spriteBatch.DrawRectangle(viewportRect, Color.Green);
				spriteBatch.FillRectangle(new Rectangle(viewportRect.Center.X - 2, viewportRect.Center.Y -2, 4, 4), Color.Green);
#endif

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
						                 ColorPalette.ApplyTint(tint, this.tint),
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
				                 ColorPalette.ApplyTint(tint, this.tint),
				                 0,
				                 Vector2.Zero,
				                 scale,
				                 SpriteEffects.None,
				                 0);
			}

		}
	}
}
