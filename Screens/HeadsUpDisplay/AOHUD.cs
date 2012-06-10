using System;
using System.Collections.Generic;
using System.Reflection;
using AsteroidOutpost.Components;
using AsteroidOutpost.Entities;
using AsteroidOutpost.Entities.Eventing;
using AsteroidOutpost.Entities.Structures;
using AsteroidOutpost.Interfaces;
using Awesomium.Core;
using C3.XNA;
using C3.XNA.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using AsteroidOutpost.Entities.Units;
using System.Diagnostics;
using Microsoft.Xna.Framework.Input;
using MouseButton = C3.XNA.MouseButton;


namespace AsteroidOutpost.Screens.HeadsUpDisplay
{
	/// <summary>
	/// The HUD is how the user interacts with the game
	/// </summary>
	public class AOHUD : DrawableGameComponent, IComponentList
	{
		
		Vector2 focusWorldPoint;
		Vector2? middleMouseGrabPoint;
		ConstructableEntity creating;				// Are they creating an entity?

		private EnhancedMouseState theMouse = new EnhancedMouseState();
		private EnhancedKeyboardState theKeyboard = new EnhancedKeyboardState();

		readonly List<Entity> selectedEntities = new List<Entity>();
		public event Action<MultiEntityEventArgs> SelectionChanged;


		private readonly World world;
		private float scaleFactor = 1.0f;			// 1.0 = no scaling, 0.5 = zoomed in, 2.0 = zoomed out
		private float desiredScaleFactor = 1.0f;

		// These are entities that are drawn in the HUD layer
		private readonly List<Component> components = new List<Component>(10);

		//private Form inGameMenu;
		private Controller localActor;
		private bool isDraggingScreen;


		private Dictionary<Keys, EventHandler> hotkeys = new Dictionary<Keys, EventHandler>();


		private SoundEffect constructionSound;


		// Local event only
		//public event EntityEventHandler StartedConstructionEvent;
		public event Action<EntityEventArgs> CancelledCreationEvent;


		/// <summary>
		/// Construct a HUD
		/// </summary>
		/// <param name="gameScreen">A reference to the game object</param>
		public AOHUD(AOGame game, World gameScreen)
			: base(game)
		{
			world = gameScreen;

			// Set up some hotkeys
			hotkeys.Add(Keys.P, btnPower_Clicked);
			hotkeys.Add(Keys.N, btnPowerNode_Clicked);
			hotkeys.Add(Keys.M, btnMiner_Clicked);
			hotkeys.Add(Keys.L, btnLaserTower_Clicked);


			// Create callbacks for Awesomium content to communicate with the hud
			game.Awesomium.WebView.CreateObject("hud");
			game.Awesomium.WebView.SetObjectCallback("hud", "OnMouseUp", OnMouseUp);
			game.Awesomium.WebView.SetObjectCallback("hud", "OnMouseDown", OnMouseDown);

			game.Awesomium.WebView.SetObjectCallback("hud", "buildSolarPower", btnPower_Clicked);
			game.Awesomium.WebView.SetObjectCallback("hud", "buildPowerNode", btnPowerNode_Clicked);
			game.Awesomium.WebView.SetObjectCallback("hud", "buildLaserMiner", btnMiner_Clicked);
			game.Awesomium.WebView.SetObjectCallback("hud", "buildLaserTower", btnLaserTower_Clicked);
		}


		/// <summary>
		/// LoadContent will be called once per game and is the place to load all of your content.
		/// </summary>
		public new void LoadContent()
		{
			//TextureDictionary.Add("HUD");
			//TextureDictionary.Add("Buttons");

			constructionSound = Game.Content.Load<SoundEffect>(@"Sound Effects\BuildStructure");

			//Form radarPanel = CreateRadarPanel(0, ScreenMan.Viewport.Height - 220);
			////createConstuctionPanel(0, (int)radarPanel.LocationAbs.Y - 220);				// Only make this when I have an actor
			//CreateSelectionInfoPanel(radarPanel.Width, ScreenMan.Viewport.Height - 150);
			//inGameMenu = CreateInGameMenu((ScreenMan.Viewport.Width / 2) - 65, (ScreenMan.Viewport.Height / 2) - 50);

			base.LoadContent();
		}


		public void AddComponent(Component component)
		{
			components.Add(component);
		}


		/// <summary>
		/// Looks up a component by ID
		/// This method is thread safe
		/// </summary>
		/// <param name="id">The ID to look up</param>
		/// <returns>The component with the given ID, or null if the entity is not found</returns>
		public Component GetComponent(int id)
		{
			// I don't think this method should ever be called locally because none of the entities here will have IDs
			Debugger.Break();
			return null;
		}

		
		//#region Create Panels

