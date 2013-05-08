using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using AsteroidOutpost.Components;
using AsteroidOutpost.Entities;
using AsteroidOutpost.Entities.Eventing;
using AsteroidOutpost.Eventing;
using AsteroidOutpost.Interfaces;
using AsteroidOutpost.Systems;
using Awesomium.Core;
using AwesomiumXNA;
using C3.XNA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using Microsoft.Xna.Framework.Input;
using fastJSON;


namespace AsteroidOutpost.Screens.HeadsUpDisplay
{
	/// <summary>
	/// The HUD is how the user interacts with the game
	/// </summary>
	public class AOHUD : DrawableGameComponent
	{

		private SpriteBatch spriteBatch;

		Vector2 focusWorldPoint;
		Vector2? middleMouseGrabPoint;
		int? creatingEntityID;				// Are they creating an entity?

		private EnhancedMouseState theMouse = new EnhancedMouseState();
		private EnhancedKeyboardState theKeyboard = new EnhancedKeyboardState();

		private readonly List<int> selectedEntities = new List<int>();
		public event Action<MultiEntityEventArgs> SelectionChanged;


		private readonly World world;
		private readonly AwesomiumComponent awesomium;
		private float scaleFactor = 1.0f;			// 1.0 = no scaling, 0.5 = zoomed in, 2.0 = zoomed out
		private float desiredScaleFactor = 1.0f;

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
		public AOHUD(AOGame game, World world)
			: base(game)
		{
			this.world = world;

			// Set up some hotkeys
			hotkeys.Add(Keys.S, btnPower_Clicked);
			hotkeys.Add(Keys.N, btnPowerNode_Clicked);
			hotkeys.Add(Keys.M, btnLaserMiner_Clicked);
			hotkeys.Add(Keys.L, btnLaserTower_Clicked);
			hotkeys.Add(Keys.I, btnMissileTower_Clicked);


			// Create callbacks for Awesomium content to communicate with the hud
			awesomium = game.Awesomium;
			awesomium.WebView.CreateObject("hud");
			awesomium.WebView.SetObjectCallback("hud", "OnMouseUp", OnMouseUp);
			awesomium.WebView.SetObjectCallback("hud", "OnMouseDown", OnMouseDown);

			awesomium.WebView.SetObjectCallback("hud", "ResumeGame", ResumeGame);

			awesomium.WebView.SetObjectCallback("hud", "BuildSolarStation", btnPower_Clicked);
			awesomium.WebView.SetObjectCallback("hud", "BuildPowerNode", btnPowerNode_Clicked);
			awesomium.WebView.SetObjectCallback("hud", "BuildLaserMiner", btnLaserMiner_Clicked);
			awesomium.WebView.SetObjectCallback("hud", "BuildLaserTower", btnLaserTower_Clicked);
			awesomium.WebView.SetObjectCallback("hud", "BuildMissileTower", btnMissileTower_Clicked);
		}


		/// <summary>
		/// LoadContent will be called once per game and is the place to load all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			spriteBatch = new SpriteBatch(Game.GraphicsDevice);
			constructionSound = Game.Content.Load<SoundEffect>(@"Sound Effects\BuildStructure");

