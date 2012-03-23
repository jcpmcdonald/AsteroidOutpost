using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using AsteroidOutpost.Entities.Weapons;
using AsteroidOutpost.Interfaces;
using AsteroidOutpost.Screens;
using C3.XNA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using XNASpriteLib;

namespace AsteroidOutpost.Entities.Units
{
	class Ship1 : Ship
	{
		private static Sprite sprite;
		private static float angleStep;

		//private SpriteAnimator animator;
		private float angleDiff = 0;
		private Weapon weapon;


		private Vector2 accelerationVector;


		public Ship1(AsteroidOutpostScreen theGame, IComponentList componentList, Force theowningForce, Vector2 theCenter)
			: base(theGame, componentList, theowningForce, theCenter, 45)
		{
			accelerationMagnitude = 10;
			animator = new SpriteAnimator(sprite);

			weapon = new Laser(theGame, this);
		}


		public Ship1(BinaryReader br)
			: base(br)
		{
			accelerationMagnitude = 10;
			animator = new SpriteAnimator(sprite);
		}

		public override void PostDeserializeLink(AsteroidOutpostScreen theGame)
		{
			base.PostDeserializeLink(theGame);

			weapon = new Laser(theGame, this);
		}


		/// <summary>
		/// This is where all entities should do any resource loading that will be required. This will be called once per game.
		/// </summary>
		/// <param name="spriteBatch">The sprite batch</param>
		/// <param name="content">The content manager</param>
		public static void LoadContent(SpriteBatch spriteBatch, ContentManager content)
		{
			sprite = new Sprite(File.OpenRead(@"..\Sprites\Spaceship128.sprx"), spriteBatch.GraphicsDevice);
			angleStep = 360.0f / sprite.OrientationLookup.Count;
		}


		public override string Name
		{
			get { return "Test Ship 1"; }
		}


		/// <summary>
		/// Updates this entity
		/// </summary>
		/// <param name="deltaTime">The elapsed time since the last update</param>
		public override void Update(TimeSpan deltaTime)
		{
			// Find my flock-mates
			List<Entity> flockMates = new List<Entity>(10);
			int searchRadius = 800;
			foreach (Entity nearEntity in theGame.EntitiesInArea((int)Position.Center.X - searchRadius, (int)Position.Center.Y - searchRadius, searchRadius * 2, searchRadius * 2))
			{
				if (nearEntity is Ship && !nearEntity.IsDead() && nearEntity != this)
				{
					flockMates.Add(nearEntity);
				}
			}



			if (Target != null)
			{

				if (Position.Distance(Target.Position) - weapon.OptimumRange > minDistanceToStop())
				{
					// Move toward the target and flock with my flock-mates
					accelerationVector = Target.Position.Center - Position.Center - Position.Velocity;
					accelerationVector.Normalize();

					const float cohesionFactor = 0.5f;
					const float separationFactor = 1f;
					const float alignmentFactor = 0.5f;

					Vector2 cohesion = Cohere(flockMates) * cohesionFactor;
					Vector2 separation = Separate(flockMates) * separationFactor;
					Vector2 alignment = Vector2.Zero;// align(flockMates) * alignmentFactor;

					accelerationVector = (accelerationVector * 5) + cohesion + separation + alignment;
					accelerationVector.Normalize();

					SetOrientation(accelerationVector);

					accelerateAlong(accelerationVector, deltaTime);
				}
				else
				{
					decelerate(deltaTime);
				}


				animator.Update(deltaTime);


				if (Position.Distance(Target.Position) <= weapon.MaxRange)
				{
					weapon.Shoot(Target);
				}
			}

			weapon.Update(deltaTime);

			base.Update(deltaTime);
		}



		/// <summary>
		/// Stick together with your friends
		/// </summary>
		/// <param name="flockMates">A list of flock-mates to keep together with</param>
		/// <returns>A vector that determines the direction and magnitude to move in</returns>
		private Vector2 Cohere(List<Entity> flockMates)
		{
			const int neighbourDistance = 200;
			Vector2 sum = Vector2.Zero;
			int count = 0;

			foreach (Entity mate in flockMates)
			{
				if(mate != this && Position.Distance(mate.Position) > 0 && Position.Distance(mate.Position) < neighbourDistance)
				{
					sum += mate.Position.Center;
					count++;
				}
			}

			if(count > 0)
			{
				sum = Vector2.Normalize(sum / count);
			}

			return sum;
		}


		/// <summary>
		/// Separate from your friends
		/// </summary>
		/// <param name="flockMates">A list of flock-mates to keep your distance from</param>
		/// <returns>A vector that determines the direction and magnitude to move in</returns>
		private Vector2 Separate(List<Entity> flockMates)
		{
			const int separationDistance = 50;
			Vector2 mean = Vector2.Zero;
			int count = 0;


			foreach(Entity mate in flockMates)
			{
				float distance = Position.Distance(mate.Position);
				if(distance > 0 && distance < separationDistance)
				{
					mean += Vector2.Normalize(Position.Center - mate.Position.Center) / distance * 10;
					count++;
				}
			}

			if(count > 1)
			{
				mean /= count;
			}
			return mean;
		}

		private Vector2 align(List<Entity> flockMates)
		{
			return Vector2.Zero;
		}


		/// <summary>
		/// A helper method to set the current Orientation for the animator
		/// </summary>
		/// <param name="direction">The direction the graphic should look like it's facing</param>
		private void SetOrientation(Vector2 direction)
		{
			float angle = (float)Math.Atan2(direction.X, -direction.Y);
			float orientation = ((int)((MathHelper.ToDegrees(angle) + (angleStep / 2)) / angleStep)) * angleStep;

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


		/// <summary>
		/// Draw this entity to the screen
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="scaleModifier"></param>
		/// <param name="tint"></param>
		public override void Draw(SpriteBatch spriteBatch, float scaleModifier, Color tint)
		{
			weapon.Draw(spriteBatch, tint);

			animator.Draw(spriteBatch, theGame.WorldToScreen(Position.Center), angleDiff, scaleModifier * 0.6f / theGame.ScaleFactor, tint);

			// Draw the velocity
			//spriteBatch.DrawLine(theGame.WorldToScreen(center), theGame.WorldToScreen(center + velocity), Color.Green);

			// Draw the acceleration
			//spriteBatch.DrawLine(theGame.WorldToScreen(center), theGame.WorldToScreen(center + (accelerationVector * 10.0f)), Color.Red);

			//base.Draw(spriteBatch, theGame, scaleModifier * 0.6f, tint);
		}
	}
}