		///// <summary>
		///// Create the Radar in its own panel
		///// </summary>
		///// <param name="x"></param>
		///// <param name="y"></param>
		//private Form CreateRadarPanel(int x, int y)
		//{
		//    Form radarPanel = new Form("Radar", x, y, 200, 220);
		//    Radar radar = new Radar(world, this, 0, 0, 200, 200);
		//    radarPanel.AddControl(radar);

		//    AddControl(radarPanel);
		//    return radarPanel;
		//}



		///// <summary>
		///// Create the construction panel and related buttons
		///// </summary>
		///// <param name="x"></param>
		///// <param name="y"></param>
		//private Form CreateConstuctionPanel(int x, int y)
		//{
		//    Form constructionPanel = new Form("Construction Menu", x, y, 150, 150);
		//    Button btnPower = new Button("Power", 5, 5, 140, 20);
		//    Button btnPowerNode = new Button("Power Node", 5, 35, 140, 20);
		//    Button btnMiner = new Button("Miner", 5, 65, 140, 20);
		//    Button btnLaserTower = new Button("Laser Tower", 5, 95, 140, 20);

		//    // Attach the button handlers
		//    btnPower.Click += btnPower_Clicked;
		//    btnPowerNode.Click += btnPowerNode_Clicked;
		//    btnMiner.Click += btnMiner_Clicked;
		//    btnLaserTower.Click += btnLaserTower_Clicked;

		//    // Set up some hotkeys
		//    hotkeys.Add(Keys.P, btnPower_Clicked);
		//    hotkeys.Add(Keys.N, btnPowerNode_Clicked);
		//    hotkeys.Add(Keys.M, btnMiner_Clicked);
		//    hotkeys.Add(Keys.L, btnLaserTower_Clicked);

		//    // Add the buttons to the construction panel
		//    constructionPanel.AddControl(btnPower);
		//    constructionPanel.AddControl(btnPowerNode);
		//    constructionPanel.AddControl(btnMiner);
		//    constructionPanel.AddControl(btnLaserTower);

		//    AddControl(constructionPanel);
		//    return constructionPanel;
		//}


		///// <summary>
		///// Create the selection info in its own panel
		///// </summary>
		///// <param name="x"></param>
		///// <param name="y"></param>
		///// <returns></returns>
		//private Form CreateSelectionInfoPanel(int x, int y)
		//{
		//    Form selectionPanel = new Form("Selection Info", x, y, 700, 150);
		//    SelectionInfo selectionInfo = new SelectionInfo(world, 5, 5, 690, 120, this, selectedEntities);
		//    selectionPanel.AddControl(selectionInfo);

		//    AddControl(selectionPanel);
		//    return selectionPanel;
		//}


		///// <summary>
		///// Create the in-game menu and related buttons
		///// </summary>
		///// <param name="x"></param>
		///// <param name="y"></param>
		///// <returns></returns>
		//private Form CreateInGameMenu(int x, int y)
		//{
		//    Form gameMenu = new Form("In-Game Menu", x, y, 130, 80);
		//    Button btnExitGame = new Button("Exit Game", 5, 5, 120, 20);
		//    Button btnCloseInGameMenu = new Button("Cancel", 5, 35, 120, 20);

		//    btnExitGame.Click += btnExitGame_Clicked;
		//    btnCloseInGameMenu.Click += btnCloseInGameMenu_Clicked;

		//    gameMenu.Visible = false;
		//    gameMenu.AddControl(btnExitGame);
		//    gameMenu.AddControl(btnCloseInGameMenu);

		//    AddControl(gameMenu);
		//    return gameMenu;
		//}

		//#endregion


		void btnCloseInGameMenu_Clicked(object sender, EventArgs e)
		{
			//inGameMenu.Visible = false;
			world.Paused = false;
		}


		void btnExitGame_Clicked(object sender, EventArgs e)
		{
			//exitRequested = true;
			//ScreenMan.Exit();
		}

