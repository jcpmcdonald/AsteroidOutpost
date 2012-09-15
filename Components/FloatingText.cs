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
	public class FloatingText : Component
	{
		private ColorF color;
		public Vector2 Offset { get; set; }
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


		public FloatingText(World world,
		                    int entityID,
		                    Vector2 offset,
		                    Vector2 velocity,
		                    string text,
		                    Color color,
		                    float fadeRate = 150)
			: base(world, entityID)
		{
			Text = text;
			Offset = offset;
			Velocity = velocity;
			this.color.Color = color;
			FadeRate = fadeRate;
			//CumulativeTime = new TimeSpan(0);
		}


		public FloatingText(BinaryReader br)
			: base(br)
		{
			//Text = br.ReadString();
			//offset = br.ReadVector2();
			//Color = new Color(br.ReadByte(), br.ReadByte(), br.ReadByte(), br.ReadByte());
		}
	}
}
