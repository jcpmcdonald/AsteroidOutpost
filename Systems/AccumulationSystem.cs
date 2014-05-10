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
		private List<FloatingText> floatingTexts = new List<FloatingText>(20);

		private static SpriteFont font;

		public AccumulationSystem(AOGame game, World world, int postFrequency)
			: base(game)
		{
			spriteBatch = new SpriteBatch(game.GraphicsDevice);
			this.world = world;
			this.postFrequency = postFrequency;
		}


		/// <summary>
		/// Called when graphics resources need to be loaded. Override this method to load any component-specific graphics resources.
		/// </summary>
		protected override void LoadContent()
		{
			font = Game.Content.Load<SpriteFont>("Fonts\\ControlFont");
			base.LoadContent();
		}


		public override void Update(GameTime gameTime)
		{
			if(world.Paused) { return; }

			// Update the floating text objects
			List<FloatingText> deadFloatingTexts = new List<FloatingText>();
			foreach (var floatingText in floatingTexts)
			{
				floatingText.FadeTimeRemaining = floatingText.FadeTimeRemaining.Subtract(gameTime.ElapsedGameTime);
				if(floatingText.FadeTimeRemaining <= TimeSpan.Zero)
				{
					deadFloatingTexts.Add(floatingText);
				}
				else
				{
					floatingText.Velocity += floatingText.Acceleration * (float)gameTime.ElapsedGameTime.TotalSeconds;
					floatingText.Position += floatingText.Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;

					float percentDone = 1f - ((float)floatingText.FadeTimeRemaining.TotalSeconds / floatingText.FadeTime);

					floatingText.Color = new Color((byte)MathHelper.SmoothStep(floatingText.StartingColor.R, floatingText.EndingColor.R, percentDone),
					                               (byte)MathHelper.SmoothStep(floatingText.StartingColor.G, floatingText.EndingColor.G, percentDone),
					                               (byte)MathHelper.SmoothStep(floatingText.StartingColor.B, floatingText.EndingColor.B, percentDone),
					                               (byte)MathHelper.SmoothStep(floatingText.StartingColor.A, floatingText.EndingColor.A, percentDone));
				}
			}

			foreach (var deadFloatingText in deadFloatingTexts)
			{
				floatingTexts.Remove(deadFloatingText);
			}


			// Create new floating text objects
			timeSinceLastPost += gameTime.ElapsedGameTime;
			if (timeSinceLastPost.TotalMilliseconds > postFrequency)
			{
				timeSinceLastPost = timeSinceLastPost.Subtract(new TimeSpan(0, 0, 0, 0, postFrequency));

				foreach (Accumulator accumulator in world.GetComponents<Accumulator>())
				{
					if (accumulator.Value != 0)
					{
						Position position = world.GetComponent<Position>(accumulator);
						Vector2 pos = position.Center + accumulator.Offset;
						FloatingText freeText = new FloatingText{
							Position = position.Center + accumulator.Offset,
							Velocity = new Vector2(GlobalRandom.Next(accumulator.VelocityMin.X, accumulator.VelocityMax.X),
							                       GlobalRandom.Next(accumulator.VelocityMin.Y, accumulator.VelocityMax.Y)),
							Acceleration = new Vector2(GlobalRandom.Next(accumulator.AccelerationMin.X, accumulator.AccelerationMax.X),
							                           GlobalRandom.Next(accumulator.AccelerationMin.Y, accumulator.AccelerationMax.Y)),
							Text = accumulator.Value.ToString("+0;-0;+0"),
							StartingColor = accumulator.StartingColor,
							EndingColor = accumulator.EndingColor,
							FadeTime = GlobalRandom.Next(accumulator.FadeTimeMin, accumulator.FadeTimeMax)
						};
						freeText.FadeTimeRemaining = new TimeSpan(0, 0, 0, 0, (int)(freeText.FadeTime * 1000f));

						floatingTexts.Add(freeText);

						accumulator.Value = 0;
					}
				}
			}

			base.Update(gameTime);
		}


		public override void  Draw(GameTime gameTime)
		{
			spriteBatch.Begin();

			foreach (var floatingText in floatingTexts)
			{
				spriteBatch.DrawString(font,
				                       floatingText.Text,
				                       world.WorldToScreen(floatingText.Position),
				                       floatingText.Color,
				                       0,
				                       Vector2.Zero,
				                       1 / world.ScaleFactor,
				                       SpriteEffects.None,
				                       0);
			}

			spriteBatch.End();
		}
	}


	class FloatingText
	{
		public Vector2 Position { get; set; }
		public Vector2 Velocity { get; set; }
		public Vector2 Acceleration { get; set; }
		public String Text { get; set; }

		public Color StartingColor { get; set; }
		public Color EndingColor { get; set; }
		public Color Color { get; set; }
		public float FadeTime { get; set; }
		public TimeSpan FadeTimeRemaining { get; set; }
	}
}