			base.LoadContent();
		}


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
				else if (scaleFactor > desiredScaleFactor)
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
				if (creatingEntityID != null)
				{
					OnCancelCreation();
				}
				else
				{
					// Make the world stop
					world.Paused = !world.Paused;
					awesomium.WebView.CallJavascriptFunction("", world.Paused ? "ShowGameMenu" : "HideGameMenu", new JSValue());
				}
			}


			// Handle the hotkeys
			foreach (Keys pressed in theKeyboard.GetJustPressedKeys())
			{
				if (hotkeys.ContainsKey(pressed))
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


			if ((theKeyboard[Keys.LeftControl] == EnhancedKeyState.PRESSED || theKeyboard[Keys.RightControl] == EnhancedKeyState.PRESSED) &&
			    (theKeyboard[Keys.LeftShift] == EnhancedKeyState.RELEASED && theKeyboard[Keys.RightShift] == EnhancedKeyState.RELEASED &&
			     theKeyboard[Keys.LeftAlt] == EnhancedKeyState.RELEASED && theKeyboard[Keys.RightAlt] == EnhancedKeyState.RELEASED)
			    && theKeyboard[Keys.E] == EnhancedKeyState.JUST_PRESSED)
			{
				DrawEllipseGuides = !DrawEllipseGuides;
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

				if (aiActor == null)
				{
					Console.WriteLine("There is no AI actor in the game");
				}
				else
				{
					//world.Add(new Ship1(world, world, aiActor.PrimaryForce, new Vector2(world.MapWidth / 2.0f, world.MapHeight / 2.0f) + new Vector2(600, -600)));

					EntityFactory.Create("Space Ship", new Dictionary<String, object>(){
						{ "Sprite.Scale", 0.7f },
						{ "Sprite.Set", null },
						{ "Sprite.Animation", null },
						{ "Sprite.Orientation", (float)GlobalRandom.Next(0, 359) },
						{ "Transpose.Position", ScreenToWorld(new Vector2(theMouse.X, theMouse.Y)) },
						{ "Transpose.Radius", 40 },
						{ "OwningForce", aiActor.PrimaryForce }
					});
				}
			}


			// Move the current creating
			if (creatingEntityID != null)
			{
				// Update the creating entity to be be where the mouse is
				Position position = world.GetComponent<Position>(creatingEntityID.Value);
				position.Center = ScreenToWorld(theMouse.X, theMouse.Y);
				world.QuadTree.Move(position);
			}


			// Move the screen
			if (isDraggingScreen && theMouse.MiddleButton == EnhancedButtonState.PRESSED)
			{
				// Move the focus screen such that the mouse will be above their grab point
				Vector2 diff = Vector2.Subtract(middleMouseGrabPoint.Value, ScreenToWorld(theMouse.X, theMouse.Y));
				focusWorldPoint = Vector2.Add(diff, focusWorldPoint);
			}
			else
			{
				// Move the screen if they move the mouse to the edge, or press the arrow keys
				double screenMovementRate = 0.450 * scaleFactor; // * 1000 = pixels/second
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


			if (LocalActor != null)
			{
				JSObject resources = new JSObject();
				resources["minerals"] = new JSValue((int)(LocalActor.PrimaryForce.GetMinerals() + 0.5));
				awesomium.WebView.CallJavascriptFunction("", "SetResources", new JSValue(resources));
			}

			//UpdateSelection();
		}


		// Used for debugging purposes
		public bool DrawEllipseGuides { get; set; }


		private void OnCancelCreation()
		{
			if (CancelledCreationEvent != null)
			{
				// TODO: 2012-08-10 Allow cancelling
				//CancelledCreationEvent(new EntityEventArgs(creatingEntityID.Value));
			}
			
			// Cancel whatever they are creating
			world.DeleteComponents(creatingEntityID.Value);
			creatingEntityID = null;
		}


		/// <summary>
		/// Draw the back of the selection circle around each of the selected entities
		/// </summary>
		/// <param name="spriteBatch">The sprite batch to use</param>
		/// <param name="tint">The color to tint this</param>
		private void DrawSelectionCirclesBack(SpriteBatch spriteBatch, Color tint)
		{
			foreach (var selectedEntity in selectedEntities)
			{
				Position selectedEntityPosition = world.GetComponent<Position>(selectedEntity);
				spriteBatch.DrawEllipseBack(world.WorldToScreen(selectedEntityPosition.Center),
				                            world.Scale(selectedEntityPosition.Radius),
				                            Color.Green);

				//if(DrawEllipseGuides)
				//{
				//    // Draw a bunch of elliptical guides for debugging purposes
				//    for(float theta = 0; theta < Math.PI * 2; theta += (float)Math.PI / 6f)
				//    {
				//        spriteBatch.DrawLine(world.WorldToScreen(selectedEntityPosition.Center),
				//                             world.WorldToScreen(selectedEntityPosition.Center +
				//                                                 new Vector2((float)Math.Sin(theta),
				//                                                             (float)Math.Cos(theta)) * selectedEntityPosition.Radius),
				//                             Color.White);
				//    }
				//}
			}
		}


		/// <summary>
		/// Draw the front of the selection circle around each of the selected entities
		/// </summary>
		/// <param name="spriteBatch">The sprite batch to use</param>
		/// <param name="tint">The color to tint this</param>
		private void DrawSelectionCirclesFront(SpriteBatch spriteBatch, Color tint)
		{
			foreach (var selectedEntity in selectedEntities)
			{
				Position selectedEntityPosition = world.GetComponent<Position>(selectedEntity);
				spriteBatch.DrawEllipseFront(world.WorldToScreen(selectedEntityPosition.Center),
				                             world.Scale(selectedEntityPosition.Radius),
				                             Color.Green,
				                             DrawEllipseGuides);
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
		public override void Draw(GameTime gameTime)
		{
			spriteBatch.Begin();

			DrawSelectionCirclesFront(spriteBatch, Color.White);

			if(creatingEntityID != null)
			{
				// Draw with a red tint if it's an invalid spot to build
				if (IsValidToBuildHere())
				{
					foreach(var animator in world.GetComponents<Animator>(creatingEntityID.Value))
					{
						animator.Tint = Color.White;
					}
				}
				else
				{
					foreach(var animator in world.GetComponents<Animator>(creatingEntityID.Value))
					{
						animator.Tint = new Color(255, 50, 50, 255);
					}
				}
			}

			spriteBatch.End();
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
						// Show constuction panel?
					}
					localActor = value;
				}
				else
				{
					// Hide constuction panel?
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
					desiredScaleFactor = 0.5f;
				}
				else if (value >= 7.0f)
				{
					desiredScaleFactor = 7.0f;
				}
				else
				{
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
				if (creatingEntityID != null)
				{
					OnCancelCreation();
				}

				// Create a new power station
				creatingEntityID = EntityFactory.Create("Solar Station", new Dictionary<String, object>(){
					{ "Sprite.Scale", 0.7f },
					{ "Sprite.Set", " " + GlobalRandom.Next(1, 4) },
					{ "Sprite.Animation", null },
					{ "Sprite.Orientation", (float)GlobalRandom.Next(0, 359) },
					{ "Transpose.Position", ScreenToWorld(new Vector2(theMouse.X, theMouse.Y)) },
					{ "Transpose.Radius", 40 },
					{ "OwningForce", localActor.PrimaryForce }
				});
			}
		}


		private void btnPowerNode_Clicked(object sender, EventArgs e)
		{
			if (!world.Paused)
			{
				if (creatingEntityID != null)
				{
					OnCancelCreation();
				}

				// Create a new power station
				creatingEntityID = EntityFactory.Create("Power Node", new Dictionary<String, object>(){
					{ "Sprite.Scale", 0.4f },
					{ "Sprite.Set", " " + GlobalRandom.Next(1, 4) },
					{ "Sprite.Animation", null },
					{ "Sprite.Orientation", (float)GlobalRandom.Next(0, 359) },
					{ "Transpose.Position", ScreenToWorld(new Vector2(theMouse.X, theMouse.Y)) },
					{ "Transpose.Radius", 20 },
					{ "OwningForce", localActor.PrimaryForce }
				});
			}
		}


		private void btnLaserMiner_Clicked(object sender, EventArgs e)
		{
			if (!world.Paused)
			{
				if (creatingEntityID != null)
				{
					OnCancelCreation();
				}

				// Create a new laser miner
				creatingEntityID = EntityFactory.Create("Laser Miner", new Dictionary<String, object>(){
					{ "Sprite.Scale", 0.6f },
					{ "Sprite.Set", null },
					{ "Sprite.Animation", null },
					{ "Sprite.Orientation", (float)GlobalRandom.Next(0, 359) },
					{ "Transpose.Position", ScreenToWorld(new Vector2(theMouse.X, theMouse.Y)) },
					{ "Transpose.Radius", 30 },
					{ "OwningForce", localActor.PrimaryForce }
				});
			}
		}


		void btnLaserTower_Clicked(object sender, EventArgs e)
		{
			if (!world.Paused)
			{

				if (creatingEntityID != null)
				{
					OnCancelCreation();
				}

				// Create a new laser tower
				creatingEntityID = EntityFactory.Create("Laser Tower", new Dictionary<String, object>(){
					{ "Sprite.Scale", 0.6f },
					{ "Sprite.Set", null },
					{ "Sprite.Animation", null },
					{ "Sprite.Orientation", (float)GlobalRandom.Next(0, 359) },
					{ "Transpose.Position", ScreenToWorld(new Vector2(theMouse.X, theMouse.Y)) },
					{ "Transpose.Radius", 30 },
					{ "OwningForce", localActor.PrimaryForce }
				});
			}
		}


		void btnMissileTower_Clicked(object sender, EventArgs e)
		{
			if (!world.Paused)
			{

				if (creatingEntityID != null)
				{
					OnCancelCreation();
				}

				// Create a new missile tower
				creatingEntityID = EntityFactory.Create("Missile Tower", new Dictionary<String, object>(){
					{ "Sprite.Scale", 0.6f },
					{ "Sprite.Set", null },
					{ "Sprite.Animation", null },
					{ "Sprite.Orientation", (float)GlobalRandom.Next(0, 359) },
					{ "Transpose.Position", ScreenToWorld(new Vector2(theMouse.X, theMouse.Y)) },
					{ "Transpose.Radius", 30 },
					{ "OwningForce", localActor.PrimaryForce }
				});
			}
		}


		//private List<ICanKillSelf> CreateRangeRings(ConstructableEntity entity)
		//{
		//    var createdSuicidals = new List<ICanKillSelf>(6);
		//    var rangeRingDefinitions = new List<Tuple<int, Color, string>>(10);
		//    entity.GetRangeRings(ref rangeRingDefinitions);

		//    foreach (var rangeRingDefinition in rangeRingDefinitions)
		//    {
		//        Ring ring = new Ring(world,
		//                             entity.Position,
		//                             rangeRingDefinition.Item1,
		//                             rangeRingDefinition.Item2);
		//        components.Add(ring);
		//        createdSuicidals.Add(ring);

		//        PositionOffset positionOffset = new PositionOffset(world, entity.Position, new Vector2(-25, -rangeRingDefinition.Item1 - 17));
		//        FreeText text = new FreeText(world,
		//                                     entity.Position.Center + new Vector2(-25, -rangeRingDefinition.Item1 - 17),
		//                                     rangeRingDefinition.Item3,
		//                                     rangeRingDefinition.Item2);
		//        components.Add(positionOffset);
		//        components.Add(text);
		//        createdSuicidals.Add(text);
		//    }

		//    return createdSuicidals;
		//}


		//private void CreateRangeRingsForConstruction(ConstructableEntity entity)
		//{
		//    foreach (var suicidal in CreateRangeRings(entity))
		//    {
		//        // TODO: I think these events will continue to hold on to the dying entity long after its dead and it will prevent garbage collection
		//        CancelledCreationEvent += suicidal.KillSelf;
		//        //world.StructureStartedEventPreAuth += suicidal.KillSelf;
		//    }
		//}


		//private void CreateRangeRingsForSelection(ConstructableEntity entity)
		//{
		//    foreach (var suicidal in CreateRangeRings(entity))
		//    {
		//        // TODO: I think these events will continue to hold on to the dying entity long after its dead and it will prevent garbage collection
		//        SelectionChanged += suicidal.KillSelf;
		//    }
		//}


		//private void CreatePowerLinker(ConstructableEntity entity)
		//{
		//    PowerLinker powerLinker = new PowerLinker(world, localActor.PrimaryForce, entity);
		//    CancelledCreationEvent += powerLinker.KillSelf;
		//    //world.StructureStartedEventPreAuth += powerLinker.KillSelf;
		//    components.Add(powerLinker);
		//}

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

				if (!clickHandled && creatingEntityID != null)
				{
					// Have we just tried to build this guy?
					if (/*mouse.LeftButton == EnhancedButtonState.JUST_RELEASED &&*/ IsValidToBuildHere())
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
					List<int> possiblyClickedEntities = world.EntitiesInArea(new Rectangle((int)(mouseMapCoords.X + 0.5), (int)(mouseMapCoords.Y + 0.5), 1, 1));
					foreach (int entity in possiblyClickedEntities)
					{
						// Make sure the unit was clicked
						if (world.GetComponent<Position>(entity).IsIntersecting(mouseMapCoords, 1))
						{
							if(theKeyboard.IsKeyDown(Keys.LeftShift) || theKeyboard.IsKeyDown(Keys.RightShift))
							{
								// Multi-select
								selectedEntities.Add(entity);

								// Connect to the death event
								HitPoints hitPoints = world.GetNullableComponent<HitPoints>(entity);
								if(hitPoints != null)
								{
									hitPoints.DyingEvent += SelectedEntityDying;
								}
							}
							else
							{
								// Single select

								// Disconnect from the death events
								foreach (HitPoints selectedHitPoints in selectedEntities.Select(selectedEntity => world.GetNullableComponent<HitPoints>(selectedEntity)).Where(selectedHitPoints => selectedHitPoints != null))
								{
									selectedHitPoints.DyingEvent -= SelectedEntityDying;
								}

								selectedEntities.Clear();
								selectedEntities.Add(entity);

								// Connect to the death event
								HitPoints hitPoints = world.GetNullableComponent<HitPoints>(entity);
								if(hitPoints != null)
								{
									hitPoints.DyingEvent += SelectedEntityDying;
								}
							}

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

						// Disconnect from the death events
						foreach (HitPoints selectedHitPoints in selectedEntities.Select(selectedEntity => world.GetNullableComponent<HitPoints>(selectedEntity)).Where(selectedHitPoints => selectedHitPoints != null))
						{
							selectedHitPoints.DyingEvent -= SelectedEntityDying;
						}

						selectedEntities.Clear();

						OnSelectionChanged();
					}
				}
			}
		}


		private void BuildStructure()
		{
			if(creatingEntityID == null)
			{
				// Ok, wow. Fail
				Debugger.Break();
			}

			constructionSound.Play(Math.Min(1, world.Scale(1f)), 0, 0);

			Constructable constructable = world.GetNullableComponent<Constructable>(creatingEntityID.Value);
			if(constructable != null)
			{
				constructable.IsBeingPlaced = false;
				constructable.IsConstructing = true;
			}

			PowerGridNode powerNode = world.GetNullableComponent<PowerGridNode>(creatingEntityID.Value);
			if(powerNode != null)
			{
				world.GetPowerGrid(powerNode).ConnectToPowerGrid(powerNode);
			}

			//// Reflectively look up what they are making, and create an other one of the same thing in the game
			//Type creatingType = creating.GetType();
			//ConstructorInfo creatingTypeConstuctructor = creatingType.GetConstructor(new Type[]{ typeof (World), typeof (IComponentList), typeof (Force), typeof (Vector2) });

			//if (creatingTypeConstuctructor == null)
			//{
			//    System.Console.WriteLine("Failed to find a constructor for the current constructable! Unable to construct entity");
			//    Debugger.Break(); // John, there's a problem with the reflection above. Fix it!
			//    return;
			//}

			creatingEntityID = null;
			//if (!theKeyboard.IsKeyDown(Keys.LeftShift) && !theKeyboard.IsKeyDown(Keys.RightShift))
			//{
			//    OnCancelCreation();
			//}
		}


		protected void OnSelectionChanged()
		{
			if (SelectionChanged != null)
			{
				SelectionChanged(new MultiEntityEventArgs(selectedEntities));
			}

			UpdateSelection();
		}


		/// <summary>
		/// Updates the UI's selection information
		/// </summary>
		private void UpdateSelection()
		{
			if(selectedEntities.Count >= 1)
			{
				// Note: I'm using Strings instead of ints because it serializes to JSON a little more cleanly
				// This is defined as a: Dictionary<entityID, Dictionary<componentName, Component>>
				//Dictionary<String, Dictionary<String, Component>> entities = new Dictionary<String, Dictionary<String, Component>>(selectedEntities.Count);
				//foreach (var selectedEntity in selectedEntities)
				//{
				//    entities[selectedEntity.ToString()] = world.GetComponents<Component>(selectedEntities[0]).ToDictionary(component => component.GetComponentClassName(), component => component);
				//}


				List<Dictionary<String, Object>> entities = new List<Dictionary<String, Object>>(selectedEntities.Count);
				int index = 0;
				foreach (var selectedEntity in selectedEntities)
				{
					entities.Add(world.GetComponents<Component>(selectedEntity).ToDictionary(component => component.GetComponentClassName(), component => (Object)component));
					entities[index].Add("EntityID", selectedEntity);
					index++;
				}


				JSON.Instance.Parameters.EnableAnonymousTypes = true;
				String json = JSON.Instance.ToJSON(entities);
#if DEBUG
				json = JSON.Instance.Beautify(json);
#endif
				awesomium.WebView.CallJavascriptFunction("", "UpdateSelection", new JSValue(json));
			}
			else
			{
				awesomium.WebView.CallJavascriptFunction("", "UpdateSelection", new JSValue());
			}
		}


		private void SelectedEntityDying(EntityDyingEventArgs e)
		{
			// Remove any deleted entities from the selection list
			HitPoints dyingHitPoints = e.Component as HitPoints;
			if (dyingHitPoints != null)
			{
				selectedEntities.Remove(dyingHitPoints.EntityID);
				dyingHitPoints.DyingEvent -= SelectedEntityDying;

				// Tell anyone who is interested in a selection change
				OnSelectionChanged();
			}
		}


		private void ResumeGame(Object sender, EventArgs e)
		{
			world.Paused = false;
		}


		/// <summary>
		/// Is this a valid place to build?
		/// </summary>
		/// <returns>True if it's legal to build here, false otherwise</returns>
		public virtual bool IsValidToBuildHere()
		{
			bool valid = true;

			// This will grab all objects who's bounding square intersects with us
			Position buildingPosition = world.GetComponent<Position>(creatingEntityID.Value);
			List<int> nearbyEntities = world.EntitiesInArea(buildingPosition.Center, buildingPosition.Radius, true);
			foreach (var nearbyEntity in nearbyEntities)
			{
				if(nearbyEntity != creatingEntityID)
				{
					Position position = world.GetComponent<Position>(nearbyEntity);
					if (position.IsIntersecting(buildingPosition))
					{
						valid = false;
						break;
					}
				}
			}

			//List<Entity> nearbyEntities = world.EntitiesInArea(Rect);
			//foreach (Entity entity in nearbyEntities)
			//{
			//    // Now determine if they are really intersecting
			//    if(entity.Solid && Position.IsIntersecting(entity.Position))
			//    {
			//        valid = false;
			//        break;
			//    }
			//}
			return valid;
		}
	}
}