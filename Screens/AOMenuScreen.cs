using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using C3.XNA;
using C3.XNA.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AsteroidOutpost.Screens
{
	public abstract class AOMenuScreen : Screen
	{
		private readonly LayeredStarField starField;
		protected Panel menuPanel;

		protected AOMenuScreen(ScreenManager theScreenManager, LayeredStarField starField)
			: base(theScreenManager)
		{
			this.starField = starField;
		}


		public override void LoadContent(SpriteBatch spriteBatch, Microsoft.Xna.Framework.Content.ContentManager content)
		{
			Size panelSize = new Size(1024, 768);
			menuPanel = new Panel((ScreenMan.Viewport.Width / 2) - (panelSize.Width / 2),
								  (ScreenMan.Viewport.Height / 2) - (panelSize.Height / 2),
								  panelSize.Width, panelSize.Height);


			AddControl(menuPanel);

			base.LoadContent(spriteBatch, content);
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

			// Make the starfield move
			starField.Move((float)(100.0 * deltaTime.TotalSeconds), (float)(60.0 * deltaTime.TotalSeconds));
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
			// Don't handle updates if we are being transitioned away from. It can cause some interesting effects for power users
			//if (percentComplete < 0.5)
			//{
			//	base.Update(deltaTime, theMouse, theKeyboard);
			//}

			if (isInFocus)
			{
				isInFocus = false;
				StartTransitionAway(deltaTime, theMouse, theKeyboard);
			}

			// Make the starfield move a little less
			starField.Move((float)(100.0 * deltaTime.TotalSeconds * (1.0f - percentComplete)), (float)(60.0 * deltaTime.TotalSeconds * (1.0f - percentComplete)));
		}


		/// <summary>
		/// Updates this screen while we are being transitioned toward
		/// </summary>
		/// <param name="deltaTime">The amount of time that has passed since the last update</param>
		/// <param name="theMouse">The current state of the mouse</param>
		/// <param name="theKeyboard">The current state of the keyboard</param>
		/// <param name="percentComplete">The transition's percentage complete (0-1)</param>
		protected override void UpdateTransitionToward(TimeSpan deltaTime, EnhancedMouseState theMouse, EnhancedKeyboardState theKeyboard, float percentComplete)
		{
			if (percentComplete > 0.5)
			{
				base.Update(deltaTime, theMouse, theKeyboard);
			}

			if (!isInFocus)
			{
				isInFocus = true;
				StartTransitionToward(deltaTime, theMouse, theKeyboard);
			}

			// Make the starfield move a little more
			starField.Move((float)(100.0 * deltaTime.TotalSeconds * percentComplete), (float)(60.0 * deltaTime.TotalSeconds * percentComplete));
		}
		
		
		/// <summary>
		/// Draw this control
		/// </summary>
		/// <param name="spriteBatch">The spriteBatch to draw to</param>
		/// <param name="tint">The color used to tint this control. Use Color.White to draw this control normally</param>
		public override void Draw(SpriteBatch spriteBatch, Color tint)
		{
			base.Draw(spriteBatch, tint);

			DrawMouse(spriteBatch, tint);
		}


		/// <summary>
		/// Draw the mouse
		/// </summary>
		/// <param name="spriteBatch">The spriteBatch to draw to</param>
		/// <param name="tint">The color used to tint this control. Use Color.White to draw this control normally</param>
		protected virtual void DrawMouse(SpriteBatch spriteBatch, Color tint)
		{
			spriteBatch.Draw(TextureDictionary.Get("Cursor"), new Vector2(ScreenMan.Mouse.X - 20, ScreenMan.Mouse.Y - 20), Color.White);
		}


		/// <summary>
		/// Automatically adds all the controls you have defined to the menu panel
		/// </summary>
		/// <param name="instance">The instance of your screen</param>
		protected void AddAllLocalControls(AOMenuScreen instance)
		{
			// Get the private fields that are declared in the instance
			FieldInfo[] localFields = instance.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
			foreach (FieldInfo localField in localFields)
			{
				Control myControl = localField.GetValue(instance) as Control;
				// TODO: Hack Hack! Fix this using attributes maybe?
				if (myControl != null && localField.Name != "world")
				{
					menuPanel.AddControl(myControl);
				}
			}
		}
	}
}
