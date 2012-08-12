using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AsteroidOutpost.Screens;
using Microsoft.Xna.Framework;
using XNASpriteLib;

namespace AsteroidOutpost.Components
{
	public class Animator : Component
	{

		public Animator(World world, int entityID, Sprite sprite)
			: base(world, entityID)
		{
			SpriteAnimator = new SpriteAnimator(sprite);
			Tint = Color.White;
		}


		protected Animator(BinaryReader br)
			: base(br)
		{
		}


		public SpriteAnimator SpriteAnimator { get; set; }
		public Color Tint { get; set; }
	}
}
