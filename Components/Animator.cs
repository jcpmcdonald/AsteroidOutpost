using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using AsteroidOutpost.Screens;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using XNASpriteLib;

namespace AsteroidOutpost.Components
{
	public class Animator : Component
	{

		public Animator(int entityID) : base(entityID) {}
		public Animator(int entityID, Sprite sprite, float scale, String set, String animation, float orientation, bool rotateFrame = false)
			: base(entityID)
		{
			Init(scale, sprite, set, animation, orientation, rotateFrame);
		}


		private void Init(float scale, Sprite sprite, String set, String animation, float orientation, bool rotateFrame)
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
			SetOrientation(orientation, rotateFrame);
		}


		public float Scale { get; set; }
		public Color Tint { get; set; }

		[XmlIgnore]
		[JsonIgnore]
		public SpriteAnimator SpriteAnimator { get; set; }


		/// <summary>
		/// Gets or Sets the current Set. Note that changing this will cause neither the frame index nor the animation to change.
		/// </summary>
		public String CurrentSet
		{
			get
			{
				return SpriteAnimator.CurrentSet;
			}
			set
			{
				SpriteAnimator.CurrentSet = value;
			}
		}


		/// <summary>
		/// Gets or Sets the current Animation. Note that changing this will cause the frame index to be reset.
		/// </summary>
		public String CurrentAnimation
		{
			get
			{
				return SpriteAnimator.CurrentAnimation;
			}
			set
			{
				SpriteAnimator.CurrentAnimation = value;
			}
		}


		/// <summary>
		/// Gets or Sets the current Orientation. Note that changing this will cause neither the frame index nor the animation to change.
		/// </summary>
		public String CurrentOrientation
		{
			get
			{
				return SpriteAnimator.CurrentOrientation;
			}
			set
			{
				SpriteAnimator.CurrentOrientation = value;
			}
		}


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
