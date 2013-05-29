using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using AsteroidOutpost.Entities;
using AsteroidOutpost.Scenarios;
using AsteroidOutpost.Screens;
using Awesomium.Core;
using AwesomiumXNA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

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

		private readonly TimeSpan fixedDeltaTime = new TimeSpan(166667);
		private TimeSpan accumulatedTime = TimeSpan.Zero;

		private GraphicsDeviceManager graphics;
		private SpriteBatch spriteBatch;
		private Settings settings;

		private AwesomiumComponent awesomium;
		private FrameRateCounter frameRateCounter;

		private Song menuMusic;
		private bool musicStarted = false;

		private LayeredStarField starField;
		private World world;
		private Texture2D cursorTexture;

		private Stopwatch stopwatch = new Stopwatch();
		private bool destroyWorld = false;

		#endregion


		#region Properties

		public AwesomiumComponent Awesomium
		{
			get
			{
				return awesomium;
			}
		}

		public World World
		{
			get
			{
				return world;
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
			stopwatch.Start();

			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";

			settings = new Settings();
			//world = new World(this);
			starField = new LayeredStarField(this);
			Components.Add(starField);

			frameRateCounter = new FrameRateCounter(this);
			//Components.Add(frameRateCounter);

			initGraphicsMode(width, height, fullScreen);

			//#if UNLIMITED_FPS
#if false
			graphics.SynchronizeWithVerticalRetrace = false;
			IsFixedTimeStep = false;
#endif

			// Create our web front end
			//WebCoreConfig.Default.ForceSingleProcess = true;
			awesomium = new AwesomiumComponent(this);
			WebCore.BaseDirectory = @"..\UI\";
			awesomium.WebView.LoadFile("MainMenu.html");
			awesomium.WebView.Focus();
			Components.Add(awesomium);

			// Create callbacks for Awesomium content to communicate with the game
			awesomium.WebView.CreateObject("xna");
			awesomium.WebView.SetObjectCallback("xna", "StartWorld", StartWorld);
			awesomium.WebView.SetObjectCallback("xna", "Exit", Exit);

			// Create somewhere to log messages to
			awesomium.WebView.CreateObject("console");
			awesomium.WebView.SetObjectCallback("console", "log", JSConsoleLog);
			awesomium.WebView.SetObjectCallback("console", "dir", JSConsoleLog);

			awesomium.WebView.CreateObject("performanceMonitor");
			//JSValue[] updateTime = new JSValue[2];
			//updateTime[0] = new JSValue(5);
			//updateTime[1] = new JSValue(6);
			//awesomium.WebView.SetObjectProperty("performanceMonitor", "updateTime", new JSValue(updateTime));

			awesomium.WebView.JSConsoleMessageAdded += WebView_JSConsoleMessageAdded;
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

			cursorTexture = Texture2DEx.FromStreamWithPremultAlphas(GraphicsDevice, File.OpenRead(@"..\Content\Cursor.png"));

			//ThreadPool.QueueUserWorkItem(delegate { EntityFactory.LoadContent(GraphicsDevice); });
			//ThreadPool.QueueUserWorkItem(delegate { EllipseEx.LoadContent(GraphicsDevice); });
			EntityFactory.LoadContent(GraphicsDevice);
			EllipseEx.LoadContent(GraphicsDevice);

			//menuMusic = Content.Load<Song>(@"Music\Soulfrost - You Should Have Never Trusted Hollywood EP - 04 Inner Battles (Bignic Remix)");
			MediaPlayer.IsRepeating = true;

			if (stopwatch.IsRunning)
			{
				stopwatch.Stop();
				Console.WriteLine("Loaded in " + stopwatch.ElapsedMilliseconds + "ms");
			}
		}


		/// <summary>
		/// UnloadContent will be called once per game and is the place to unload
		/// all content.
		/// </summary>
		protected override void UnloadContent()
		{
			// Note: Is this abuse?
			MediaPlayer.Stop();
			if (menuMusic != null)
			{
				menuMusic.Dispose();
			}
		}


		/// <summary>
		/// Update the game
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
			while (gameTime.TotalGameTime > accumulatedTime + fixedDeltaTime)
			{
				accumulatedTime += fixedDeltaTime;
				frameRateCounter.StartOfUpdate(gameTime);

				base.Update(new GameTime(accumulatedTime, fixedDeltaTime));

				if (!musicStarted)
				{
					if (settings.MusicVolume > 0)
					{
						MediaPlayer.Volume = settings.MusicVolume * 0.5f;
						MediaPlayer.Play(menuMusic);
					}
					musicStarted = true;
				}


				frameRateCounter.EndOfUpdate(gameTime);


				if (awesomium.WebView.IsLive)
				{
					// Force this to be a synchronous call otherwise we could hammer this with update calls, and it can't keep up
					String js = String.Format("if(typeof RefreshPerformanceGraph != 'undefined'){{ RefreshPerformanceGraph({0}, {1}); }}", frameRateCounter.LastUpdateTime(), frameRateCounter.LastDrawTime());
					awesomium.WebView.ExecuteJavascriptWithResult(js, 100);
				}

				// Allows the game to exit if I ever load this on an XBox
				if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
				{
					Exit();
				}
			}
		}


		/// <summary>
		/// Draw the game
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			frameRateCounter.StartOfDraw(gameTime);

			spriteBatch.Begin();
			starField.Draw(spriteBatch, Color.White);
			spriteBatch.End();

			// Note: This needs to be called before we can draw Awesomium
			base.Draw(gameTime);

			// Draw Awesomium and the cursor last
			spriteBatch.Begin();
			spriteBatch.Draw(awesomium.WebViewTexture, GraphicsDevice.Viewport.Bounds, Color.White);
			spriteBatch.Draw(cursorTexture, new Vector2(Mouse.GetState().X - 20, Mouse.GetState().Y - 20), Color.White);
			spriteBatch.End();

			frameRateCounter.EndOfDraw();


			if (destroyWorld)
			{
				destroyWorld = false;
				Components.Remove(world);
				world.Dispose();
				world = null;
			}
		}


		/// <summary>
		/// Wipe the world so that we can start fresh
		/// </summary>
		public void DestroyWorld()
		{
			awesomium.WebView.LoadFile("MainMenu.html");
			destroyWorld = true;
		}


		#region JavaScript Callbacks

		private void StartWorld(object sender, JSCallbackEventArgs e)
		{
			//awesomium.WebView.LoadCompleted += HUD_LoadCompleted;

			String mapName = e.Arguments[0].ToString();
			if (World != null)
			{
				// The world should always be null before starting to make a new one, right?
				Debugger.Break();
			}

			world = new World(this);
			EntityFactory.Init(world);


			Scenario scenario;
			switch(mapName.ToLower())
			{
			case "tutorial":
				scenario = new TutorialScenario(this, 1);
				break;

			case "random":
				scenario = new RandomScenario(this, 1);
				break;

			default:
				goto case "random";
			}
			world.StartServer(scenario);

			Components.Add(World);
		}

		//void HUD_LoadCompleted(object sender, EventArgs e)
		//{
		//    awesomium.WebView.LoadCompleted -= HUD_LoadCompleted;

		//    //RandomScenario randomScenario = new RandomScenario(this, 1);
		//    TutorialScenario tutorial = new TutorialScenario(this, 1);
		//    world.StartServer(tutorial);
		//}


		/// <summary>
		/// Exit callback so that JavaScript can exit the game
		/// </summary>
		private void Exit(object sender, JSCallbackEventArgs e)
		{
			Exit();
		}


		/// <summary>
		/// Allows JavaScript to log
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void JSConsoleLog(object sender, JSCallbackEventArgs e)
		{
			Console.WriteLine(e.Arguments[0].ToString());
		}


		private void WebView_JSConsoleMessageAdded(object sender, JSConsoleMessageEventArgs e)
		{
			// JavaScript Error! Fail
			Console.WriteLine("Awesomium JS Error: {0}, {1} on line {2}", e.Message, e.Source, e.LineNumber);
#if DEBUG
			Debugger.Break();
#endif
		}

		#endregion

	}
}