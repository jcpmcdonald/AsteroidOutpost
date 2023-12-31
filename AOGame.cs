using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using AsteroidOutpost.Entities;
using AsteroidOutpost.Extensions;
using AsteroidOutpost.Scenarios;
using AsteroidOutpost.Scenes;
using AsteroidOutpost.Screens;
using Awesomium.Core;
using AwesomiumXNA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using ProjectMercury;
using ProjectMercury.Renderers;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using MediaState = Microsoft.Xna.Framework.Media.MediaState;

namespace AsteroidOutpost
{
	using System.Globalization;

	/// <summary>
	/// This is the entry point for the game and is responsible for creating the menu and the world
	/// </summary>
	public class AOGame : Game
	{
		#region user32 SetWindowPos

		[DllImport("user32.dll", EntryPoint = "SetWindowPos")]
		public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int cx, int cy, int wFlags);


		[DllImport("user32.dll", EntryPoint = "ClipCursor")]
		private static extern void ClipCursor(ref Rectangle rect);


		private const short SWP_NOMOVE = 0X2;
		private const short SWP_NOSIZE = 1;
		private const short SWP_NOZORDER = 0X4;
		private const int SWP_SHOWWINDOW = 0x0040;

		#endregion


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
		private  ParticleEffectManager particleEffectManager;

		private List<Song> music = new List<Song>();
		private int currentTrack;
		private bool changingTrack = false;
		private bool musicStarted = false;

		private Dictionary<String, SoundEffect> soundEffects = new Dictionary<string, SoundEffect>();

		public SceneManager sceneManager;
		private World world;
		private EntityFactory entityFactory;
		private UpgradeFactory upgradeFactory;
		private Texture2D cursorTexture;

		private Stopwatch stopwatch = new Stopwatch();
		private bool destroyWorld = false;

		private List<String> executeAwesomiumJSNextCycle = new List<string>();

		private Vector2? previousMousePos = null;

		public Profile ActiveProfile { get; private set; }

		
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

		public ParticleEffectManager ParticleEffectManager
		{
			get
			{
				return particleEffectManager;
			}
		}


		private bool displayPerformanceGraph;
		private bool executingNextCycleJS;

		public bool DisplayPerformanceGraph
		{
			get
			{
				return displayPerformanceGraph;
			}
			set
			{
				displayPerformanceGraph = value;
			}
		}


		#region Construct

		public AOGame(int width, int height, bool fullScreen, bool performanceGraph)
		{
			displayPerformanceGraph = performanceGraph;
			Init(width, height, fullScreen);
		}


		public AOGame(int moveToX, int moveToY, int width, int height, bool performanceGraph)
		{
			displayPerformanceGraph = performanceGraph;
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
			//starField = new LayeredStarField(this, "Snow&Ice");
			//starField = new Scene(this, "Sufista");
			sceneManager = new SceneManager(this);
			Components.Add(sceneManager);
			entityFactory = new EntityFactory();
			upgradeFactory = new UpgradeFactory();

			frameRateCounter = new FrameRateCounter(this);
			//Components.Add(frameRateCounter);

			try
			{
				initGraphicsMode(width, height, fullScreen);
			}
			catch(NoSuitableGraphicsDeviceException ex)
			{
				try
				{
					// Try again at a lower resolution
					initGraphicsMode(800, 600, fullScreen);

					// This will only get logged IF it succeeds, otherwise, nobody will ever know. Except you.
					Console.WriteLine("Your resolution was reduced to 800x600 because {0}x{1} will not display on your machine. Use -w and -h to specify a resolution on the command line", width, height);
				}
				catch(NoSuitableGraphicsDeviceException)
				{
					// Don't throw the 800x600 exception, throw the original exception.
					throw ex;
				}
			}



			//#if UNLIMITED_FPS
#if false
			graphics.SynchronizeWithVerticalRetrace = false;
			IsFixedTimeStep = false;
#endif

			// Create our web front end
			//WebCoreConfig.Default.ForceSingleProcess = true;
			awesomium = new AwesomiumComponent(this, GraphicsDevice.Viewport.Bounds);
			awesomium.WebView.ParentWindow = Window.Handle;


			// A document must be loaded in order for me to make a global JS object, but the presence of
			// the global JS object affects the first page to be loaded, so give it an egg:
			awesomium.WebView.DocumentReady += WebView_DocumentReady;
			awesomium.WebView.LoadHTML("<html><head><title>Loading...</title></head><body>Loading...</body></html>");
			

			Components.Add(awesomium);

			//awesomium.WebView.CreateObject("performanceMonitor");
			//JSValue[] updateTime = new JSValue[2];
			//updateTime[0] = new JSValue(5);
			//updateTime[1] = new JSValue(6);
			//awesomium.WebView.SetObjectProperty("performanceMonitor", "updateTime", new JSValue(updateTime));

			awesomium.WebView.ConsoleMessage += WebView_JSConsoleMessageAdded;
		}

