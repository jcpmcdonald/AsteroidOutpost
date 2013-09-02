using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using AsteroidOutpost.Screens.HeadsUpDisplay;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace AsteroidOutpost.Screens
{
	public class LayeredStarField : DrawableGameComponent
	{
		private List<Layer> starLayers = new List<Layer>();
		private List<Layer> anomalies = new List<Layer>();
		private List<Layer> distantAnomalies = new List<Layer>();
		private Vector2 offset = Vector2.Zero;
		private Vector2? lastFocusPoint;

		private AOGame aoGame;


		public LayeredStarField(Game game)
			: base(game)
		{
			this.aoGame = (AOGame)game;
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
			                        new Vector2(Game.GraphicsDevice.Viewport.Width * 0.3f, 500f)));
		}


		public override void Update(GameTime gameTime)
		{
			if (this.aoGame.World == null)
			{
				// world is not started yet, so do nothing
				return;
			}

			if (lastFocusPoint == null)
			{
				// can't really do anything
			}
			else
			{
				Vector2 delta = lastFocusPoint.Value - this.aoGame.World.HUD.FocusWorldPoint;
				Move(delta);
			}

			lastFocusPoint = this.aoGame.World.HUD.FocusWorldPoint;
		}





		public void Draw(SpriteBatch spriteBatch, Color tint)
		{
			spriteBatch.GraphicsDevice.Clear(Color.Black.Blend(tint));



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
}
