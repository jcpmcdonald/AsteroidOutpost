using System;
using System.IO;
using AsteroidOutpost.Entities;
using AsteroidOutpost.Interfaces;
using AsteroidOutpost.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using XNASpriteLib;

namespace AsteroidOutpost.Components
{
	public class Beacon : Entity
	{
		private static Sprite sprite;
		private static float angleStep;

		private float rotationSpeed = 0.8f;
		private float angle = 0;
		private float angleDiff = 0;


		public Beacon(AsteroidOutpostScreen theGame, IComponentList componentList, Force theOwningForce, Vector2 theCentre, int theRadius)
			: base(theGame, componentList, theOwningForce, theCentre, theRadius, 9999)
		{
			Init();
		}


		public Beacon(BinaryReader br) : base(br)
		{
			Init();
		}


		private void Init()
		{
			Solid = false;

			animator = new SpriteAnimator(sprite);
			animator.CurrentAnimation = "Rotate";
			animator.CurrentOrientation = (angleStep * GlobalRandom.Next(0, sprite.OrientationLookup.Count - 1)).ToString();
		}


		/// <summary>
		/// This is where all entities should do any resource loading that will be required. This will be called once per game.
		/// </summary>
		/// <param name="spriteBatch">The sprite batch</param>
		/// <param name="content">The content manager</param>
		public static void LoadContent(SpriteBatch spriteBatch, ContentManager content)
		{
			sprite = new Sprite(File.OpenRead(@"..\Sprites\Beacon.sprx"), spriteBatch.GraphicsDevice);
			angleStep = 360.0f / sprite.OrientationLookup.Count;
		}


		/// <summary>
		/// Gets the name of this entity
		/// </summary>
		public override string Name
		{
			get
			{
				return "Beacon";
			}
		}


		public override void Update(TimeSpan deltaTime)
		{
			base.Update(deltaTime);

			angle += rotationSpeed * (float)deltaTime.TotalSeconds;
			SetOrientation();
		}


		public override void Draw(SpriteBatch spriteBatch, float scaleModifier, Color tint)
		{
			//base.Draw(spriteBatch, scaleModifier, tint);

			animator.Draw(spriteBatch, theGame.WorldToScreen(Position.Center), angleDiff, scaleModifier * 0.7f / theGame.ScaleFactor, tint);
		}


		/// <summary>
		/// A helper method to set the current Orientation for the animator
		/// </summary>
		private void SetOrientation()
		{
			float orientation = ((int)((MathHelper.ToDegrees(angle) + (angleStep / 2.0)) / angleStep)) * angleStep;

			while (orientation < 0)
			{
				orientation += 360f;
			}
			while (orientation >= 360)
			{
				orientation -= 360f;
			}

			animator.CurrentOrientation = orientation.ToString();
			angleDiff = angle - MathHelper.ToRadians(orientation);
		}
	}
}
