using System;
using System.Collections.Generic;
using System.Diagnostics;
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
	public class ProgressBar : Component
	{
		private Position position;
		private int postDeserializePositionID;		// For serialization linking, don't use this

		private int length;
		private int thickness;
		private int min;
		private int max = 100;
		private int progress;
		private Color backgroundColor;
		private Color foregroundColor;


		public ProgressBar(AsteroidOutpostScreen theGame, IComponentList componentList, Position position, Vector2 positionOffset, int length, int thickness, Color backgroundColor, Color foregroundColor)
			: base(theGame, componentList)
		{
			this.position = new PositionOffset(theGame, componentList, position, positionOffset);
			componentList.AddComponent(this.position);

			this.length = length;
			this.thickness = thickness;
			this.foregroundColor = foregroundColor;
			this.backgroundColor = backgroundColor;
		}


		public ProgressBar(AsteroidOutpostScreen theGame, IComponentList componentList, Vector2 position, int length, int thickness, Color backgroundColor, Color foregroundColor)
			: base(theGame, componentList)
		{
			this.position = new Position(theGame, componentList, position);
			componentList.AddComponent(this.position);

			this.length = length;
			this.thickness = thickness;
			this.foregroundColor = foregroundColor;
			this.backgroundColor = backgroundColor;
		}


		protected ProgressBar(BinaryReader br)
			: base(br)
		{
			postDeserializePositionID = br.ReadInt32();
		}


		public override void Serialize(BinaryWriter bw)
		{
			// Always serialize the base first because we can't pick the deserialization order
			base.Serialize(bw);

			bw.Write(position.ID);
		}

		public override void PostDeserializeLink(AsteroidOutpostScreen theGame)
		{
			base.PostDeserializeLink(theGame);

			position = theGame.GetComponent(postDeserializePositionID) as Position;

			if (position == null)
			{
				Debugger.Break();
			}
		}


		public void SetProgress(IQuantifiable e)
		{
			progress = e.Quantity;

			// Sanity check
			if(progress < min || progress > max)
			{
				Debugger.Break();
			}
		}


		public int Progress
		{
			get
			{
				return progress;
			}
			set
			{
				progress = value;
			}
		}

		public int Max
		{
			get
			{
				return max;
			}
			set
			{
				max = value;
			}
		}


		public override void Draw(SpriteBatch spriteBatch, float scaleModifier, Color tint)
		{
			base.Draw(spriteBatch, scaleModifier, tint);

			float percentFilled = (float)(Progress - min) / (max - min);
			spriteBatch.FillRectangle(theGame.WorldToScreen(position.Center) + theGame.Scale(new Vector2(-length / 2.0f, thickness / 2.0f)),
			                          theGame.Scale(new Vector2(length, thickness)),
									  backgroundColor);

			spriteBatch.FillRectangle(theGame.WorldToScreen(position.Center) + theGame.Scale(new Vector2(-length / 2.0f, thickness / 2.0f)),
									  theGame.Scale(new Vector2(length * percentFilled, thickness)),
									  foregroundColor);
		}
	}
}
