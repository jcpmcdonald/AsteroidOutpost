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
	public class Ring : Component
	{
		private Position position;
		private int postDeserializePositionID;		// For serialization linking, don't use this

		private readonly int radius;

		private Color color;
		private Texture2D texture;
		private int textureSize;
		private float sizeRatio;

		public Ring(World world, Position position, int radius, Color color)
			: base(world)
		{
			this.position = position;
			this.radius = radius;
			this.color = color;
			Init();
		}


		public Ring(BinaryReader br)
			: base(br)
		{
			postDeserializePositionID = br.ReadInt32();

			color = new Color(br.ReadByte(), br.ReadByte(), br.ReadByte(), br.ReadByte());
			Init();
		}


		public override void Serialize(BinaryWriter bw)
		{
			// Always serialize the base first because we can't pick the deserialization order
			base.Serialize(bw);

			bw.Write(position.ID);

			bw.Write(color.R);
			bw.Write(color.G);
			bw.Write(color.B);
			bw.Write(color.A);
		}


		/// <summary>
		/// After deserializing, this should be called to link this object to other objects
		/// </summary>
		/// <param name="world"></param>
		public override void PostDeserializeLink(World world)
		{
			base.PostDeserializeLink(world);

			position = world.GetComponent(postDeserializePositionID) as Position;

			if (position == null)
			{
				Debugger.Break();
			}
		}


		private void Init()
		{
			if (radius <= 130)
			{
				texture = TextureDictionary.Get("ellipse100");
				textureSize = 100;
				sizeRatio = (radius / (float)textureSize);
			}
			else// if(Radius <= 250)
			{
				texture = TextureDictionary.Get("ellipse220");
				textureSize = 220;
				sizeRatio = (radius / (float)textureSize);
			}
		}


		public override void Draw(SpriteBatch spriteBatch, float scaleModifier, Color tint)
		{
			spriteBatch.Draw(texture,
							 world.WorldToScreen(position.Center - (new Vector2(textureSize + 10, textureSize + 10) * sizeRatio)),  // 10 padding in the textures
							 null,
							 color.Blend(tint),
							 0,
							 Vector2.Zero,
							 (sizeRatio * scaleModifier) / world.ScaleFactor,
							 SpriteEffects.None,
							 0);

			//base.Draw(spriteBatch, scaleModifier, tint);
		}

	}
}
