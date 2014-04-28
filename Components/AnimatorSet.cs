using System;
using System.Collections.Generic;
using System.Globalization;
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
	public class AnimatorSet : Component
	{

		public AnimatorSet(int entityID) : base(entityID) {}

		public bool RotateFrame { get; set; }
		public float Scale { get; set; }
		public Color Tint { get; set; }

		[XmlIgnore]
		[JsonIgnore]
		public List<KeyValuePair<SpriteAnimator, int>> SpriteAnimators { get; set; }


		/// <summary>
		/// Gets or Sets the current Set. Note that changing this will cause neither the frame index nor the animation to change.
		/// </summary>
		public String CurrentSet
		{
			get
			{
				return SpriteAnimators[0].Key.CurrentSet;
			}
			set
			{
				SpriteAnimators.ForEach(x => x.Key.CurrentSet = value);
			}
		}


		/// <summary>
		/// Gets or Sets the current Animation. Note that changing this will cause the frame index to be reset.
		/// </summary>
		public String CurrentAnimation
		{
			get
			{
				return SpriteAnimators[0].Key.CurrentAnimation;
			}
			set
			{
				SpriteAnimators.ForEach(x => x.Key.CurrentAnimation = value);
			}
		}


		/// <summary>
		/// Gets or Sets the current Orientation. Note that changing this will cause neither the frame index nor the animation to change.
		/// </summary>
		public float CurrentOrientation
		{
			get
			{
				return float.Parse(SpriteAnimators[0].Key.CurrentOrientation, CultureInfo.InvariantCulture);
			}
			set
			{
				SetOrientation(value, RotateFrame);
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

			float angleStep = 360.0f / SpriteAnimators[0].Key.Sprite.OrientationLookup.Count;
			float roundedAngle = ((int)((angle + (angleStep / 2)) / angleStep)) * angleStep;

			while (roundedAngle < 0)
			{
				roundedAngle += 360f;
			}
			while (roundedAngle >= 360)
			{
				roundedAngle -= 360f;
			}

			SpriteAnimators.ForEach(x => x.Key.CurrentOrientation = roundedAngle.ToString(CultureInfo.InvariantCulture));
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
