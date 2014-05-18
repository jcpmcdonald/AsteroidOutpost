using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AsteroidOutpost.Components;
using AsteroidOutpost.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AsteroidOutpost.Systems
{
	public class PowerStorageSystem : DrawableGameComponent
	{
		private World world;
		private SpriteBatch spriteBatch;
		private Texture2D powerBar;

		public PowerStorageSystem(AOGame game, World world)
			: base(game)
		{
			this.world = world;
			spriteBatch = new SpriteBatch(game.GraphicsDevice);
		}


		protected override void LoadContent()
		{
			powerBar = Texture2DEx.FromStreamWithPremultAlphas(Game.GraphicsDevice, File.OpenRead(@"..\data\images\PowerBar.png"));
			base.LoadContent();
		}


		
		public override void Draw(GameTime gameTime)
		{
			spriteBatch.Begin();

			// Draw power level bars
			foreach (var storage in world.GetComponents<PowerStorage>())
			{
				Position storagePosition = world.GetComponent<Position>(storage);

				const float invisiblePoint = 1.7f;
				const float fadePoint = 1.2f;
				if (world.ScaleFactor < invisiblePoint)
				{
					// Default to completely visible
					float fadePercent = 0.0f;
					if (world.ScaleFactor > fadePoint)
					{
						// Fade out as we get further away
						fadePercent = (world.ScaleFactor - fadePoint) / (invisiblePoint - fadePoint);
					}

					float percentFull = storage.AvailablePower / storage.MaxPower;
					float scale = 0.4f;
					int fillToHeight = (int)((powerBar.Height * percentFull) + 0.5f);
					int barHeight = (int)(powerBar.Height * scale);

					// Draw the depleted part of the bar
					spriteBatch.Draw(powerBar,
					                 world.WorldToScreen(new Vector2(storagePosition.Left + 10,
					                                                 storagePosition.Center.Y - (barHeight / 2.0f))),
					                 new Rectangle(0,
					                               0,
					                               powerBar.Width,
					                               powerBar.Height - fillToHeight),
					                 Color.White * (1 - fadePercent),
					                 0f,
					                 Vector2.Zero,
					                 world.Scale(scale),
					                 SpriteEffects.None,
					                 0);

					// Draw the available part of the bar
					spriteBatch.Draw(powerBar,
					                 world.WorldToScreen(new Vector2(storagePosition.Left + 10,
					                                                 storagePosition.Center.Y - (barHeight / 2.0f) + ((powerBar.Height - fillToHeight) * scale))),
					                 new Rectangle(0,
					                               powerBar.Height - fillToHeight,
					                               powerBar.Width,
					                               fillToHeight),
					                 Color.Green * (1 - fadePercent),
					                 0f,
					                 Vector2.Zero,
					                 world.Scale(scale),
					                 SpriteEffects.None,
					                 0);
				}
			}

			spriteBatch.End();
		}

	}
}
