using System;
using System.Collections.Generic;
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
	public class LayeredStarField : Screen
	{
		private readonly AsteroidOutpostScreen theGame;
		List<Tuple<Texture2D, float, Rectangle>> starLayers = new List<Tuple<Texture2D, float, Rectangle>>();
		List<Tuple<Texture2D, float, Vector2>> anomalies = new List<Tuple<Texture2D, float, Vector2>>();
		private Vector2 offset = Vector2.Zero;
		private Vector2? lastFocusPoint;


		public LayeredStarField(ScreenManager theScreenManager, AsteroidOutpostScreen theGame) : base(theScreenManager)
		{
			this.theGame = theGame;
		}


		public override void LoadContent(SpriteBatch spriteBatch, ContentManager content)
		{
			base.LoadContent(spriteBatch, content);

			starLayers.Add(new Tuple<Texture2D, float, Rectangle>(content.Load<Texture2D>("Scenes\\stars1"),
			                                                      0.06f,
			                                                      new Rectangle(GlobalRandom.Next(64), GlobalRandom.Next(64), GlobalRandom.Next(128, 512 - 64), GlobalRandom.Next(128, 512 - 64))));

			starLayers.Add(new Tuple<Texture2D, float, Rectangle>(content.Load<Texture2D>("Scenes\\stars1"),
			                                                      0.04f,
			                                                      new Rectangle(GlobalRandom.Next(64), GlobalRandom.Next(64), GlobalRandom.Next(128, 512 - 64), GlobalRandom.Next(128, 512 - 64))));

			starLayers.Add(new Tuple<Texture2D, float, Rectangle>(content.Load<Texture2D>("Scenes\\stars1"),
			                                                      0.025f,
			                                                      new Rectangle(GlobalRandom.Next(64), GlobalRandom.Next(64), GlobalRandom.Next(128, 512 - 64), GlobalRandom.Next(128, 512 - 64))));

			starLayers.Add(new Tuple<Texture2D, float, Rectangle>(content.Load<Texture2D>("Scenes\\stars1"),
			                                                      0.0125f,
			                                                      new Rectangle(GlobalRandom.Next(64), GlobalRandom.Next(64), GlobalRandom.Next(128, 512 - 64), GlobalRandom.Next(128, 512 - 64))));

			anomalies.Add(new Tuple<Texture2D, float, Vector2>(content.Load<Texture2D>("Scenes\\IcePlanet"),
			                                                   0.04f,
			                                                   new Vector2(spriteBatch.GraphicsDevice.Viewport.Width * 0.3f, 300f)));
		}


		public void Update(TimeSpan deltaTime)
		{
			if (lastFocusPoint == null)
			{
				// can't really do anything
			}
			else
			{
				Vector2 delta = lastFocusPoint.Value - theGame.HUD.FocusWorldPoint;
				Move(delta);
			}

			lastFocusPoint = theGame.HUD.FocusWorldPoint;
		}


		


		public override void Draw(SpriteBatch spriteBatch, Color tint)
		{
			ScreenMan.GraphicsDevice.Clear(ColorPalette.ApplyTint(Color.Black, tint));
			
			foreach (var starLayer in starLayers)
			{
				Vector2 viewport = new Vector2(spriteBatch.GraphicsDevice.Viewport.Width, spriteBatch.GraphicsDevice.Viewport.Height);

				int gridsHorizontal = (int)((viewport.X / (starLayer.Item3.Width * 2f)) + 1) + 1;
				int gridsVertical = (int)((viewport.Y / (starLayer.Item3.Height * 2f)) + 1) + 1;

				Vector2 start = new Vector2((offset.X * starLayer.Item2) % (starLayer.Item3.Width * 2f),
				                            (offset.Y * starLayer.Item2) % (starLayer.Item3.Height * 2f)) + theGame.WorldToScreen(theGame.HUD.FocusWorldPoint) - (viewport / 2f);

				for (int x = 0; x < gridsHorizontal; x++)
				{
					for (int y = 0; y < gridsVertical; y++)
					{
						spriteBatch.Draw(starLayer.Item1,
						                 start + new Vector2(x * starLayer.Item3.Width * 2f, y * starLayer.Item3.Height * 2f), //(theGame.HUD.FocusWorldPoint + offset) * starLayer.Item2,
						                 starLayer.Item3,
						                 tint,
						                 0,
						                 Vector2.Zero,
						                 2f,
						                 SpriteEffects.None,
						                 0);
					}
				}
			}

			foreach (var anomaly in anomalies)
			{
				spriteBatch.Draw(anomaly.Item1, anomaly.Item3 + (offset * anomaly.Item2), null, tint, 0, Vector2.Zero, 1f, SpriteEffects.None, 0);
			}

			base.Draw(spriteBatch, tint);
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
}
