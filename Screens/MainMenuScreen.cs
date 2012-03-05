using System;
using AsteroidOutpost.Scenarios;
using C3.XNA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace AsteroidOutpost.Screens
{
	partial class MainMenuScreen : AOMenuScreen
	{
		// Set to true if the mouse should fade for the current transition
		private bool shouldFadeMouse;


		public MainMenuScreen(ScreenManager theScreenManager, LayeredStarField starField)
			: base(theScreenManager, starField)
		{
		}


		void button_MouseEnter(object sender, C3.XNA.Events.MouseEventArgs e)
		{
			mouseOverSound.Play();
		}

		void btnSinglePlayer_Clicked(object sender, EventArgs e)
		{
			ScreenMan.SwitchScreens("Mission Select");
		}

		void btnMultiPlayer_Clicked(object sender, EventArgs e)
		{
			ScreenMan.SwitchScreens("Server Browser");
		}

		void btnCredits_Clicked(object sender, EventArgs e)
		{
			shouldFadeMouse = true;
			ScreenMan.SwitchScreens("Credits");
		}

		void btnExit_Clicked(object sender, EventArgs e)
		{
			ScreenMan.Exit();
		}


		/// <summary>
		/// Draw this control
		/// </summary>
		/// <param name="spriteBatch">The spriteBatch to draw to</param>
		/// <param name="tint">The color used to tint this control. Use Color.White to draw this control normally</param>
		public override void Draw(SpriteBatch spriteBatch, Color tint)
		{
			base.Draw(spriteBatch, tint);

			// Draw the title text
			titleText.Draw(spriteBatch, tint, new Vector2((ScreenMan.Viewport.Width / 2) - (titleText.Width / 2), (ScreenMan.Viewport.Height / 2) - 300));
		}


		/// <summary>
		/// Draw the mouse
		/// </summary>
		/// <param name="spriteBatch">The spriteBatch to draw to</param>
		/// <param name="tint">The color used to tint this control. Use Color.White to draw this control normally</param>
		protected override void DrawMouse(SpriteBatch spriteBatch, Color tint)
		{
			if (shouldFadeMouse && ScreenMan.IsTransitioning)
			{
				spriteBatch.Draw(TextureDictionary.Get("Cursor"), new Vector2(ScreenMan.Mouse.X - 20, ScreenMan.Mouse.Y - 20), tint);
			}
			else
			{
				shouldFadeMouse = false;
				spriteBatch.Draw(TextureDictionary.Get("Cursor"), new Vector2(ScreenMan.Mouse.X - 20, ScreenMan.Mouse.Y - 20), Color.White);
			}
		}
	}
}