		public override void Update(GameTime gameTime)
		{
			TimeSpan deltaTime = gameTime.ElapsedGameTime;

			theMouse.UpdateState();
			theKeyboard.UpdateState();

			if (theMouse.ScrollWheelDelta != 0)
			{
				ScaleFactor = desiredScaleFactor + (theMouse.ScrollWheelDelta / 120.0f) * (scaleFactor * -0.2f);
			}
			else if (scaleFactor != desiredScaleFactor)
			{
				// Smooth the zooming in and out
				float absDiff = Math.Abs(scaleFactor - desiredScaleFactor);
				float scaleDelta = absDiff * 5.0f * (float)deltaTime.TotalSeconds;

				Vector2 zoomPointWorldBefore = ScreenToWorld(theMouse.X, theMouse.Y);

				if (absDiff < (0.001 * desiredScaleFactor) ||
					(scaleFactor > desiredScaleFactor && (scaleFactor - scaleDelta) < desiredScaleFactor) ||
					(scaleFactor < desiredScaleFactor && (scaleFactor + scaleDelta) > desiredScaleFactor))
				{
					scaleFactor = desiredScaleFactor;
				}
				else if(scaleFactor > desiredScaleFactor)
				{
					scaleFactor -= scaleDelta;
				}
				else
				{
					scaleFactor += scaleDelta;
				}

				// Make your mouse focus on the same point before and after the zoom for some awesome (yet trippy) zooming
				Vector2 zoomPointWorldAfter = ScreenToWorld(theMouse.X, theMouse.Y);
				focusWorldPoint -= zoomPointWorldAfter - zoomPointWorldBefore;

			}


			// Take a screenshot
			//if (theKeyboard[Keys.F12] == EnhancedKeyState.JUST_RELEASED)
			//{
			//    ScreenMan.TakeScreenshot();
			//}


			if (theKeyboard[Keys.Escape] == EnhancedKeyState.JUST_PRESSED)
			{
				if (creating != null)
				{
					OnCancelCreation();
				}
				else
				{
					// Pause/Unpause the game
					world.Paused = !world.Paused;
					//inGameMenu.Visible = !inGameMenu.Visible;
					//GiveFocus(inGameMenu);
				}
			}


			// Handle the hotkeys
			foreach(Keys pressed in theKeyboard.GetJustPressedKeys())
			{
				if(hotkeys.ContainsKey(pressed))
				{
					hotkeys[pressed](this, EventArgs.Empty);
				}
			}


			if ((theKeyboard[Keys.LeftControl] == EnhancedKeyState.PRESSED || theKeyboard[Keys.RightControl] == EnhancedKeyState.PRESSED) &&
				(theKeyboard[Keys.LeftShift] == EnhancedKeyState.RELEASED && theKeyboard[Keys.RightShift] == EnhancedKeyState.RELEASED &&
				 theKeyboard[Keys.LeftAlt] == EnhancedKeyState.RELEASED && theKeyboard[Keys.RightAlt] == EnhancedKeyState.RELEASED)
				&& theKeyboard[Keys.Q] == EnhancedKeyState.JUST_PRESSED)
			{
				world.DrawQuadTree = !world.DrawQuadTree;
			}


			// Make a new bad guy when a key is pressed for debugging
			if (theKeyboard[Keys.F8] == EnhancedKeyState.JUST_RELEASED)
			{
				Controller aiActor = null;
				foreach (Controller actor in world.Controllers)
				{
					if (actor.Role == ControllerRole.AI)
					{
						aiActor = actor;
						break;
					}
				}
				
				Debug.Assert(aiActor != null, "There is no AI actor in the game");
				// Allow them to ignore the assert without crashing the game
				// ReSharper disable ConditionIsAlwaysTrueOrFalse
				if (aiActor != null)
				// ReSharper restore ConditionIsAlwaysTrueOrFalse
				{
					//world.AddComponent(new Ship1(aiActor.PrimaryForce, new Vector2(world.MapWidth / 2.0f, world.MapHeight / 2.0f) + new Vector2(1600, -10600)));
					world.Add(new Ship1(world, world, aiActor.PrimaryForce, new Vector2(world.MapWidth / 2.0f, world.MapHeight / 2.0f) + new Vector2(600, -600)));
				}
			}


			// Move the current creating
			if(creating != null)
			{
				// Update the creating entity to be be where the mouse is
				creating.Position.Center = ScreenToWorld(theMouse.X, theMouse.Y);
			}
			
			
			// Move the screen
			if (isDraggingScreen && theMouse.MiddleButton == EnhancedButtonState.PRESSED)
			{
				// Let them grab  the screen with the middle mouse button
				if(middleMouseGrabPoint == null)
				{
					// Store the map location of the grab
					//middleMouseGrabPoint = ScreenToWorld(mouse.X, mouse.Y);
				}
				else
				{
					// Move the focus screen such that the mouse will be above their grab point
					Vector2 diff = Vector2.Subtract(middleMouseGrabPoint.Value, ScreenToWorld(theMouse.X, theMouse.Y));
					focusWorldPoint = Vector2.Add(diff, focusWorldPoint);
				}
			}
			else
			{
				//middleMouseGrabPoint = null;
				
				
				// Move the screen if they move the mouse to the edge, or press the arrow keys
				double screenMovementRate = 0.450 * scaleFactor;			// * 1000 = pixels/second
				if ((theMouse.X >= 0 && theMouse.X < 15) || theKeyboard.IsKeyDown(Keys.Left))
				{
					focusWorldPoint.X -= (float)(screenMovementRate * deltaTime.TotalMilliseconds);
				}
				else if ((theMouse.X > Game.GraphicsDevice.Viewport.Width - 15 && theMouse.X <= Game.GraphicsDevice.Viewport.Width) || theKeyboard.IsKeyDown(Keys.Right))
				{
					focusWorldPoint.X += (float)(screenMovementRate * deltaTime.TotalMilliseconds);
				}
				if ((theMouse.Y >= 0 && theMouse.Y < 15) || theKeyboard.IsKeyDown(Keys.Up))
				{
					focusWorldPoint.Y -= (float)(screenMovementRate * deltaTime.TotalMilliseconds);
				}
				else if ((theMouse.Y > Game.GraphicsDevice.Viewport.Height - 15 && theMouse.Y <= Game.GraphicsDevice.Viewport.Height) || theKeyboard.IsKeyDown(Keys.Down))
				{
					focusWorldPoint.Y += (float)(screenMovementRate * deltaTime.TotalMilliseconds);
				}
			}


			if (!world.Paused)
			{
				// Update the entities
				List<Component> deleteList = new List<Component>();
				// TODO: This should not require a regular for loop, and I should not be modifying the component list during this loop
				for (int index = 0; index < components.Count; index++)
				{
					var component = components[index];
					if (!component.IsDead())
					{
						component.Update(deltaTime);
					}
					if (component.IsDead())
					{
						deleteList.Add(component);
					}
				}

				// Delete any entities that need to be deleted
				foreach (var entity in deleteList)
				{
					components.Remove(entity);
				}
			}
		}


