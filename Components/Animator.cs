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

		//public Animator(World world, int entityID, Sprite sprite, float scale = 1.0f, String set = null, String animation = null, String orientation = null)
		//    : base(world, entityID)
		//{
		//    Init(scale, sprite, set, animation, orientation);
		//}

		public Animator(World world, int entityID, Sprite sprite, float scale, String set, String animation, float orientation)
			: base(world, entityID)
		{
			Init(scale, sprite, set, animation, orientation);
		}


		private void Init(float scale, Sprite sprite, String set, String animation, float orientation)
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
			SetOrientation(orientation);
		}


		public float Scale { get; set; }
		public SpriteAnimator SpriteAnimator { get; set; }
		public Color Tint { get; set; }
		public float OrientationDiff { get; set; }

		public void SetOrientation(float exactAngle, bool exact = false)
		{
			while (exactAngle < 0)
			{
				exactAngle += 360f;
			}
			while (exactAngle >= 360)
			{
				exactAngle -= 360f;
			}

			float angleStep = 360.0f / SpriteAnimator.Sprite.OrientationLookup.Count;
			float roundedAngle = ((int)((exactAngle + (angleStep / 2)) / angleStep)) * angleStep;

			while (roundedAngle < 0)
			{
				roundedAngle += 360f;
			}
			while (roundedAngle >= 360)
			{
				roundedAngle -= 360f;
			}

			SpriteAnimator.CurrentOrientation = roundedAngle.ToString();
			if(exact)
			{
				OrientationDiff = exactAngle - roundedAngle;
			}
			else
			{
				OrientationDiff = 0;
			}
		}
	}
}
