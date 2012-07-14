using System.Diagnostics;
using System.IO;
using AsteroidOutpost.Entities;
using AsteroidOutpost.Interfaces;
using AsteroidOutpost.Screens;
using C3.XNA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AsteroidOutpost.Components
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


		public ProgressBar(World world, Position position, Vector2 positionOffset, int length, int thickness, Color backgroundColor, Color foregroundColor)
			: base(world)
		{
			this.position = new PositionOffset(world, position, positionOffset);
			world.HUD.AddComponent(this.position);

			this.length = length;
			this.thickness = thickness;
			this.foregroundColor = foregroundColor;
			this.backgroundColor = backgroundColor;
		}


		public ProgressBar(World world, IComponentList componentList, Vector2 position, int length, int thickness, Color backgroundColor, Color foregroundColor)
			: base(world)
		{
			this.position = new Position(world, position);
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

		public override void PostDeserializeLink(World world)
		{
			base.PostDeserializeLink(world);

			position = world.GetComponent(postDeserializePositionID) as Position;

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

		public int Min
		{
			get
			{
				return min;
			}
			set
			{
				min = value;
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
			spriteBatch.FillRectangle(world.WorldToScreen(position.Center) + world.Scale(new Vector2(-length / 2.0f, thickness / 2.0f)),
			                          world.Scale(new Vector2(length, thickness)),
									  backgroundColor);

			spriteBatch.FillRectangle(world.WorldToScreen(position.Center) + world.Scale(new Vector2(-length / 2.0f, thickness / 2.0f)),
									  world.Scale(new Vector2(length * percentFilled, thickness)),
									  foregroundColor);
		}
	}
}
