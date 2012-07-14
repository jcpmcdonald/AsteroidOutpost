using System;
using System.IO;
using AsteroidOutpost.Entities;
using AsteroidOutpost.Interfaces;
using AsteroidOutpost.Screens;
using C3.XNA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace AsteroidOutpost.Components
{
	public class FreeText : Component
	{
		private readonly Vector2 offset;
		private readonly Vector2 velocity;
		private String text;
		protected Color color;

		private TimeSpan cumulativeTime = new TimeSpan(0);
		private readonly float fadeRate;

		private static SpriteFont font;

		public FreeText(World world, Vector2 offset, string text, Color color, float fadeRate = 150)
			: base(world)
		{
			this.text = text;
			this.offset = offset;
			this.color = color;
			this.fadeRate = fadeRate;
		}


		public FreeText(BinaryReader br)
			: base(br)
		{
			text = br.ReadString();
			//offset = br.ReadVector2();
			color = new Color(br.ReadByte(), br.ReadByte(), br.ReadByte(), br.ReadByte());
		}


		/// <summary>
		/// This is where all entities should do any resource loading that will be required. This will be called once per game.
		/// </summary>
		/// <param name="graphicsDevice">The graphics device</param>
		/// <param name="content">The content manager</param>
		public static void LoadContent(GraphicsDevice graphicsDevice, ContentManager content)
		{
			font = content.Load<SpriteFont>("Fonts\\ControlFont");
		}


		/// <summary>
		/// Updates this component
		/// </summary>
		/// <param name="deltaTime">The amount of time that has passed since the last frame</param>
		public override void Update(TimeSpan deltaTime)
		{
			base.Update(deltaTime);

			cumulativeTime += deltaTime;

			int fadeAmount = (int)((fadeRate * cumulativeTime.TotalSeconds) + 0.5);
			color.R = (byte)Math.Max(0, 255 - fadeAmount);
			color.G = (byte)Math.Max(0, 255 - fadeAmount);
			color.B = (byte)Math.Max(0, 255 - fadeAmount);
			color.A = (byte)Math.Max(0, 255 - fadeAmount);

			if (color.A == 0 || (color.R == 0 && color.G == 0 && color.B == 0))
			{
				SetDead(true, true);
			}
		}


		public override void Draw(SpriteBatch spriteBatch, float scaleModifier, Color tint)
		{
			spriteBatch.DrawString(font,
			                       text,
			                       world.WorldToScreen(offset),
			                       color.Blend(tint),
			                       0,
			                       Vector2.Zero,
			                       1 / world.ScaleFactor,
			                       SpriteEffects.None,
			                       0);
		}
	}
}