		private void OnCancelCreation()
		{
			if (CancelledCreationEvent != null)
			{
				CancelledCreationEvent(new EntityEventArgs(creating));
			}
			
			// Cancel whatever they are creating
			creating = null;
		}


		/// <summary>
		/// Draw the back of the selection circle around each of the selected entities
		/// </summary>
		/// <param name="spriteBatch">The sprite batch to use</param>
		/// <param name="tint">The color to tint this</param>
		private void DrawSelectionCirclesBack(SpriteBatch spriteBatch, Color tint)
		{
			foreach (Entity selectedEntity in selectedEntities)
			{
				if (scaleFactor < 2)
				{
					float sizeRatio = ((selectedEntity.Radius.Value) / 45f);
					spriteBatch.Draw(TextureDictionary.Get("ellipse50back"),
					                 world.WorldToScreen(selectedEntity.Position.Center - (new Vector2(60, 60) * sizeRatio)),
					                 null,
					                 ColorPalette.ApplyTint(Color.Green, tint),
					                 0,
					                 Vector2.Zero,
					                 sizeRatio / world.ScaleFactor,
					                 SpriteEffects.None,
					                 0);
				}
				else if(scaleFactor < 4)
				{
					float sizeRatio = ((selectedEntity.Radius.Value) / 45f) * 2;
					spriteBatch.Draw(TextureDictionary.Get("ellipse25back"),
					                 world.WorldToScreen(selectedEntity.Position.Center - (new Vector2(35, 35) * sizeRatio)),
					                 null,
					                 ColorPalette.ApplyTint(Color.Green, tint),
					                 0,
					                 Vector2.Zero,
					                 sizeRatio / world.ScaleFactor,
					                 SpriteEffects.None,
					                 0);
				}
				else
				{
				}
			}
		}


