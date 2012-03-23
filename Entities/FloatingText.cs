using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AsteroidOutpost.Interfaces;
using AsteroidOutpost.Screens;
using C3.XNA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AsteroidOutpost.Entities
{
	public class FloatingText : FreeText
	{
		private TimeSpan cumulativeTime = new TimeSpan(0);
		private readonly float fade;
		private Color fadeTint = Color.White;

		public FloatingText(AsteroidOutpostScreen theGame, IComponentList componentList, Position position, string text, Color color, float fade)
			: base(theGame, componentList, position, text, color)
		{
			this.fade = fade;
		}


		public FloatingText(BinaryReader br)
			: base(br)
		{
		}


		/// <summary>
		/// Updates this component
		/// </summary>
		/// <param name="deltaTime">The amount of time that has passed since the last frame</param>
		public override void Update(TimeSpan deltaTime)
		{
			base.Update(deltaTime);

			cumulativeTime += deltaTime;

			int fadeAmount = (int)((fade * cumulativeTime.TotalSeconds) + 0.5);
			fadeTint.R = (byte)Math.Max(0, 255 - fadeAmount);
			fadeTint.G = (byte)Math.Max(0, 255 - fadeAmount);
			fadeTint.B = (byte)Math.Max(0, 255 - fadeAmount);
			fadeTint.A = (byte)Math.Max(0, 255 - fadeAmount);

			if (fadeTint.A == 0 || (fadeTint.R == 0 && fadeTint.G == 0 && fadeTint.B == 0))
			{
				SetDead(true, true);
			}
		}

		public override void Draw(SpriteBatch spriteBatch, float scaleModifier, Color tint)
		{
			base.Draw(spriteBatch, scaleModifier, ColorPalette.ApplyTint(fadeTint, tint));
		}
	}
}
