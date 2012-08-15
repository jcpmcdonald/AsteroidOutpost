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

		public Animator(World world, int entityID, Sprite sprite, float scale = 1.0f, String set = null, String animation = null, String orientation = null)
			: base(world, entityID)
		{
			Init(scale, sprite, set, animation, orientation);
		}

		public Animator(World world, int entityID, Sprite sprite, float scale, String set, String animation, float orientation)
			: base(world, entityID)
		{
			Init(scale, sprite, set, animation, orientation.ToString());
		}


		protected Animator(BinaryReader br)
			: base(br)
		{
		}


		private void Init(float scale, Sprite sprite, String set = null, String animation = null, String orientation = null)
		{
			Scale = scale;
			Tint = Color.White;
			SpriteAnimator = new SpriteAnimator(sprite);

			if (set != null)
			{
				SpriteAnimator.CurrentSet = set;
			}
			if (animation != null)
			{
				SpriteAnimator.CurrentAnimation = animation;
			}
			if (orientation != null)
			{
				SpriteAnimator.CurrentOrientation = orientation;
			}
		}


		public float Scale { get; set; }
		public SpriteAnimator SpriteAnimator { get; set; }
		public Color Tint { get; set; }
	}
}