		/// <summary>
		/// Draw the front of the selection circle around each of the selected entities
		/// </summary>
		/// <param name="spriteBatch">The sprite batch to use</param>
		/// <param name="tint">The color to tint this</param>
		private void DrawSelectionCirclesFront(SpriteBatch spriteBatch, Color tint)
		{
			foreach (Entity selectedEntity in selectedEntities)
			{
				if (scaleFactor < 2)
				{
					float sizeRatio = ((selectedEntity.Radius.Value) / 45f);
					spriteBatch.Draw(TextureDictionary.Get("ellipse50front"),
					                 world.WorldToScreen(selectedEntity.Position.Center - (new Vector2(60, 60) * sizeRatio)),
					                 null,
					                 ColorPalette.ApplyTint(Color.Green, tint),
					                 0,
					                 Vector2.Zero,
					                 sizeRatio / world.ScaleFactor,
					                 SpriteEffects.None,
					                 0);
				}
				else if(scaleFactor < 4)
				{
					float sizeRatio = ((selectedEntity.Radius.Value) / 45f) * 2;
					spriteBatch.Draw(TextureDictionary.Get("ellipse25front"),
					                 world.WorldToScreen(selectedEntity.Position.Center - (new Vector2(35, 35) * sizeRatio)),
					                 null,
					                 ColorPalette.ApplyTint(Color.Green, tint),
					                 0,
					                 Vector2.Zero,
					                 sizeRatio / world.ScaleFactor,
					                 SpriteEffects.None,
					                 0);
				}
				else
				{
					float sizeRatio = ((selectedEntity.Radius.Value) / 45f) * 2;
					spriteBatch.Draw(TextureDictionary.Get("ellipse25bold"),
					                 world.WorldToScreen(selectedEntity.Position.Center - (new Vector2(35, 35) * sizeRatio)),
					                 null,
					                 ColorPalette.ApplyTint(Color.Green, tint),
					                 0,
					                 Vector2.Zero,
					                 sizeRatio / world.ScaleFactor,
					                 SpriteEffects.None,
					                 0);
				}
			}
		}



		/// <summary>
		/// Draw the back of the HUD
		/// </summary>
		/// <param name="spriteBatch">The sprite batch to use</param>
		/// <param name="tint">The color to tint this</param>
		public void DrawBack(SpriteBatch spriteBatch, Color tint)
		{
			DrawSelectionCirclesBack(spriteBatch, tint);
		}
		
		
		/// <summary>
		/// Draw the front of the HUD
		/// </summary>
		/// <param name="spriteBatch">The sprite batch to use</param>
		/// <param name="tint">The color to tint this</param>
		public void DrawFront(SpriteBatch spriteBatch, Color tint)
		{

			foreach (var entity in components)
			{
				entity.Draw(spriteBatch, 1, tint);
			}

			DrawSelectionCirclesFront(spriteBatch, tint);

			if(creating != null)
			{
				// Draw with a red tint if it's an invalid spot to build
				if (creating.IsValidToBuildHere())
				{
					creating.Draw(spriteBatch, 1, tint);
				}
				else
				{
					creating.Draw(spriteBatch, 1, ColorPalette.ApplyTint(new Color(255, 50, 50, 255), tint));
				}
			}

			if (LocalActor != null)
			{
				// TODO: Add these to some kind of control
				spriteBatch.DrawString(Fonts.ControlFont, "Minerals:", new Vector2(10, 10), ColorPalette.ApplyTint(Color.White, tint), 0.0f, new Vector2(0, 0), 1f, SpriteEffects.None, 0.0f);
				spriteBatch.DrawString(Fonts.ControlFont, "" + (int)(LocalActor.PrimaryForce.GetMinerals() + 0.5), new Vector2(90, 10), ColorPalette.ApplyTint(Color.Gray, tint), 0.0f, new Vector2(0, 0), 1f, SpriteEffects.None, 0.0f);
			}


			// If we are paused, draw a big "PAUSED" on the screen
			if (world.Paused)
			{
				spriteBatch.DrawString(Fonts.ControlFont, "** PAUSED **", new Vector2((Game.GraphicsDevice.Viewport.Width / 2.0f) - 75, Game.GraphicsDevice.Viewport.Height / 5.0f), Color.White);
			}


			//base.Draw(spriteBatch, tint);
			
			
//#if DEBUG
//            // Draw the current frame rate
//            string str = String.Format("{0} FPS", CurrentFrameRate);
//            spriteBatch.DrawString(Fonts.ControlFont, str, new Vector2(200, 10), ColorPalette.ApplyTint(Color.White, tint), 0, Vector2.Zero, 1f, SpriteEffects.None, 0);
//#endif
			
			// Draw the mouse last (so that it shows up on top)
			spriteBatch.Draw(TextureDictionary.Get("Cursor"), new Vector2(theMouse.X - 20, theMouse.Y - 20), Color.White);
		}
		
		
		public Vector2 ScreenToWorld(Vector2 point)
		{
			return ScreenToWorld(point.X, point.Y);
		}
		public Vector2 ScreenToWorld(float x, float y)
		{
			float deltaX = x - (Game.GraphicsDevice.Viewport.Width / 2f);
			float deltaY = y - (Game.GraphicsDevice.Viewport.Height / 2f);

			deltaX = deltaX * scaleFactor / (float)Math.Sqrt(3);
			deltaY = deltaY * scaleFactor;

			return new Vector2(focusWorldPoint.X + deltaX, focusWorldPoint.Y + deltaY);
		}
		
		
		public Vector2 WorldToScreen(Vector2 point)
		{
			return WorldToScreen(point.X, point.Y);
		}
		public Vector2 WorldToScreen(float x, float y)
		{
			float deltaX = x - focusWorldPoint.X;
			float deltaY = y - focusWorldPoint.Y;

			deltaX = deltaX / scaleFactor * (float)Math.Sqrt(3);
			deltaY = deltaY / scaleFactor;

			return new Vector2(Game.GraphicsDevice.Viewport.Width / 2f + deltaX, Game.GraphicsDevice.Viewport.Height / 2f + deltaY);
		}
		
		
		
