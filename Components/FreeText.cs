using System;
using System.IO;
using AsteroidOutpost.Entities;
using AsteroidOutpost.Interfaces;
using AsteroidOutpost.Screens;
using C3.XNA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AsteroidOutpost.Components
{
	public class FreeText : Component
	{
		private readonly Position position;
		private int postDeserializePositionID;		// For serialization linking, don't use this

		private String text;
		protected Color color;

		public FreeText(World world, IComponentList componentList, Position position, string text, Color color)
			: base(world, componentList)
		{
			this.text = text;
			this.position = position;
			this.color = color;
		}


		public FreeText(BinaryReader br)
			: base(br)
		{
			postDeserializePositionID = br.ReadInt32();

			text = br.ReadString();
			//offset = br.ReadVector2();
			color = new Color(br.ReadByte(), br.ReadByte(), br.ReadByte(), br.ReadByte());
		}


		/// <summary>
		/// Updates this component
		/// </summary>
		/// <param name="deltaTime">The amount of time that has passed since the last frame</param>
		public override void Update(TimeSpan deltaTime)
		{
			base.Update(deltaTime);

			position.Update(deltaTime);
		}


		public override void Draw(SpriteBatch spriteBatch, float scaleModifier, Color tint)
		{
			spriteBatch.DrawString(Fonts.ControlFont,
								   text,
								   world.WorldToScreen(position.Center),
								   ColorPalette.ApplyTint(color, tint),
								   0,
								   Vector2.Zero,
								   1 / world.ScaleFactor,
								   SpriteEffects.None,
								   0);
		}
	}
}
