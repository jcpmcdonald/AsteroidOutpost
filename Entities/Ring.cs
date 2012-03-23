using System.Diagnostics;
using System.IO;
using AsteroidOutpost.Entities.Eventing;
using AsteroidOutpost.Interfaces;
using AsteroidOutpost.Screens;
using C3.XNA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AsteroidOutpost.Entities
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

		public Ring(AsteroidOutpostScreen theGame, IComponentList componentList, Position position, int radius, Color color)
			: base(theGame, componentList)
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
		/// <param name="theGame"></param>
		public override void PostDeserializeLink(AsteroidOutpostScreen theGame)
		{
			base.PostDeserializeLink(theGame);

			position = theGame.GetComponent(postDeserializePositionID) as Position;

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
							 theGame.WorldToScreen(position.Center - (new Vector2(textureSize + 10, textureSize + 10) * sizeRatio)),  // 10 padding in the textures
							 null,
							 ColorPalette.ApplyTint(color, tint),
							 0,
							 Vector2.Zero,
							 (sizeRatio * scaleModifier) / theGame.ScaleFactor,
							 SpriteEffects.None,
							 0);

			//base.Draw(spriteBatch, scaleModifier, tint);
		}

	}
}