		public Rectangle FocusScreen
		{
			get
			{
				Vector2 topLeft = ScreenToWorld(0, 0);
				Vector2 bottomRight = ScreenToWorld(Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height);
				
				return new Rectangle((int)(topLeft.X + 0.5),
				                     (int)(topLeft.Y + 0.5),
				                     (int)(bottomRight.X - topLeft.X + 0.5),
				                     (int)(bottomRight.Y - topLeft.Y + 0.5));
			}
		}

		public Vector2 FocusWorldPoint
		{
			get
			{
				return focusWorldPoint;
			}
			set
			{
				focusWorldPoint = value;
			}
		}

		
		public Controller LocalActor
		{
			get { return localActor; }
			set
			{
				if(value != null)
				{
					if(localActor == null)
					{
						//CreateConstuctionPanel(0, size.Height - 370);
					}
					localActor = value;
				}
				else
				{
					// TODO: Delete the construction panel
				}
			}
		}

		public float ScaleFactor
		{
			get
			{
				return scaleFactor;
			}
			set
			{
				if(value <= 0.5f)
				{
					//scaleFactor = 0.5f;
					desiredScaleFactor = 0.5f;
				}
				else if (value >= 7.0f)
				{
					//scaleFactor = 7.0f;
					desiredScaleFactor = 7.0f;
				}
				else
				{
					//scaleFactor = value;
					desiredScaleFactor = value;
				}
			}
		}

		public float Scale(float value)
		{
			return value / scaleFactor;
		}

		public Vector2 Scale(Vector2 value)
		{
			return value / scaleFactor;
		}

		#region Button Handlers

		private void btnPower_Clicked(object sender, EventArgs e)
		{
			if (!world.Paused)
			{
				if(creating != null)
				{
					OnCancelCreation();
				}

				// Create a new power station
				creating = new SolarStation(world, this, LocalActor.PrimaryForce, ScreenToWorld(new Vector2(theMouse.X, theMouse.Y)));

				CreateRangeRingsForConstruction(creating);
				CreatePowerLinker(creating);
			}
		}


		private void btnPowerNode_Clicked(object sender, EventArgs e)
		{
			if (!world.Paused)
			{
				if (creating != null)
				{
					OnCancelCreation();
				}

				// Create a new power node
				creating = new PowerNode(world, this, LocalActor.PrimaryForce, ScreenToWorld(new Vector2(theMouse.X, theMouse.Y)));

				CreateRangeRingsForConstruction(creating);
				CreatePowerLinker(creating);
			}
		}
		
		private void btnMiner_Clicked(object sender, EventArgs e)
		{
			if (!world.Paused)
			{
				if (creating != null)
				{
					OnCancelCreation();
				}

				// Create a new power station
				creating = new LaserMiner(world, this, LocalActor.PrimaryForce, ScreenToWorld(new Vector2(theMouse.X, theMouse.Y)));

				CreateRangeRingsForConstruction(creating);
				CreatePowerLinker(creating);

				LaserMiner laserMiner = creating as LaserMiner;
				Linker linker = new Linker(world, this, creating.Position);
				linker.Links.Add(new Tuple<Predicate<Entity>, Color, float>(entity => entity is Asteroid, Color.Green, laserMiner.MiningRange));

				CancelledCreationEvent += linker.KillSelf;
				//world.StructureStartedEventPreAuth += linker.KillSelf;
				components.Add(linker);
			}
		}

		void btnLaserTower_Clicked(object sender, EventArgs e)
		{
			if (!world.Paused)
			{
				if (creating != null)
				{
					OnCancelCreation();
				}

				// Create a new power station
				creating = new LaserTower(world, this, LocalActor.PrimaryForce, ScreenToWorld(new Vector2(theMouse.X, theMouse.Y)));

				CreateRangeRingsForConstruction(creating);
				CreatePowerLinker(creating);
			}
		}


