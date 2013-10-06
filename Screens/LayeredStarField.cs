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
		//private List<Layer> starLayers = new List<Layer>();
		//private List<Layer> anomalies = new List<Layer>();
		//private List<Layer> distantAnomalies = new List<Layer>();
		private List<Layer> layers = new List<Layer>();

		private Vector2 offset = Vector2.Zero;
		private Vector2? lastFocusPoint;

		private AOGame aoGame;
		private SpriteBatch spriteBatch;


		public LayeredStarField(Game game)
			: base(game)
		{
			this.aoGame = (AOGame)game;
		}


		protected override void LoadContent()
		{
			spriteBatch = new SpriteBatch(aoGame.GraphicsDevice);

			/*
			starLayers.Add(new Tuple<Texture2D, float, Rectangle>(content.Load<Texture2D>("Scenes\\stars1"),
			                                                      0.06f,
			                                                      new Rectangle(GlobalRandom.Next(64), GlobalRandom.Next(64), GlobalRandom.Next(128, 512 - 64), GlobalRandom.Next(128, 512 - 64))));
			*/
			layers.Add(new Layer(){
				Texture = Game.Content.Load<Texture2D>("Scenes\\stars1"),
				Speed = 0.04f,
				Tile = true,
				TileRect = new Rectangle(GlobalRandom.Next(64),
				                           GlobalRandom.Next(64),
				                           GlobalRandom.Next(128, 512 - 64),
				                           GlobalRandom.Next(128, 512 - 64))
			});

			layers.Add(new Layer(){
				Texture = Game.Content.Load<Texture2D>("Scenes\\stars1"),
				Speed = 0.025f,
				Tile = true,
				TileRect = new Rectangle(GlobalRandom.Next(64),
				                           GlobalRandom.Next(64),
				                           GlobalRandom.Next(128, 512 - 64),
				                           GlobalRandom.Next(128, 512 - 64))
			});

			layers.Add(new Layer(){
				Texture = Game.Content.Load<Texture2D>("Scenes\\stars1"),
				Speed = 0.0125f,
				Tile = true,
				TileRect = new Rectangle(GlobalRandom.Next(64),
				                           GlobalRandom.Next(64),
				                           GlobalRandom.Next(128, 512 - 64),
				                           GlobalRandom.Next(128, 512 - 64))
			});

			layers.Add(new Layer(){
				Texture = Game.Content.Load<Texture2D>("Scenes\\SpaceNebula"),
				Speed = 0.03f,
				Tint = new Color(200, 200, 200, 100),
				Scale = 2f,
				Position = new Vector2(-1500, -1500)
			});

			layers.Add(new Layer(){
				Texture = Game.Content.Load<Texture2D>("Scenes\\IcePlanet"),
				Speed = 0.04f,
				Position = new Vector2(Game.GraphicsDevice.Viewport.Width * 0.3f, 500f)
			});
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


		public override void Draw(GameTime gameTime)
		{
			spriteBatch.Begin();
			spriteBatch.GraphicsDevice.Clear(Color.Black);

			foreach (var layer in layers)
			{
				layer.Draw(spriteBatch, offset);
			}
			spriteBatch.End();

			base.Draw(gameTime);
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