		void WebView_DocumentReady(object sender, UrlEventArgs e)
		{
			// Call this only once
			awesomium.WebView.DocumentReady -= WebView_DocumentReady;

			// Create callbacks for Awesomium content to communicate with the game
			JSObject jsXNA = awesomium.WebView.CreateGlobalJavascriptObject("xna");
			jsXNA.Bind("StartWorld", false, StartWorld);
			jsXNA.Bind("PopulateScenarioList", false, PopulateScenarioList);
			jsXNA.Bind("Save", false, Save);
			jsXNA.Bind("Exit", false, Exit);
			jsXNA.Bind("PlaySound", false, PlaySound);

			// Create somewhere to log messages to
			JSObject jsConsole = awesomium.WebView.CreateGlobalJavascriptObject("console");
			jsConsole.Bind("log", false, JSConsoleLog);
			jsConsole.Bind("dir", false, JSConsoleLog);

			//awesomium.WebView.Source = @"..\UI\MainMenu.html".ToUri();
			awesomium.WebView.Source = (Environment.CurrentDirectory +  @"\..\data\HUD\MainMenu.html").ToUri();
		}


		private void Save(object sender, JavascriptMethodEventArgs e)
		{
			DirectoryInfo savePath = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\My Games\\Asteroid Outpost");
			if (!savePath.Exists)
			{
				savePath.Create();
			}


			FileInfo saveFileInfo = new FileInfo(savePath + "\\AO " + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".sav");
			var saveFile = new JsonTextWriter(new StreamWriter(new DeflateStream(saveFileInfo.Create(), CompressionMode.Compress)));

#if DEBUG
			var settings = new JsonSerializerSettings()
			{
				ContractResolver = new CustomContractResolver(),
				Formatting = Formatting.Indented
			};
#else
			var settings = new JsonSerializerSettings()
			{
				ContractResolver = new CustomContractResolver()
			};
#endif

			saveFile.WriteStartObject();
			saveFile.WritePropertyName("Profile");
			saveFile.WriteRawValue(JsonConvert.SerializeObject(ActiveProfile, settings));
			saveFile.WritePropertyName("World");
			saveFile.WriteRawValue(JsonConvert.SerializeObject(world, settings));
			saveFile.WriteEndObject();

			saveFile.Close();
		}


		private void PlaySound(object sender, JavascriptMethodEventArgs javascriptMethodEventArgs)
		{
			String soundName = javascriptMethodEventArgs.Arguments[0].ToString();
			PlaySound(soundName);
		}

		private void PlaySound(String soundName)
		{
			soundEffects[soundName.ToLowerInvariant()].Play();
		}


