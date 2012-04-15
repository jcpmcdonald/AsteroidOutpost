using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AsteroidOutpost.Entities;
using AsteroidOutpost.Entities.Structures;
using AsteroidOutpost.Networking;
using AsteroidOutpost.Screens;
using AsteroidOutpost.Screens.Credits;
using AsteroidOutpost.Screens.HeadsUpDisplay;
using C3.XNA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Threading;
using Microsoft.Xna.Framework.Media;

namespace AsteroidOutpost
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class AOGame : Game
	{

		[DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int cx, int cy, int wFlags);

		private const short SWP_NOMOVE = 0X2;
		private const short SWP_NOSIZE = 1;
		private const short SWP_NOZORDER = 0X4;
		private const int SWP_SHOWWINDOW = 0x0040;

		private bool moveWindow = false;
		private int moveWindowX;
		private int moveWindowY;


		private GraphicsDeviceManager graphics;
		private SpriteBatch spriteBatch;
		private Settings settings;

		// TODO: Think of a better place to put these
		private Song menuMusic;
		private bool musicStarted = false;


		// TODO: Allow all screens to dispose
		private ServerBrowserScreen serverBrowser;		// This needs to be disposed
		private ScreenManager screenManager;


		public AOGame(int width, int height, bool fullScreen)
		{
			Init(width, height, fullScreen);
		}

		public AOGame(int moveToX, int moveToY, int width, int height)
		{
			moveWindow = true;
			moveWindowX = moveToX;
			moveWindowY = moveToY;

			Init(width, height, false);
		}


		private void Init(int width, int height, bool fullScreen)
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";

			settings = new Settings();

			initGraphicsMode(width, height, fullScreen);


#if UNLIMITED_FPS && DEBUG
			graphics.SynchronizeWithVerticalRetrace = false;
#endif

			//IsFixedTimeStep = false;
		}
		
		
		/// <summary>
		/// Attempt to set the display mode to the desired resolution.  Iterates through the display
		/// capabilities of the default graphics adapter to determine if the graphics adapter supports the
		/// requested resolution.  If so, the resolution is set and the function returns true.  If not,
		/// no change is made and the function returns false.
		/// </summary>
		/// <param name="iWidth">Desired screen width.</param>
		/// <param name="iHeight">Desired screen height.</param>
		/// <param name="bFullScreen">True if you wish to go to Full Screen, false for Windowed Mode.</param>
		private bool initGraphicsMode(int iWidth, int iHeight, bool bFullScreen)
		{
			// If we aren't using a full screen mode, the height and width of the window can
			// be set to anything equal to or smaller than the actual screen size.
			if (bFullScreen == false)
			{
				if ((iWidth <= GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width)
				    && (iHeight <= GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height))
				{
					graphics.PreferredBackBufferWidth = iWidth;
					graphics.PreferredBackBufferHeight = iHeight;
					graphics.IsFullScreen = false;
					graphics.ApplyChanges();
					return true;
				}
			}
			else
			{
				// If we are using full screen mode, we should check to make sure that the display
				// adapter can handle the video mode we are trying to set.  To do this, we will
				// iterate through the display modes supported by the adapter and check them against
				// the mode we want to set.
				foreach (DisplayMode dm in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
				{
					// Check the width and height of each mode against the passed values
					if ((dm.Width == iWidth) && (dm.Height == iHeight))
					{
						// The mode is supported, so set the buffer formats, apply changes and return
						graphics.PreferredBackBufferWidth = iWidth;
						graphics.PreferredBackBufferHeight = iHeight;
						graphics.IsFullScreen = true;
						graphics.ApplyChanges();
						return true;
					}
				}
			}
			return false;
		}
		
		
		
		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{

			if (moveWindow)
			{
				SetWindowPos(Window.Handle, 0, moveWindowX, moveWindowY, 0, 0, SWP_NOZORDER | SWP_NOSIZE | SWP_SHOWWINDOW);
			}

			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch(GraphicsDevice);
			TextureDictionary.SetContent(Content);

			Fonts.Initialize(Content);

			// Set up some colours to use
			ColorPalette.ActiveBorder        = new Color(73, 73, 73, 180);
			ColorPalette.ActiveCaption       = new Color(46, 46, 46, 180);
			ColorPalette.ActiveCaptionText   = new Color(255, 255, 255, 255);
			ColorPalette.ButtonFace          = new Color(73, 73, 73, 180);
			ColorPalette.ButtonHighlight     = new Color(100, 100, 100, 180);
			ColorPalette.ButtonShadow        = new Color(46, 46, 46, 180);
			ColorPalette.Control             = new Color(73, 73, 73, 180);
			ColorPalette.ControlDark         = new Color(46, 46, 46, 180);
			ColorPalette.ControlDarkDark     = new Color(30, 30, 30, 180);
			ColorPalette.ControlLight        = new Color(100, 100, 100, 180);
			ColorPalette.ControlLightLight   = new Color(120, 120, 120, 180);
			ColorPalette.ControlText         = new Color(255, 255, 255, 255);
			ColorPalette.GrayText            = new Color(190, 190, 190, 255);
			ColorPalette.Highlight           = new Color(100, 100, 150, 180);
			ColorPalette.HighlightText       = new Color(255, 255, 255, 255);
			ColorPalette.InactiveBorder      = new Color(100, 100, 100, 180);
			ColorPalette.InactiveCaption     = new Color(30, 30, 30, 180);
			ColorPalette.InactiveCaptionText = new Color(230, 230, 230, 255);
			ColorPalette.ScrollBar           = new Color(46, 46, 46, 180);
			ColorPalette.Window              = new Color(73, 73, 73, 180);
			ColorPalette.WindowFrame         = new Color(73, 73, 73, 180);
			ColorPalette.WindowText          = new Color(255, 255, 255, 255);


			screenManager = new ScreenManager(this, graphics);
			AsteroidOutpostScreen gameScreen = new AsteroidOutpostScreen(screenManager);
			LayeredStarField starField = new LayeredStarField(screenManager, gameScreen);
			gameScreen.SetScene(starField);
			MainMenuScreen mainMenuScreen = new MainMenuScreen(screenManager, starField);
			CreditsScreen creditsScreen = new CreditsScreen(screenManager, starField);
			serverBrowser = new ServerBrowserScreen(gameScreen, screenManager, starField);
			ServerHostScreen serverHost = new ServerHostScreen(gameScreen, screenManager, starField);
			LobbyScreen gameLobby = new LobbyScreen(gameScreen, screenManager, starField);
			MissionSelectScreen missionSelect = new MissionSelectScreen(gameScreen, screenManager, starField);


			screenManager.AddScreen("Main Menu", mainMenuScreen);
			screenManager.AddScreen("Game", gameScreen);
			screenManager.AddScreen("Credits", creditsScreen);
			screenManager.AddScreen("Starfield", starField);
			screenManager.AddScreen("Server Browser", serverBrowser);
			screenManager.AddScreen("Server Host", serverHost);
			screenManager.AddScreen("Lobby", gameLobby);
			screenManager.AddScreen("Mission Select", missionSelect);

			screenManager.SetBackground("Starfield");

			menuMusic = Content.Load<Song>(@"Music\Soulfrost - You Should Have Never Trusted Hollywood EP - 04 Inner Battles (Bignic Remix)");
			MediaPlayer.IsRepeating = true;

			screenManager.LoadContent(spriteBatch, Content);
		}

		/// <summary>
		/// UnloadContent will be called once per game and is the place to unload
		/// all content.
		/// </summary>
		protected override void UnloadContent()
		{
			// Unload any non ContentManager content here
			screenManager.UnloadContent();
			serverBrowser.Dispose();

			MediaPlayer.Stop();
			menuMusic.Dispose();
		}


		/// <summary>
		/// Update the world
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
			if (!musicStarted)
			{
				if (settings.MusicVolume > 0)
				{
					MediaPlayer.Volume = settings.MusicVolume * 0.5f;
					MediaPlayer.Play(menuMusic);
				}
				musicStarted = true;
			}

			// Allows the game to exit if I ever load this on an XBox
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
			{
				Exit();
			}

			// Update the screen manager
			screenManager.Update(gameTime.ElapsedGameTime);
		}
		
		
		/// <summary>
		/// Draw the world
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			// Draw the screen manager
			screenManager.Draw(spriteBatch, gameTime.ElapsedGameTime);
		}



	}
}