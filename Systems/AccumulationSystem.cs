using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteroidOutpost.Components;
using AsteroidOutpost.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace AsteroidOutpost.Systems
{
	public class AccumulationSystem : DrawableGameComponent
	{
		private SpriteBatch spriteBatch;
		private readonly World world;

		private TimeSpan timeSinceLastPost = new TimeSpan(0);
		private int postFrequency;

		private static SpriteFont font;


		public AccumulationSystem(AOGame game, World world, int postFrequency)
			: base(game)
		{
			spriteBatch = new SpriteBatch(game.GraphicsDevice);
			this.world = world;
			this.postFrequency = postFrequency;
		}


		/// <summary>
		/// This is where all entities should do any resource loading that will be required. This will be called once per game.
		/// </summary>
		/// <param name="graphicsDevice">The graphics device</param>
		/// <param name="content">The content manager</param>
		protected override void LoadContent()
		{
			font = Game.Content.Load<SpriteFont>("Fonts\\ControlFont");
		}


		public override void Update(GameTime gameTime)
		{
			// Update the floating text objects
			List<FloatingText> floatingTexts = world.GetComponents<FloatingText>();
			foreach (var floatingText in floatingTexts)
			{
				//floatingText.CumulativeTime += gameTime.ElapsedGameTime;
				float fadeAmount = floatingText.FadeRate * (float)gameTime.ElapsedGameTime.TotalSeconds;
				floatingText.Offset += floatingText.Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;

				floatingText.Color = new ColorF(floatingText.Color.R - fadeAmount,
				                                floatingText.Color.G - fadeAmount,
				                                floatingText.Color.B - fadeAmount,
				                                floatingText.Color.A - fadeAmount);

				//Console.WriteLine(String.Format("{0} {1} {2} {3}", floatingText.Color.R, floatingText.Color.G, floatingText.Color.B, floatingText.Color.A));

				if (floatingText.Color.A <= 0 ||
				    (floatingText.Color.R <= 0 &&
				     floatingText.Color.G <= 0 &&
				     floatingText.Color.B <= 0))
				{
					//floatingText.SetDead(true, true);
					world.DeleteComponent(floatingText);
				}
			}


			// Create new floating text objects
			timeSinceLastPost += gameTime.ElapsedGameTime;
			if (timeSinceLastPost.TotalMilliseconds > postFrequency)
			{
				timeSinceLastPost = timeSinceLastPost.Subtract(new TimeSpan(0, 0, 0, 0, postFrequency));

				List<Accumulator> accumulators = world.GetComponents<Accumulator>();
				foreach (Accumulator accumulator in accumulators)
				{
					if (accumulator.Value != 0)
					{
						FloatingText freeText = new FloatingText(world,
						                                         accumulator.EntityID,
						                                         accumulator.Offset,
						                                         accumulator.Velocity,
						                                         accumulator.Value.ToString("+0;-0;+0"),
						                                         accumulator.Color,
						                                         accumulator.FadeRate);
						world.AddComponent(freeText);

						accumulator.Value = 0;
					}
				}
			}

			base.Update(gameTime);
		}


		public override void  Draw(GameTime gameTime)
		{
			spriteBatch.Begin();

			List<FloatingText> floatingTexts = world.GetComponents<FloatingText>();
			foreach (var floatingText in floatingTexts)
			{
				Position position = world.GetComponent<Position>(floatingText);
				spriteBatch.DrawString(font,
				                       floatingText.Text,
				                       world.WorldToScreen(position.Center + floatingText.Offset),
				                       floatingText.Color.Color,
				                       0,
				                       Vector2.Zero,
				                       1 / world.ScaleFactor,
				                       SpriteEffects.None,
				                       0);
			}

			spriteBatch.End();
		}
	}
}
