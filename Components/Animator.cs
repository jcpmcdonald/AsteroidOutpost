using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
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
		public Color Tint { get; set; }

		[XmlIgnore]
		public SpriteAnimator SpriteAnimator { get; set; }

		/// <summary>
		/// The angle difference between the exact angle this was set to, and the orientation of the frame being displayed
		/// </summary>
		public float FrameAngle { get; set; }
		public float Angle { get; private set; }

		public void SetOrientation(float angle, bool rotateFrame = false)
		{
			while (angle < 0)
			{
				angle += 360f;
			}
			while (angle >= 360)
			{
				angle -= 360f;
			}

			// Store this just in case someone wants to use it
			Angle = angle;

			float angleStep = 360.0f / SpriteAnimator.Sprite.OrientationLookup.Count;
			float roundedAngle = ((int)((angle + (angleStep / 2)) / angleStep)) * angleStep;

			while (roundedAngle < 0)
			{
				roundedAngle += 360f;
			}
			while (roundedAngle >= 360)
			{
				roundedAngle -= 360f;
			}

			SpriteAnimator.CurrentOrientation = roundedAngle.ToString();
			if(rotateFrame)
			{
				FrameAngle = angle - roundedAngle;
			}
			else
			{
				FrameAngle = 0;
			}
		}
	}
}
