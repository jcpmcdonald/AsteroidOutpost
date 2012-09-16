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

		/// <summary>
		/// The angle difference between the exact angle this was set to, and the orientation of the frame being displayed
		/// </summary>
		public float AngleDiff { get; set; }
		public float ExactAngle { get; private set; }

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

			// Store this just in case someone wants to use it
			ExactAngle = exactAngle;

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
				AngleDiff = exactAngle - roundedAngle;
			}
			else
			{
				AngleDiff = 0;
			}
		}
	}
}
