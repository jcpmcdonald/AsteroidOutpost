using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using C3.XNA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AsteroidOutpost.Screens.Credits
{
	class ExplodingWord
	{
		private List<MovableCharacter> characters = new List<MovableCharacter>();
		private String text;
		private float scale;
		private Vector2 viewSize;
		private bool visible;

		private const int offScreenMinDistance = 100;
		private const int offScreenMaxDistance = 200;

		static readonly Random rand = new Random();


		public ExplodingWord(String theText, float theAcceleration, float theScale, Vector2 theViewSize, Color theColor)
		{
			text = theText;
			scale = theScale;
			viewSize = theViewSize;

			foreach (char character in text)
			{
				characters.Add(new MovableCharacter(character, theAcceleration, scale, theColor));
			}
		}


		/// <summary>
		/// Make this word implode (form in the center of the screen)
		/// </summary>
		/// <param name="centreLocation">Where the Center of the word should implode to</param>
		public void Implode(Vector2 centreLocation)
		{
			Vector2 stringMeasure = Fonts.CreditsFont.MeasureString(text) * scale;
			float yOffset = centreLocation.Y - (stringMeasure.Y / 2);
			float currentXOffset = centreLocation.X - (stringMeasure.X / 2);

			foreach (MovableCharacter movingCharacter in characters)
			{
				Vector2 characterMeasure = Fonts.CreditsFont.MeasureString("" + movingCharacter.Character) * scale;

				if (movingCharacter.Character != ' ')
				{
					Vector2 startingLocation;

					switch (rand.Next(0, 4))
					{
					case 0:
						// top
						startingLocation.X = (int)(rand.NextDouble() * viewSize.X);
						startingLocation.Y = -characterMeasure.Y - rand.Next(offScreenMinDistance, offScreenMaxDistance);
						break;
					case 1:
						// right
						startingLocation.X = viewSize.X + rand.Next(offScreenMinDistance, offScreenMaxDistance);
						startingLocation.Y = (int)(rand.NextDouble() * viewSize.Y);
						break;
					case 2:
						// bottom
						startingLocation.X = (int)(rand.NextDouble() * viewSize.X);
						startingLocation.Y = viewSize.Y + rand.Next(offScreenMinDistance, offScreenMaxDistance);
						break;
					case 3:
						// left
						startingLocation.X = -characterMeasure.X - rand.Next(offScreenMinDistance, offScreenMaxDistance);
						startingLocation.Y = (int)(rand.NextDouble() * viewSize.Y);
						break;
					default:
						//wtf
						Debug.Assert(false, "The random number we generated above didn't follow the rules");
						startingLocation = Vector2.Zero;
						break;
					}

					movingCharacter.Location = startingLocation;
					movingCharacter.Destination = new Vector2(currentXOffset, yOffset);
					
				}

				// Move the next character over
				currentXOffset += characterMeasure.X;
			}
		}



		/// <summary>
		/// Explodes this word off-screen
		/// </summary>
		public void Explode()
		{
			
			foreach (MovableCharacter movingCharacter in characters)
			{
				Vector2 characterMeasure = Fonts.CreditsFont.MeasureString("" + movingCharacter.Character) * scale;

				if (movingCharacter.Character != ' ')
				{
					Vector2 destination;

					switch (rand.Next(0, 4))
					{
					case 0:
						// top
						destination.X = (int)(rand.NextDouble() * viewSize.X);
						destination.Y = -characterMeasure.Y - rand.Next(offScreenMinDistance, offScreenMaxDistance);
						break;
					case 1:
						// right
						destination.X = viewSize.X + rand.Next(offScreenMinDistance, offScreenMaxDistance);
						destination.Y = (int)(rand.NextDouble() * viewSize.Y);
						break;
					case 2:
						// bottom
						destination.X = (int)(rand.NextDouble() * viewSize.X);
						destination.Y = viewSize.Y + rand.Next(offScreenMinDistance, offScreenMaxDistance);
						break;
					case 3:
						// left
						destination.X = -characterMeasure.X - rand.Next(offScreenMinDistance, offScreenMaxDistance);
						destination.Y = (int)(rand.NextDouble() * viewSize.Y);
						break;
					default:
						//wtf
						Debug.Assert(false, "The random number we generated above didn't follow the rules");
						destination = Vector2.Zero;
						break;
					}

					movingCharacter.Destination = destination;
				}
			}
		}


		/// <summary>
		/// Updates this word
		/// </summary>
		/// <param name="deltaTime">The amount of time that has passed since the last update</param>
		public void Update(TimeSpan deltaTime)
		{
			foreach(MovableCharacter movingCharater in characters)
			{
				movingCharater.Update(deltaTime);
			}
		}


		/// <summary>
		/// Draws this word
		/// </summary>
		/// <param name="deltaTime">The amount of time since the last draw</param>
		/// <param name="tint"></param>
		public void Draw(SpriteBatch spriteBatch, Color tint)
		{
			if (visible)
			{
				foreach (MovableCharacter movingCharater in characters)
				{
					movingCharater.Draw(spriteBatch, tint);
				}
			}
		}


		/// <summary>
		/// Gets or sets whether this word is visible or not
		/// </summary>
		public bool Visible
		{
			get
			{
				return visible; 
			}
			set
			{
				visible = value;
			}
		}


		public void Reset()
		{
			foreach (MovableCharacter movingCharater in characters)
			{
				movingCharater.Location = Vector2.Zero;
				movingCharater.Velocity = Vector2.Zero;
			}
		}
	}
}