		private List<ICanKillSelf> CreateRangeRings(ConstructableEntity entity)
		{
			var createdSuicidals = new List<ICanKillSelf>(6);
			var rangeRingDefinitions = new List<Tuple<int, Color, string>>(10);
			entity.GetRangeRings(ref rangeRingDefinitions);

			foreach (var rangeRingDefinition in rangeRingDefinitions)
			{
				Ring ring = new Ring(world,
				                     this,
				                     entity.Position,
				                     rangeRingDefinition.Item1,
				                     rangeRingDefinition.Item2);
				components.Add(ring);
				createdSuicidals.Add(ring);

				PositionOffset positionOffset = new PositionOffset(world, this, entity.Position, new Vector2(-25, -rangeRingDefinition.Item1 - 17));
				FreeText text = new FreeText(world,
				                             this,
				                             positionOffset,
				                             rangeRingDefinition.Item3,
				                             rangeRingDefinition.Item2);
				components.Add(positionOffset);
				components.Add(text);
				createdSuicidals.Add(text);
			}

			return createdSuicidals;
		}


		private void CreateRangeRingsForConstruction(ConstructableEntity entity)
		{
			foreach (var suicidal in CreateRangeRings(entity))
			{
				// TODO: I think these events will continue to hold on to the dying entity long after its dead and it will prevent garbage collection
				CancelledCreationEvent += suicidal.KillSelf;
				//world.StructureStartedEventPreAuth += suicidal.KillSelf;
			}
		}


		private void CreateRangeRingsForSelection(ConstructableEntity entity)
		{
			foreach (var suicidal in CreateRangeRings(entity))
			{
				// TODO: I think these events will continue to hold on to the dying entity long after its dead and it will prevent garbage collection
				SelectionChanged += suicidal.KillSelf;
			}
		}


		private void CreatePowerLinker(ConstructableEntity entity)
		{
			PowerLinker powerLinker = new PowerLinker(world, this, localActor.PrimaryForce, entity);
			CancelledCreationEvent += powerLinker.KillSelf;
			//world.StructureStartedEventPreAuth += powerLinker.KillSelf;
			components.Add(powerLinker);
		}

		#endregion



		protected void OnMouseDown(object sender, JSCallbackEventArgs e)
		{
			MouseButton mouseButton = (MouseButton)e.Arguments[1].ToInteger();
			// TODO: Start a multi-select, BUT only actually do something about it after they move... lets say 10px away from this location
			if (mouseButton == MouseButton.MIDDLE)
			{
				isDraggingScreen = true;
				middleMouseGrabPoint = ScreenToWorld(theMouse.X, theMouse.Y);
			}
		}


		/// <summary>
		/// Handle mouse up everywhere except the controls
		/// </summary>
		protected void OnMouseUp(object sender, JSCallbackEventArgs e)
		{
			bool mouseUpOverHUD = e.Arguments[0].ToBoolean();
			MouseButton mouseButton = (MouseButton)e.Arguments[1].ToInteger();
			bool clickHandled = mouseUpOverHUD;

			if (mouseButton == MouseButton.MIDDLE)
			{
				middleMouseGrabPoint = null;
				isDraggingScreen = false;
			}

			if (mouseButton == MouseButton.LEFT)
			{

				if (!clickHandled && creating != null)
				{
					// Have we just tried to build this guy?
					if (/*mouse.LeftButton == EnhancedButtonState.JUST_RELEASED &&*/ creating.IsValidToBuildHere())
					{
						// Yes, build this now!
						BuildStructure();
						clickHandled = true;
					}
				}

				if (!clickHandled)
				{
					// Did we click on a unit?
					Vector2 mouseMapCoords = ScreenToWorld(theMouse.X, theMouse.Y);

					// Grab a possible list of clicked entities by using a square-area search
					List<Entity> possiblyClickedEntities = world.EntitiesInArea(new Rectangle((int)(mouseMapCoords.X + 0.5), (int)(mouseMapCoords.Y + 0.5), 1, 1));
					foreach (Entity entity in possiblyClickedEntities)
					{
						// Make sure the unit was clicked
						if (entity.Radius.IsIntersecting(mouseMapCoords, 1))
						{
							selectedEntities.Clear();
							selectedEntities.Add(entity);
							entity.DyingEvent += SelectedEntityDying;

							OnSelectionChanged();

							clickHandled = true;
						}
					}
				}

				if (!clickHandled)
				{
					if (selectedEntities.Count > 0)
					{
						// Deselect the selected unit(s)

						foreach (Entity entity in selectedEntities)
						{
							entity.DyingEvent -= SelectedEntityDying;
						}

						selectedEntities.Clear();

						OnSelectionChanged();
					}
				}
			}
		}


