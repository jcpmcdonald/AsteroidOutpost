using System;
using System.Collections.Generic;
using System.Diagnostics;
using C3.XNA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AsteroidOutpost.Screens.Credits
{

	class CreditsScreen : AOMenuScreen
	{
		private Dictionary<String, ExplodingWord> words = new Dictionary<String, ExplodingWord>();
		private List<ExplodingWord> currentWords = new List<ExplodingWord>(5);
		private int activeSequence = -1;
		private TimeSpan countdownToNextSequence = new TimeSpan(0);

		private bool resetCredits;


		public CreditsScreen(ScreenManager theScreenManager, LayeredStarField starfield)
			: base(theScreenManager, starfield)
		{
		}


		public override void LoadContent(SpriteBatch spriteBatch, Microsoft.Xna.Framework.Content.ContentManager content)
		{
			Vector2 viewSize = new Vector2(ScreenMan.Viewport.Width, ScreenMan.Viewport.Height);
			float titleScale = 1.0f;
			float nameScale = 1.2f;

			words.Add("Core Programming", new ExplodingWord("Core Programming", 350, titleScale, viewSize, Color.Gray));
			words.Add("John McDonald", new ExplodingWord("John (Cell) McDonald", 200, nameScale, viewSize, Color.White));
			words.Add("John McDonald2", new ExplodingWord("John (Cell) McDonald", 200, nameScale, viewSize, Color.White));	// Since I will appear a lot, make two of me's so that I can be imploding while exploding

			words.Add("Additional Programming", new ExplodingWord("Additional Programming", 350, titleScale, viewSize, Color.Gray));
			words.Add("Gary Texmo", new ExplodingWord("Gary (Trinith) Texmo", 200, nameScale, viewSize, Color.White));
			words.Add("Justin Huskic", new ExplodingWord("Justin (gdi1942) Huskic", 200, nameScale, viewSize, Color.White));

			words.Add("Art", new ExplodingWord("Art", 350, titleScale, viewSize, Color.Gray));
			words.Add("Jesse Ninos", new ExplodingWord("Jesse (Shogun) Ninos [Solar Station]", 200, nameScale, viewSize, Color.White));
			words.Add("Abraham Katase", new ExplodingWord("Abraham Katase [Space Ship]", 200, nameScale, viewSize, Color.White));

			words.Add("Music & Sounds", new ExplodingWord("Music & Sounds", 350, titleScale, viewSize, Color.Gray));
			words.Add("Sufista", new ExplodingWord("CJ (Sufista)", 200, nameScale, viewSize, Color.White));
			words.Add("Bignic", new ExplodingWord("(Bignic)", 200, nameScale, viewSize, Color.White));

			base.LoadContent(spriteBatch, content);
			menuPanel.Visible = false;
		}


		


		/// <summary>
		/// Updates this screen
		/// </summary>
		/// <param name="deltaTime">The amount of time that has passed since the last update</param>
		/// <param name="theMouse">The current state of the mouse</param>
		/// <param name="theKeyboard">The current state of the keyboard</param>
		public override void Update(TimeSpan deltaTime, EnhancedMouseState theMouse, EnhancedKeyboardState theKeyboard)
		{
			base.Update(deltaTime, theMouse, theKeyboard);

			updateWords(deltaTime);
		}


		/// <summary>
		/// Updates this screen while we are being transitioned out
		/// </summary>
		/// <param name="deltaTime">The amount of time that has passed since the last update</param>
		/// <param name="theMouse">The current state of the mouse</param>
		/// <param name="theKeyboard">The current state of the keyboard</param>
		/// <param name="percentComplete">The transition's percentage complete (0-1)</param>
		protected override void UpdateTransitionAway(TimeSpan deltaTime, EnhancedMouseState theMouse, EnhancedKeyboardState theKeyboard, float percentComplete)
		{
			base.UpdateTransitionAway(deltaTime, theMouse, theKeyboard, percentComplete);

			// The next time we come into focus, reset the credits
			resetCredits = true;

			updateWords(deltaTime);
		}


		/// <summary>
		/// Updates this screen while we are being transitioned out
		/// </summary>
		/// <param name="deltaTime">The amount of time that has passed since the last update</param>
		/// <param name="theMouse">The current state of the mouse</param>
		/// <param name="theKeyboard">The current state of the keyboard</param>
		/// <param name="percentComplete">The transition's percentage complete (0-1)</param>
		protected override void UpdateTransitionToward(TimeSpan deltaTime, EnhancedMouseState theMouse, EnhancedKeyboardState theKeyboard, float percentComplete)
		{
			base.UpdateTransitionToward(deltaTime, theMouse, theKeyboard, percentComplete);

			// Reset the credits
			if(resetCredits)
			{
				resetCredits = false;

				activeSequence = -1;
				foreach (ExplodingWord word in words.Values)
				{
					word.Reset();
					word.Visible = false;
				}
				currentWords.Clear();
				countdownToNextSequence = new TimeSpan(0);
			}

			updateWords(deltaTime);
		}


		protected override void OnMouseUp(EnhancedMouseState mouse, MouseButton mouseButton, ref bool handled)
		{
			// Skip the credits
			ScreenMan.SwitchScreens("Main Menu");
			base.OnMouseUp(mouse, mouseButton, ref handled);
		}

		protected override void OnKeyDown(EnhancedKeyboardState theKeyboard, Microsoft.Xna.Framework.Input.Keys key)
		{
			// Skip the credits
			ScreenMan.SwitchScreens("Main Menu");
			base.OnKeyDown(theKeyboard, key);
		}


		private void updateWords(TimeSpan deltaTime)
		{
			countdownToNextSequence -= deltaTime;

			if(countdownToNextSequence.TotalMilliseconds <= 0)
			{
				activeSequence++;

				// Move all the currently visible words off the screen
				foreach(ExplodingWord word in currentWords)
				{
					word.Explode();
				}
				currentWords.Clear();



				switch(activeSequence)
				{
				default:
					activeSequence = 0;
					goto case 0;
				case 0:
					currentWords.Add(words["Core Programming"]);
					currentWords.Add(words["John McDonald"]);

					countdownToNextSequence += TimeSpan.FromSeconds(7);
					break;


				case 1:
					currentWords.Add(words["Additional Programming"]);
					currentWords.Add(words["Gary Texmo"]);
					currentWords.Add(words["Justin Huskic"]);

					countdownToNextSequence += TimeSpan.FromSeconds(7);
					break;


				case 2:
					currentWords.Add(words["Art"]);
					currentWords.Add(words["Jesse Ninos"]);
					currentWords.Add(words["Abraham Katase"]);
					currentWords.Add(words["John McDonald2"]);

					countdownToNextSequence += TimeSpan.FromSeconds(10);
					break;


				case 3:
					currentWords.Add(words["Music & Sounds"]);
					currentWords.Add(words["Sufista"]);
					currentWords.Add(words["Bignic"]);

					countdownToNextSequence += TimeSpan.FromSeconds(7);
					break;
				}


				Vector2 onScreenDestination = new Vector2(ScreenMan.Viewport.Width / 2.0f, (ScreenMan.Viewport.Height / 2.0f) - 100);
				foreach (ExplodingWord word in currentWords)
				{
					word.Visible = true;
					word.Implode(onScreenDestination);
					onScreenDestination.Y = onScreenDestination.Y + 100;
				}


			}

			foreach (ExplodingWord word in words.Values)
			{
				word.Update(deltaTime);
			}
			//words[0].Implode(new Vector2(ScreenMan.Viewport.Width / 2, ScreenMan.Viewport.Height / 2));
		}


		public override void Draw(SpriteBatch spriteBatch, Color tint)
		{
			foreach (ExplodingWord word in words.Values)
			{
				word.Draw(spriteBatch, tint);
			}

			base.Draw(spriteBatch, tint);
		}


		/// <summary>
		/// Draw the mouse
		/// </summary>
		/// <param name="spriteBatch">The spriteBatch to draw to</param>
		/// <param name="tint">The color used to tint this control. Use Color.White to draw this control normally</param>
		protected override void DrawMouse(SpriteBatch spriteBatch, Color tint)
		{
			// Don't draw the mouse
		}

	}
}
