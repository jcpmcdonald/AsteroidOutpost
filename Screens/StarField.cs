using System;
using System.Collections.Generic;
using System.IO;
using C3.XNA;
using C3.XNA.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AsteroidOutpost
{

	public enum Prominance
	{
		HIGH,
		MEDIUM,
		LOW
	}

	public class StarField : Screen
	{
		readonly List<Star> stars;
		Viewport viewport;
		Vector2? lastFocusPoint;

		private Texture2D icePlanet;
		
		public StarField(ScreenManager theScreenManager, int starCount, Viewport theViewport, Prominance prominance) : base(theScreenManager)
		{
			viewport = theViewport;
			
			stars = new List<Star>(starCount);
			for(int i = 0; i < starCount; i++)
			{
				stars.Add(new Star(viewport.X, viewport.X + viewport.Width,
				                   viewport.Y, viewport.Y + viewport.Height, prominance));
			}
		}

		public override void LoadContent(SpriteBatch spriteBatch, Microsoft.Xna.Framework.Content.ContentManager content)
		{
			base.LoadContent(spriteBatch, content);

			//icePlanet = Texture2D.FromStream(spriteBatch.GraphicsDevice, File.OpenRead(@"..\scene\IcePlanet.png"));
			icePlanet = content.Load<Texture2D>("Scenes\\IcePlanet");
		}
		
		
		public void Update(TimeSpan deltaTime, Rectangle? focusScreen, float scaleFactor)
		{
			if (lastFocusPoint == null || focusScreen == null)
			{
				// can't really do anything
			}
			else
			{
				// Note: Smelly code! We are calculating the Center of the rectangle that is being calculated from a Center point earlier on
				double deltaX = (lastFocusPoint.Value.X - focusScreen.Value.Center.X) / scaleFactor;
				double deltaY = (lastFocusPoint.Value.Y - focusScreen.Value.Center.Y) / scaleFactor;
				Move(deltaX, deltaY);
			}

			if (focusScreen != null)
			{
				lastFocusPoint = new Vector2(focusScreen.Value.Center.X, focusScreen.Value.Center.Y);
			}
			else
			{
				lastFocusPoint = null;
			}
		}


		public override void HandleKeyboardActions(TimeSpan deltaTime, EnhancedKeyboardState theKeyboard)
		{
			// Kill any keyboard handling
		}


		public override void HandleMouseActions(TimeSpan deltaTime, EnhancedMouseState theMouse, ref bool handled)
		{
			// Kill any mouse handling
		}



		public void Move(double deltaX, double deltaY)
		{
			foreach (Star star in stars)
			{
				star.Move(deltaX, deltaY, 0, viewport.Width, 0, viewport.Height);
			}
		}


		/// <summary>
		/// Draw this control
		/// </summary>
		/// <param name="spriteBatch">The spriteBatch to draw to</param>
		/// <param name="tint">The color used to tint this control. Use Color.White to draw this control normally</param>
		public override void Draw(SpriteBatch spriteBatch, Color tint)
		{
			ScreenMan.GraphicsDevice.Clear(ColorPalette.ApplyTint(Color.Black, tint));

			foreach(Star star in stars)
			{
				star.Draw(spriteBatch);
			}

			Vector2 planetScale = new Vector2(1f);
			int screenWidth = (int)(spriteBatch.GraphicsDevice.Viewport.Width / planetScale.X);
			int screenHeight = (int)(spriteBatch.GraphicsDevice.Viewport.Height / planetScale.Y);
			spriteBatch.Draw(icePlanet, new Vector2(screenWidth - (screenWidth * 2f / 3f), 400f), null, tint, 0, Vector2.Zero, planetScale, SpriteEffects.None, 0);

			base.Draw(spriteBatch, tint);
		}
		
	}

	class Star
	{
		private int size;
		private Vector2 pos;

		private double moveFactor;
		private Color brightness;
		private int brightnessLevel;
		private static readonly Random rand = new Random();

		public Star(float minX, float maxX, float minY, float maxY, Prominance prominance)
		{
			Randomize(minX, maxX, minY, maxY, prominance);
		}


		private void Randomize(float minX, float maxX, float minY, float maxY, Prominance prominance)
		{
			RandomizeX(minX, maxX);
			RandomizeY(minY, maxY);
			RandomizeSize(prominance);
			RandomizeBrightness(prominance);
			RandomizeMoveFactor();
		}
		
		private void RandomizeX(float minX, float maxX)
		{
			pos.X = rand.Next((int)minX, (int)(maxX - 1.0));
		}

		private void RandomizeY(float minY, float maxY)
		{
			pos.Y = rand.Next((int)minY, (int)(maxY - 1.0));
		}

		private void RandomizeSize(Prominance prominance)
		{
			double num = rand.NextDouble();

			switch (prominance)
			{
			default:
				goto case Prominance.LOW;
			case Prominance.LOW:
				if (num < 0.95)
				{
					size = 1;
				}
				else if (num < 0.98)
				{
					size = 2;
				}
				else
				{
					size = 3;
				}
				break;


			case Prominance.MEDIUM:
				if (num < 0.75)
				{
					size = 1;
				}
				else if (num < 0.90)
				{
					size = 2;
				}
				else
				{
					size = 3;
				}
				break;


			case Prominance.HIGH:
				if (num < 0.50)
				{
					size = 1;
				}
				else if (num < 0.75)
				{
					size = 2;
				}
				else
				{
					size = 3;
				}
				break;
			}
		}

		private void RandomizeBrightness(Prominance prominance)
		{
			byte rgbVal;
			double num = rand.NextDouble();
			switch(size)
			{
			case 1:
				rgbVal = (byte)(num * 40 + 20);
				brightness = new Color(rgbVal, rgbVal, rgbVal);
				if(num < 0.3)
				{
					brightnessLevel = 0;
				}
				else if(num < 0.5)
				{
					brightnessLevel = 1;
				}
				else if(num < 0.97)
				{
					brightnessLevel = 2;
				}
				else
				{
					brightnessLevel = 3;
				}
				break;
					
					
			case 2:
				rgbVal = (byte)(num * 70 + 20);
				brightness = new Color(rgbVal, rgbVal, rgbVal);
				if(num < 0.3)
				{
					brightnessLevel = 1;
				}
				else if(num < 0.8)
				{
					brightnessLevel = 2;
				}
				else
				{
					brightnessLevel = 3;
				}
				break;
					
					
			case 3:
				rgbVal = (byte)(num * 100 + 20);
				brightness = new Color(rgbVal, rgbVal, rgbVal);
				if(num < 0.05)
				{
					brightnessLevel = 1;
				}
				else if(num < 0.80)
				{
					brightnessLevel = 2;
				}
				else
				{
					brightnessLevel = 3;
				}
				break;
			}
		}
		
		// Randomize the movement factor based on the size of the star, the smaller the slower ( & farther it looks)
		private void RandomizeMoveFactor()
		{
			moveFactor = 0.0;
			double high = 0.0;
			double low = 0.0;
			switch(size + brightnessLevel)
			{
			case 1:
				low = 0.001;
				high = 0.05;
				break;
						
			case 2:
				low = 0.03;
				high = 0.15;
				break;
					
			case 3:
				low = 0.20;
				high = 0.40;
				break;
					
			case 4:
				low = 0.25;
				high = 0.55;
				break;
					
			case 5:
				low = 0.30;
				high = 0.65;
				break;
					
			case 6:
				low = 0.40;
				high = 0.70;
				break;
			}
			
			moveFactor = low + ((high - low + 1.0) * rand.NextDouble());
		}


		internal void Move(double deltaX, double deltaY, float minX, float maxX, float minY, float maxY)
		{
			MoveX(deltaX, minX, maxX);
			MoveY(deltaY, minY, maxY);
		}


		private void MoveX(double delta, float minX, float maxX)
		{
			pos.X += (float)((delta * 0.02) * moveFactor);

			// Wrap the star
			while (pos.X > maxX)
			{
				pos.X -= (maxX - minX);
			}
			while (pos.X < minX)
			{
				pos.X += (maxX - minX);
			}
		}

		private void MoveY(double delta, float minY, float maxY)
		{
			pos.Y += (float)((delta * 0.02) * moveFactor);

			// Wrap the star
			while (pos.Y > maxY)
			{
				pos.Y -= (maxY - minY);
			}
			while (pos.Y < minY)
			{
				pos.Y += (maxY - minY);
			}
		}

		internal void Draw(SpriteBatch spriteBatch)
		{
			spriteBatch.FillRectangle(pos, new Vector2(size), brightness);
		}
	}
}