		private void BuildStructure()
		{
			constructionSound.Play(Math.Min(1, world.Scale(1f)), 0, 0);

			// Reflectively look up what they are making, and create an other one of the same thing in the game
			Type creatingType = creating.GetType();
			ConstructorInfo creatingTypeConstuctructor = creatingType.GetConstructor(new Type[] { typeof(World), typeof(IComponentList), typeof(Force), typeof(Vector2) });

			if (creatingTypeConstuctructor == null)
			{
				System.Console.WriteLine("Failed to find a constructor for the current constructable! Unable to construct entity");
				Debugger.Break(); // John, there's a problem with the reflection above. Fix it!
				return;
			}

			ConstructableEntity toBuild = (ConstructableEntity)creatingTypeConstuctructor.Invoke(new object[]{world, world, LocalActor.PrimaryForce, ScreenToWorld(new Vector2(theMouse.X, theMouse.Y))});

			toBuild.StartConstruction();
			world.Add(toBuild);


			// TODO: Find a better place to put this, but I don't think the LaserMiner should know about this
			// Maybe add an accumulator definition retriever to the entity? Just like the rings?
			LaserMiner laserMiner = toBuild as LaserMiner;
			if (laserMiner != null)
			{
				PositionOffset miningAccumPosition = new PositionOffset(world,
				                                                        this,
				                                                        laserMiner.Position,
				                                                        new Vector2(-5, -20));
				Accumulator miningAccumulator = new Accumulator(world,
				                                                this,
				                                                miningAccumPosition,
				                                                new Color(100, 255, 100, 255),
				                                                450,
				                                                new Vector2(0, -15),
				                                                120);
				laserMiner.AccumulationEvent += miningAccumulator.Accumulate;
				AddComponent(miningAccumPosition);
				AddComponent(miningAccumulator);
			}

			PositionOffset healthAccumPosisiton = new PositionOffset(world,
				                                                        this,
				                                                        toBuild.Position,
				                                                        new Vector2(-5, -20));
			Accumulator healthAccumulator = new Accumulator(world,
				                                            this,
				                                            healthAccumPosisiton,
				                                            new Color(200, 50, 50, 255),
				                                            450,
				                                            new Vector2(0, -15),
				                                            120);
			toBuild.HitPoints.HitPointsChangedEvent += healthAccumulator.Accumulate;
			AddComponent(healthAccumPosisiton);
			AddComponent(healthAccumulator);

			ProgressBar progressBar = new ProgressBar(world, this, toBuild.Position, new Vector2(0, toBuild.Radius.Value - 6), toBuild.Radius.Value * 2, 6, Color.Gray, Color.RoyalBlue);
			progressBar.Max = toBuild.MineralsToConstruct;
			toBuild.ConstructionProgressChangedEvent += progressBar.SetProgress;
			toBuild.ConstructionCompletedEvent += progressBar.KillSelf;
			toBuild.HitPoints.DyingEvent += progressBar.KillSelf;
			AddComponent(progressBar);

			ProgressBar healthBar = new ProgressBar(world, this, toBuild.Position, new Vector2(0, toBuild.Radius.Value), toBuild.Radius.Value * 2, 6, Color.Gray, Color.Green);
			healthBar.Max = toBuild.HitPoints.GetTotal();
			healthBar.Progress = healthBar.Max;
			toBuild.HitPoints.HitPointsChangedEvent += healthBar.SetProgress;
			toBuild.HitPoints.DyingEvent += healthBar.KillSelf;
			AddComponent(healthBar);


			if (!theKeyboard.IsKeyDown(Keys.LeftShift) && !theKeyboard.IsKeyDown(Keys.RightShift))
			{
				OnCancelCreation();
			}
		}


		protected void OnSelectionChanged()
		{
			if (SelectionChanged != null)
			{
				SelectionChanged(new MultiEntityEventArgs(selectedEntities));
			}

			foreach (var selectedEntity in selectedEntities)
			{
				ConstructableEntity constructableEntity = selectedEntity as ConstructableEntity;
				if (constructableEntity != null)
				{
					CreateRangeRingsForSelection(constructableEntity);
				}
			}
		}


		private void SelectedEntityDying(EntityReflectiveEventArgs e)
		{
			// Remove any deleted entities from the selection list
			Entity dyingEntity = e.Entity;
			if (dyingEntity != null)
			{
				selectedEntities.Remove(dyingEntity);
				dyingEntity.DyingEvent -= SelectedEntityDying;

				// Tell anyone who is interested in a selection change
				OnSelectionChanged();
			}
		}
	}
}