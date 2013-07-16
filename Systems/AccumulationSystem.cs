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
			if(world.Paused) { return; }

			// Update the floating text objects
			List<FloatingText> deadFloatingTexts = new List<FloatingText>();
			foreach (var floatingText in floatingTexts)
			{
				// TODO: 2012-10-07 Fix up this code, it doesn't fade very well. Zoom in and see for yourself

				//floatingText.CumulativeTime += gameTime.ElapsedGameTime;
				float fadeAmount = floatingText.FadeRate * (float)gameTime.ElapsedGameTime.TotalSeconds;
				floatingText.Position += floatingText.Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;

				floatingText.Color = new ColorF(floatingText.Color.R - fadeAmount,
				                                floatingText.Color.G - fadeAmount,
				                                floatingText.Color.B - fadeAmount,
				                                floatingText.Color.A - fadeAmount);

				//Console.WriteLine(String.Format(CultureInfo.InvariantCulture, "{0} {1} {2} {3}", floatingText.Color.R, floatingText.Color.G, floatingText.Color.B, floatingText.Color.A));

				if (floatingText.Color.A <= 0 ||
				    (floatingText.Color.R <= 0 &&
				     floatingText.Color.G <= 0 &&
				     floatingText.Color.B <= 0))
				{
					//floatingText.SetDead(true, true);
					//world.DeleteComponent(floatingText);
					deadFloatingTexts.Add(floatingText);
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
						FloatingText freeText = new FloatingText(position.Center + accumulator.Offset,
						                                         accumulator.Velocity,
						                                         accumulator.Value.ToString("+0;-0;+0"),
						                                         accumulator.Color,
						                                         accumulator.FadeRate);
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


	class FloatingText
	{
		private ColorF color;
		public Vector2 Position { get; set; }
		public Vector2 Velocity { get; set; }
		public string Text { get; set; }
		public ColorF Color
		{
			get
			{
				return color;
			}
			set
			{
				color = value;
			}
		}

		//public TimeSpan CumulativeTime { get; set; }
		public float FadeRate { get; set; }
		//public float FadeAmount { get; set; }


		public FloatingText(Vector2 position,
		                    Vector2 velocity,
		                    string text,
		                    Color color,
		                    float fadeRate = 150)
		{
			Text = text;
			Position = position;
			Velocity = velocity;
			this.color.Color = color;
			FadeRate = fadeRate;
			//CumulativeTime = new TimeSpan(0);
		}
	}
}
