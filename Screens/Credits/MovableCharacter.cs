using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using C3.XNA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AsteroidOutpost.Screens.Credits
{
	class MovableCharacter
	{
		private String character;
		private Color color;
		private Vector2 location;
		private Vector2 destination;
		private float acceleration;
		private Vector2 velocity;
		private float scale;

		/// <summary>
		/// How close to your destination we should get before stopping
		/// </summary>
		private const float floatMarginOfError = 1.0f;

		public MovableCharacter(char theCharacter, float theAcceleration, float theScale, Color theColor)
		{
			character = theCharacter.ToString();
			color = theColor;
			acceleration = theAcceleration;
			velocity = Vector2.Zero;
			scale = theScale;

			location = Vector2.Zero;
			destination = Vector2.Zero;
		}


		/// <summary>
		/// Gets or sets the location
		/// </summary>
		public Vector2 Location
		{
			get
			{
				return location; 
			}
			set
			{
				location = value;
			}
		}


		/// <summary>
		/// Gets or sets the destination
		/// </summary>
		public Vector2 Destination
		{
			get
			{
				return destination;
			}
			set
			{
				destination = value;
			}
		}


		/// <summary>
		/// Gets the character this will display when drawn
		/// </summary>
		public char Character
		{
			get
			{
				return character[0];
			}
		}

		public Vector2 Velocity
		{
			get
			{
				return velocity;
			}
			set
			{
				velocity = value;
			}
		}


		/// <summary>
		/// Updates this movable character
		/// </summary>
		/// <param name="deltaTime">The amount of time that has passed since the last update</param>
		public void Update(TimeSpan deltaTime)
		{
			if (location != destination)
			{

				if (distanceToDestination() < floatMarginOfError)
				{
					// We have reached our destination, stop!
					location = destination;
					velocity = Vector2.Zero;
				}
				else if (distanceToDestination() > minDistanceToStop())
				{
					accelerate(deltaTime);
				}
				else
				{
					decelerate(deltaTime);
				}
			}

			if (velocity != Vector2.Zero)
			{
				// Move!
				location.X = location.X + (float)(velocity.X * deltaTime.TotalSeconds);
				location.Y = location.Y + (float)(velocity.Y * deltaTime.TotalSeconds);
			}
		}


		/// <summary>
		/// Draws this movable character to the screen
		/// </summary>
		/// <param name="spriteBatch">The sprite batch to draw to</param>
		/// <param name="tint">The colour to use as a tint</param>
		public void Draw(SpriteBatch spriteBatch, Color tint)
		{
			spriteBatch.DrawString(Fonts.CreditsFont, character, location, ColorPalette.ApplyTint(color, tint), 0, Vector2.Zero, scale, SpriteEffects.None, 0);
		}



		#region A copy of a number of useful functions from Entity

		/// <summary>
		/// Accelerate this object
		/// </summary>
		/// <param name="deltaTime">The amount of time that has passed since the last update cycle</param>
		private void accelerate(TimeSpan deltaTime)
		{
			float angle = getAngleToDestination();
			float increaseX = (float)(Math.Sin(angle) * acceleration * deltaTime.TotalSeconds);
			float increaseY = -(float)(Math.Cos(angle) * acceleration * deltaTime.TotalSeconds);

			velocity.X += increaseX;
			velocity.Y += increaseY;
		}


		/// <summary>
		/// Decelerate this object
		/// </summary>
		/// <param name="deltaTime">The amount of time that has passed since the last update cycle</param>
		private void decelerate(TimeSpan deltaTime)
		{
			float angle = getAngleToDestination();
			float decreaseX = (float)(Math.Sin(angle) * acceleration * deltaTime.TotalSeconds);
			float decreaseY = -(float)(Math.Cos(angle) * acceleration * deltaTime.TotalSeconds);

			if (Math.Abs(decreaseX) > Math.Abs(velocity.X))
			{
				velocity.X = 0;
			}
			else
			{
				velocity.X -= decreaseX;
			}

			if (Math.Abs(decreaseY) > Math.Abs(velocity.Y))
			{
				velocity.Y = 0;
			}
			else
			{
				velocity.Y -= decreaseY;
			}
		}


		/// <summary>
		/// Get the relative angle from here to the destination (relative to the world)
		/// </summary>
		/// <returns>An angle, relative to the game axis, in radians, from here to the destination</returns>
		protected float getAngleToDestination()
		{
			float dx = destination.X - location.X;
			float dy = location.Y - destination.Y;
			double distance = Math.Sqrt((dx * dx) + (dy * dy));

			double angle = (Math.Acos(dy / distance));

			if (dx < 0)
			{
				angle = (2 * Math.PI) - angle;
			}
			return (float)angle;
		}


		/// <summary>
		/// Gets the distance between here and the destination
		/// </summary>
		/// <returns>The distance between here and the destination</returns>
		private double distanceToDestination()
		{
			double xDiff = location.X - destination.X;
			double yDiff = location.Y - destination.Y;
			return Math.Sqrt((xDiff * xDiff) + (yDiff * yDiff));
		}


		/// <summary>
		/// Calculates the minimum distance to stop given the acceleration and current velocity
		/// </summary>
		/// <returns>The minimum distance to stop given the acceleration and current velocity</returns>
		private float minDistanceToStop()
		{
			return (float)(0.5 * acceleration * Math.Pow(velocityLinear() / acceleration, 2.0));
		}


		/// <summary>
		/// Calculates the velocity in a straight line
		/// </summary>
		/// <returns></returns>
		protected float velocityLinear()
		{
			return (float)Math.Sqrt((velocity.X * velocity.X) + (velocity.Y * velocity.Y));
		}

		#endregion

	}
}
