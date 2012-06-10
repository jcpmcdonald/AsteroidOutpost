using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AsteroidOutpost.Components;
using AsteroidOutpost.Entities;
using AsteroidOutpost.Entities.Structures;
using AsteroidOutpost.Entities.Units;
using AsteroidOutpost.Networking;
using AsteroidOutpost.Scenarios;
using AsteroidOutpost.Screens;
using AsteroidOutpost.Screens.Credits;
using AsteroidOutpost.Screens.HeadsUpDisplay;
using Awesomium.Core;
using AwesomiumXNA;
using C3.XNA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Threading;
using Microsoft.Xna.Framework.Media;
using Console = AsteroidOutpost.Screens.HeadsUpDisplay.Console;

namespace AsteroidOutpost
{
	/// <summary>
	/// This is the entry point for the game and is responsible for creating the menu and the world
	/// </summary>
	public class AOGame : Game
	{

		#region user32 SetWindowPos

		[DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int cx, int cy, int wFlags);

		private const short SWP_NOMOVE = 0X2;
		private const short SWP_NOSIZE = 1;
		private const short SWP_NOZORDER = 0X4;
		private const int SWP_SHOWWINDOW = 0x0040;

		#endregion


		#region Fields

		private bool moveWindow = false;
		private int moveWindowX;
		private int moveWindowY;


		private GraphicsDeviceManager graphics;
		private SpriteBatch spriteBatch;
		private Settings settings;

		private AwesomiumComponent awesomium;

		private Song menuMusic;
		private bool musicStarted = false;

		private LayeredStarField starField;
		private World world;

		#endregion


		#region Properties

		public AwesomiumComponent Awesomium
		{
			get
			{
				return awesomium;
			}
		}

		#endregion


		#region Construct

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
			TextureDictionary.SetContent(Content);

			settings = new Settings();
			//world = new World(this);
			starField = new LayeredStarField(this);
			Components.Add(starField);

			initGraphicsMode(width, height, fullScreen);


#if UNLIMITED_FPS && DEBUG
			graphics.SynchronizeWithVerticalRetrace = false;
#endif

			// Create our web front end
			awesomium = new AwesomiumComponent(this);
			WebCore.BaseDirectory = @"..\UI\";
			awesomium.WebView.LoadFile("MainMenu.html");
			awesomium.WebView.Focus();
			Components.Add(awesomium);

			// Create callbacks for Awesomium content to communicate with the game
			awesomium.WebView.CreateObject("xna");
			awesomium.WebView.SetObjectCallback("xna", "StartWorld", StartWorld);
			awesomium.WebView.SetObjectCallback("xna", "Exit", Exit);

			IsFixedTimeStep = false;
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

		#endregion



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

			spriteBatch = new SpriteBatch(GraphicsDevice);
			Fonts.Initialize(Content);

			Ship1.LoadContent(GraphicsDevice, Content);
			SolarStation.LoadContent(GraphicsDevice, Content);
			Asteroid.LoadContent(GraphicsDevice, Content);
			LaserMiner.LoadContent(GraphicsDevice, Content);
			LaserTower.LoadContent(GraphicsDevice, Content);
			PowerNode.LoadContent(GraphicsDevice, Content);
			Beacon.LoadContent(GraphicsDevice, Content);

			TextureDictionary.Add("Sprites\\Power", "power");
			TextureDictionary.Add("Sprites\\Asteroids", "asteroids");
			TextureDictionary.Add("Sprites\\Miners", "miners");
			TextureDictionary.Add("Sprites\\SolarStation", "solarStation");
			TextureDictionary.Add("Sprites\\Spaceship", "spaceship");

			TextureDictionary.Add("Ellipse25", "ellipse25");
			TextureDictionary.Add("Ellipse25Bold", "ellipse25bold");
			TextureDictionary.Add("Ellipse25Back", "ellipse25back");
			TextureDictionary.Add("Ellipse25Front", "ellipse25front");
			TextureDictionary.Add("Ellipse50", "ellipse50");
			TextureDictionary.Add("Ellipse50Back", "ellipse50back");
			TextureDictionary.Add("Ellipse50Front", "ellipse50front");
			TextureDictionary.Add("Ellipse100", "ellipse100");
			TextureDictionary.Add("Ellipse220", "ellipse220");

			TextureDictionary.Add("powerline", "powerline");

			TextureDictionary.Add("Cursor");

			menuMusic = Content.Load<Song>(@"Music\Soulfrost - You Should Have Never Trusted Hollywood EP - 04 Inner Battles (Bignic Remix)");
			MediaPlayer.IsRepeating = true;
		}

		/// <summary>
		/// UnloadContent will be called once per game and is the place to unload
		/// all content.
		/// </summary>
		protected override void UnloadContent()
		{
			// Note: Is this abuse?
			MediaPlayer.Stop();
			menuMusic.Dispose();
		}


		/// <summary>
		/// Update the game
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

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
		}
		
		
		/// <summary>
		/// Draw the game
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			spriteBatch.Begin();
			starField.Draw(spriteBatch, Color.White);
			spriteBatch.End();

			// Note: This needs to be called before we can draw Awesomium
			base.Draw(gameTime);

			// Draw Awesomium and the cursor last
			spriteBatch.Begin();
			spriteBatch.Draw(awesomium.WebViewTexture, GraphicsDevice.Viewport.Bounds, Color.White);
			spriteBatch.Draw(TextureDictionary.Get("Cursor"), new Vector2(Mouse.GetState().X - 20, Mouse.GetState().Y - 20), Color.White);
			spriteBatch.End();
		}


		protected void StartWorld(object sender, JSCallbackEventArgs e)
		{
			String mapName = e.Arguments[0].ToString();
			System.Console.WriteLine("");
			if(world != null)
			{
				// The world should always be null before starting to make a new one
				Debugger.Break();
			}

			world = new World(this);
			RandomScenario randomScenario = new RandomScenario(world, 1);
			randomScenario.Start();

			Components.Add(world);
		}
		
		
		/// <summary>
		/// Exit callback so that JavaScript can exit the game
		/// </summary>
		public void Exit(object sender, JSCallbackEventArgs e)
		{
			Exit();
		}

	}
}