		private void PopulateScenarioList(object sender, JavascriptMethodEventArgs javascriptMethodEventArgs)
		{
			ExecuteAwesomiumJS(String.Format(CultureInfo.InvariantCulture, "createMap({0})", JsonConvert.SerializeObject(ActiveProfile.progress)));
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

			ActiveProfile = new Profile();
			spriteBatch = new SpriteBatch(GraphicsDevice);

			//sceneManager.SetScene("Sufista");
			sceneManager.SetScene("Red Planet");


			foreach (var soundFileName in Directory.EnumerateFiles(@"..\data\soundEffects\", "*.wav"))
			{
				FileInfo soundFileInfo = new FileInfo(soundFileName);
				soundEffects.Add(soundFileInfo.Name.Replace(".wav", "").ToLowerInvariant(), SoundEffect.FromStream(File.OpenRead(soundFileName)));
			}


			cursorTexture = Texture2DEx.FromStreamWithPremultAlphas(GraphicsDevice, File.OpenRead(@"..\data\images\Cursor.png"));

			//ThreadPool.QueueUserWorkItem(delegate { entityFactory.LoadContent(GraphicsDevice); upgradeFactory.LoadUpgradeTemplates(); });
			//ThreadPool.QueueUserWorkItem(delegate { EllipseEx.LoadContent(GraphicsDevice); });
			entityFactory.LoadContent(GraphicsDevice);
			upgradeFactory.LoadUpgradeTemplates();
			SpriteBatchEx.LoadContent(GraphicsDevice);

			particleEffectManager = new ParticleEffectManager();
			particleEffectManager.LoadParticles(graphics, Content);
			Services.AddService(particleEffectManager.GetType(), particleEffectManager);

			music.Add(Content.Load<Song>(@"Music\Soulfrost - You Should Have Never Trusted Hollywood EP - 04 Inner Battles (Bignic Remix)"));
			music.Add(Content.Load<Song>(@"Music\Soulfrost - You Should Have Never Trusted Hollywood EP - 01 The Plan"));
			currentTrack = 1;

			//MediaPlayer.IsRepeating = true;

			if (stopwatch.IsRunning)
			{
				stopwatch.Stop();
				Console.WriteLine("Loaded in " + stopwatch.ElapsedMilliseconds + "ms");
			}

			if(graphics.IsFullScreen)
			{
				// Clip the mouse to the window in full screen mode
				Rectangle rect = Window.ClientBounds;
				rect.Width += rect.X;
				rect.Height += rect.Y;
				ClipCursor(ref rect);
			}
		}


		/// <summary>
		/// UnloadContent will be called once per game and is the place to unload
		/// all content.
		/// </summary>
		protected override void UnloadContent()
		{
			MediaPlayer.Stop();
			while(MediaPlayer.State != MediaState.Stopped){}
			if (music != null)
			{
				foreach (var song in music)
				{
					song.Dispose();
				}
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

				// Execute anything we weren't able to before
				ExecuteNextCycleAwesomiumJS();

				if(world != null)
				{
					base.Update(new GameTime(accumulatedTime, new TimeSpan((long)(fixedDeltaTime.Ticks * world.TimeMultiplier))));
					previousMousePos = null;
				}
				else
				{
					Vector2 currentMouse = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
					Vector2 mouseDrift = Vector2.Zero;
					if(previousMousePos != null)
					{
						mouseDrift = currentMouse - previousMousePos.Value;
					}
					sceneManager.Move(mouseDrift);
					previousMousePos = currentMouse;

					base.Update(new GameTime(accumulatedTime, fixedDeltaTime));
				}

				if (!musicStarted && gameTime.TotalGameTime.TotalSeconds > 3)
				{
					if (settings.MusicVolume > 0)
					{
						MediaPlayer.Volume = settings.MusicVolume;
						MediaPlayer.Play(music[currentTrack]);

						MediaPlayer.MediaStateChanged += MediaPlayerOnMediaStateChanged;
					}
					musicStarted = true;
				}


				if (displayPerformanceGraph &&
					awesomium.WebView.IsDocumentReady &&
					!awesomium.WebView.IsLoading 
					//&& !gameTime.IsRunningSlowly  // Removed because it hides the issue(s)
					)
				{
					if(frameRateCounter.LastUpdateTime() <= 0.00001 &&
						frameRateCounter.LastDrawTime() <= 0.00001 &&
						frameRateCounter.LastDrawDelay <= 0.00001)
					{
						//Console.WriteLine("ZERO");
					}
					else
					{
						String js = String.Format(CultureInfo.InvariantCulture, "if(typeof RefreshPerformanceGraph != 'undefined'){{ RefreshPerformanceGraph({0}, [{1}, {2}, {3}]); }}", gameTime.TotalGameTime.TotalSeconds, frameRateCounter.LastUpdateTime(), frameRateCounter.LastDrawTime(), frameRateCounter.LastDrawDelay);
						awesomium.WebView.ExecuteJavascript(js);
					}
				}

				frameRateCounter.EndOfUpdate(gameTime);

				// Allows the game to exit if I ever load this on an XBox
				if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
				{
					Exit();
				}
			}
		}


		private void MediaPlayerOnMediaStateChanged(object sender, EventArgs eventArgs)
		{
			if(MediaPlayer.State == MediaState.Stopped)
			{
				if(changingTrack) { return; }
				changingTrack = true;
				currentTrack = ++currentTrack % music.Count;
				MediaPlayer.Play(music[currentTrack]);
				changingTrack = false;
			}
		}


		/// <summary>
		/// Draw the game
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			// If this is a positive number, it means that we've skipped a frame, and that's bad
			frameRateCounter.LastDrawDelay = (float)(gameTime.ElapsedGameTime.TotalMilliseconds - fixedDeltaTime.TotalMilliseconds);

			frameRateCounter.StartOfDraw(gameTime);

			//sceneManager.Draw(gameTime);

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
			awesomium.WebView.Source = (Environment.CurrentDirectory +  @"\..\data\HUD\MissionSelect.html").ToUri();
			destroyWorld = true;

			sceneManager.SetScene("Sufista");
		}


		#region JavaScript Callbacks

		private void StartWorld(object sender, JavascriptMethodEventArgs e)
		{
			//awesomium.WebView.LoadCompleted += HUD_LoadCompleted;

			String mapName = e.Arguments[0].ToString();
			if (World != null)
			{
				// The world should always be null before starting to make a new one, right?
				Debugger.Break();
			}

			world = new World(this, entityFactory, upgradeFactory);


			// TODO: do this automatically
			Scenario scenario;
			switch(mapName.ToLower())
			{
			case "mineral grab":
				scenario = new MinerealCollectionScenario();
				break;

			case "super station":
				scenario = new SuperStructureProtectScenario();
				break;

			case "tutorial":
				scenario = new TutorialScenario();
				break;

			case "endless":
				scenario = new RandomScenario();
				break;

			default:
				Debugger.Break();
				goto case "endless";
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
		private void Exit(object sender, JavascriptMethodEventArgs e)
		{
			Exit();
		}


		/// <summary>
		/// Allows JavaScript to log
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void JSConsoleLog(object sender, JavascriptMethodEventArgs e)
		{
			Console.WriteLine(e.Arguments[0].ToString());
		}


		private void WebView_JSConsoleMessageAdded(object sender, ConsoleMessageEventArgs e)
		{
			// JavaScript Error! Fail
			Console.WriteLine("Awesomium JS Error: {0}, {1} on line {2}. \nLast JS = \"{3}\"", e.Message, e.Source, e.LineNumber, LastJSExecuted);
#if DEBUG
			//Debugger.Break();
#endif
		}

		#endregion



		public String LastJSExecuted { get; private set; }


		public void ExecuteAwesomiumJS(String js)
		{
			bool loaded = awesomium.WebView.IsDocumentReady;// && !awesomium.WebView.ExecuteJavascriptWithResult("typeof scopeOf == 'undefined'");
			if (loaded)
			{
				//awesomium.WebView.ExecuteJavascriptWithResult(js, 50);
				try
				{
					if(!executingNextCycleJS)
					{
						// Execute anything we weren't able to before
						ExecuteNextCycleAwesomiumJS();
					}

					LastJSExecuted = js;
					awesomium.WebView.ExecuteJavascript(js);
				}
				catch (InvalidOperationException)
				{
					executeAwesomiumJSNextCycle.Add(js);
					Console.WriteLine("JS Executed Next Cycle");
				}
			}
			else
			{
				executeAwesomiumJSNextCycle.Add(js);
				Console.WriteLine("JS Executed Next Cycle");
			}
		}


		private void ExecuteNextCycleAwesomiumJS()
		{
			executingNextCycleJS = true;
			int count = executeAwesomiumJSNextCycle.Count;
			for (int i = 0; i < count; i++)
			{
				var js = executeAwesomiumJSNextCycle[0];
				executeAwesomiumJSNextCycle.RemoveAt(0);
				ExecuteAwesomiumJS(js);
			}
			executingNextCycleJS = false;
		}
	}
}