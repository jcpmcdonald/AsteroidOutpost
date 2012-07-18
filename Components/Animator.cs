using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AsteroidOutpost.Screens;
using XNASpriteLib;

namespace AsteroidOutpost.Components
{
	public class Animator : Component
	{


		public Animator(World world, Sprite sprite)
			: base(world)
		{
			SpriteAnimator = new SpriteAnimator(sprite);
		}


		protected Animator(BinaryReader br)
			: base(br)
		{
		}


		public SpriteAnimator SpriteAnimator { get; set; }
	}
}
