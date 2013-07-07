using System;
using System.Linq;
using System.Collections.Generic;
using AsteroidOutpost.Components;
using AsteroidOutpost.Entities;
using AsteroidOutpost.Entities.Eventing;
using AsteroidOutpost.Eventing;
using Awesomium.Core;
using AwesomiumXNA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AsteroidOutpost.Systems
{
	/// <summary>
	/// The HUD is how the user interacts with the game
	/// </summary>
	public class AOHUD : DrawableGameComponent
	{

		private SpriteBatch spriteBatch;

		private Vector2 focusWorldPoint;
		private Vector2? middleMouseGrabPoint;
		private int? creatingEntityID; // Are they creating an entity?

		private EnhancedMouseState theMouse = new EnhancedMouseState();
		private EnhancedKeyboardState theKeyboard = new EnhancedKeyboardState();

		private readonly List<int> selectedEntities = new List<int>();
		public event Action<MultiEntityEventArgs> SelectionChanged;


		private readonly World world;
		private readonly AwesomiumComponent awesomium;
		private float scaleFactor = 1.0f; // 1.0 = no scaling, 0.5 = zoomed in, 2.0 = zoomed out
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
			hotkeys.Add(Keys.H, GoHome);
			hotkeys.Add(Keys.R, RefreshEntityTemplates);
			hotkeys.Add(Keys.OemPlus, TimeSpeedUp);
			hotkeys.Add(Keys.OemMinus, TimeSlowDown);
			hotkeys.Add(Keys.D0, TimeReset);


			// Create callbacks for Awesomium content to communicate with the hud
			awesomium = game.Awesomium;

			//while(!awesomium.WebView.IsDocumentReady)
			//{
			//    WebCore.Update();
			//}

			JSObject jsHud = awesomium.WebView.CreateGlobalJavascriptObject("hud");
			jsHud.Bind("OnMouseUp", false, OnMouseUp);
			jsHud.Bind("OnMouseDown", false, OnMouseDown);

			jsHud.Bind("ResumeGame", false, ResumeGame);
			jsHud.Bind("ForfeitGame", false, ForfeitGame);

			jsHud.Bind("BuildSolarStation", false, btnPower_Clicked);
			jsHud.Bind("BuildPowerNode", false, btnPowerNode_Clicked);
			jsHud.Bind("BuildLaserMiner", false, btnLaserMiner_Clicked);
			jsHud.Bind("BuildLaserTower", false, btnLaserTower_Clicked);
			jsHud.Bind("BuildMissileTower", false, btnMissileTower_Clicked);

			jsHud.Bind("EditEntity", false, awesomium_EditEntity);

			world.PauseToggledEvent += WorldOnPauseToggledEvent;
		}


		private void RefreshEntityTemplates(object sender, EventArgs e)
		{
			EntityFactory.Refresh(world);
		}


		private void awesomium_EditEntity(object sender, JavascriptMethodEventArgs javascriptMethodEventArgs)
		{
			String json = javascriptMethodEventArgs.Arguments[0].ToString();
			JObject change = JObject.Parse(json);
			int entityID = (int)change["EntityID"];
			change.Remove("EntityID");
			EntityTemplate.Update(world, entityID, change);
		}


		private void TimeSpeedUp(object sender, EventArgs e)
		{
			world.TimeMultiplier += 0.2f;
		}


		private void TimeSlowDown(object sender, EventArgs e)
		{
			world.TimeMultiplier -= 0.2f;
		}


		private void TimeReset(object sender, EventArgs e)
		{
			world.TimeMultiplier = 1.0f;
		}


		private void GoHome(object sender, EventArgs e)
		{

		}


		public void DisablePowerButton()
		{
			hotkeys.Remove(Keys.S);
			world.ExecuteAwesomiumJS("$('#SolarStation').addClass('disabled');");
		}
		public void EnablePowerButton()
		{
			hotkeys.Add(Keys.S, btnPower_Clicked);
			world.ExecuteAwesomiumJS("$('#SolarStation').removeClass('disabled');");
		}


		public void DisableMinerButton()
		{
			hotkeys.Remove(Keys.M);
			world.ExecuteAwesomiumJS("$('#LaserMiner').addClass('disabled');");
		}
		public void EnableMinerButton()
		{
			hotkeys.Add(Keys.M, btnLaserMiner_Clicked);
			world.ExecuteAwesomiumJS("$('#LaserMiner').removeClass('disabled');");
		}


		public void DisablePowerNodeButton()
		{
			hotkeys.Remove(Keys.N);
			world.ExecuteAwesomiumJS("$('#PowerNode').addClass('disabled');");
		}
		public void EnablePowerNodeButton()
		{
			hotkeys.Add(Keys.N, btnPowerNode_Clicked);
			world.ExecuteAwesomiumJS("$('#PowerNode').removeClass('disabled');");
		}


		public void DisableLaserTowerButton()
		{
			hotkeys.Remove(Keys.L);
			world.ExecuteAwesomiumJS("$('#LaserTower').addClass('disabled');");
		}
		public void EnableLaserTowerButton()
		{
			hotkeys.Add(Keys.L, btnLaserTower_Clicked);
			world.ExecuteAwesomiumJS("$('#LaserTower').removeClass('disabled');");
		}


		public void DisableMissileTowerButton()
		{
			hotkeys.Remove(Keys.I);
			world.ExecuteAwesomiumJS("$('#MissileTower').addClass('disabled')");
		}
		public void EnableMissileTowerButton()
		{
			hotkeys.Add(Keys.I, btnMissileTower_Clicked);
			world.ExecuteAwesomiumJS("$('#MissileTower').removeClass('disabled');");
		}


		public void ShowModalDialog(String text)
		{
			world.Paused = true;
			world.ExecuteAwesomiumJS(String.Format("ShowModalDialog('{0}')", text.Replace("'", "\\'")));
		}



		private void WorldOnPauseToggledEvent(bool paused)
		{
			awesomium.WebView.ExecuteJavascript(String.Format("SetPaused({0})", paused.ToString().ToLower()));
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
					if (world.Paused)
					{
						world.ExecuteAwesomiumJS("ShowModalGameMenu()");
					}
				}
			}


			if (world.Paused) { return; }


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

					Vector2 worldPosition = world.ScreenToWorld(new Vector2(theMouse.X, theMouse.Y));
					EntityFactory.Create("Spaceship", aiActor.PrimaryForce, new JObject{
						{ "Position", new JObject{
							{ "Center", String.Format("{0}, {1}", worldPosition.X, worldPosition.Y) },
						}}
					});
					//new Dictionary<String, object>(){
					//    { "Sprite.Scale", 0.7f },
					//    { "Sprite.Set", null },
					//    { "Sprite.Animation", null },
					//    { "Sprite.Orientation", (float)GlobalRandom.Next(0, 359) },
					//    { "Transpose.Position", ScreenToWorld(new Vector2(theMouse.X, theMouse.Y)) },
					//    { "Transpose.Radius", 40 },
					//    { "OwningForce", aiActor.PrimaryForce }
					//})};
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
				awesomium.WebView.ExecuteJavascript(String.Format("SetResources({0})", (int)(LocalActor.PrimaryForce.GetMinerals() + 0.5)));
			}

			UpdateSelection();
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

				// Draw range circles for various components
				LaserMiner laserMiner = world.GetNullableComponent<LaserMiner>(selectedEntity);
				if(laserMiner != null)
				{
					spriteBatch.DrawEllipse(world.WorldToScreen(selectedEntityPosition.Center),
					                        world.Scale(laserMiner.MiningRange),
					                        Color.Red);
				}

				LaserWeapon laserWeapon = world.GetNullableComponent<LaserWeapon>(selectedEntity);
				if(laserWeapon != null)
				{
					spriteBatch.DrawEllipse(world.WorldToScreen(selectedEntityPosition.Center),
					                        world.Scale(laserWeapon.Range),
					                        Color.Red);
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
		public override void Draw(GameTime gameTime)
		{
			spriteBatch.Begin();

			DrawSelectionCirclesFront(spriteBatch, Color.White);

			if(creatingEntityID != null)
			{
				// Draw with a red tint if it's an invalid spot to build
				List<Position> blockers = GetBuildBlockers();
				if (!blockers.Any())
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

					// Draw the radius of the creating entity, and the radius of all the blocking entities
					Position creatingPosition = world.GetComponent<Position>(creatingEntityID.Value);
					spriteBatch.DrawEllipse(world.WorldToScreen(creatingPosition.Center),
						                        world.Scale(creatingPosition.Radius),
						                        Color.Red);

					foreach (var position in blockers)
					{
						spriteBatch.DrawEllipse(world.WorldToScreen(position.Center),
						                        world.Scale(position.Radius),
						                        Color.Red);
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
				Vector2 worldPosition = ScreenToWorld(new Vector2(theMouse.X, theMouse.Y));
				creatingEntityID = EntityFactory.Create("Solar Station", localActor.PrimaryForce, new JObject{
					{ "Animator", new JObject{
						{ "CurrentOrientation", (float)GlobalRandom.Next(0, 359) }
					}},
					{ "Position", new JObject{
						{ "Center", String.Format("{0}, {1}", worldPosition.X, worldPosition.Y) },
					}}
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
				Vector2 worldPosition = ScreenToWorld(new Vector2(theMouse.X, theMouse.Y));
				creatingEntityID = EntityFactory.Create("Power Node", localActor.PrimaryForce, new JObject{
					{ "Animator", new JObject{
						{ "CurrentOrientation", (float)GlobalRandom.Next(0, 359) }
					}},
					{ "Position", new JObject{
						{ "Center", String.Format("{0}, {1}", worldPosition.X, worldPosition.Y) },
					}}
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
				Vector2 worldPosition = ScreenToWorld(new Vector2(theMouse.X, theMouse.Y));
				creatingEntityID = EntityFactory.Create("Laser Miner", localActor.PrimaryForce, new JObject{
					{ "Animator", new JObject{
						{ "CurrentOrientation", (float)GlobalRandom.Next(0, 359) }
					}},
					{ "Position", new JObject{
						{ "Center", String.Format("{0}, {1}", worldPosition.X, worldPosition.Y) },
					}}
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
				Vector2 worldPosition = ScreenToWorld(new Vector2(theMouse.X, theMouse.Y));
				creatingEntityID = EntityFactory.Create("Laser Tower", localActor.PrimaryForce, new JObject{
					{ "Animator", new JObject{
						{ "CurrentOrientation", (float)GlobalRandom.Next(0, 359) }
					}},
					{ "Position", new JObject{
						{ "Center", String.Format("{0}, {1}", worldPosition.X, worldPosition.Y) },
					}}
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
				Vector2 worldPosition = ScreenToWorld(new Vector2(theMouse.X, theMouse.Y));
				creatingEntityID = EntityFactory.Create("Missile Tower", localActor.PrimaryForce, new JObject{
					{ "Animator", new JObject{
						{ "CurrentOrientation", (float)GlobalRandom.Next(0, 359) }
					}},
					{ "Position", new JObject{
						{ "Center", String.Format("{0}, {1}", worldPosition.X, worldPosition.Y) },
					}}
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



		protected void OnMouseDown(object sender, JavascriptMethodEventArgs e)
		{
			MouseButton mouseButton = (MouseButton)(int)e.Arguments[1];
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
		protected void OnMouseUp(object sender, JavascriptMethodEventArgs e)
		{
			bool mouseUpOverHUD = e.Arguments[0];
			MouseButton mouseButton = (MouseButton)(int)e.Arguments[1];
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

			Constructible constructable = world.GetNullableComponent<Constructible>(creatingEntityID.Value);
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

			SetSelection();
		}


		/// <summary>
		/// Sets the UI's selection information
		/// </summary>
		private void SetSelection()
		{
			//awesomium.WebView.CallJavascriptFunction("", "SetSelection", GetSelectionJSON());
			//bool loaded = !awesomium.WebView.ExecuteJavascriptWithResult("typeof scopeOf == 'undefined'").ToBoolean();
			//if(loaded)
			//{
				awesomium.WebView.ExecuteJavascript("if(typeof SetSelection != 'undefined'){ SetSelection(" + GetSelectionJSON() + "); }");
			//}
		}

		/// <summary>
		/// Updates the UI's selection information
		/// </summary>
		private void UpdateSelection()
		{
			//bool loaded = !awesomium.WebView.ExecuteJavascriptWithResult("typeof scopeOf == 'undefined'").ToBoolean();
			//if(loaded)
			{
				//awesomium.WebView.ExecuteJavascript("UpdateSelection(" + GetSelectionJSON() + ");");
				awesomium.WebView.ExecuteJavascript(String.Format("UpdateSelection({0})", GetSelectionJSON()));
			}
		}


		private String GetSelectionJSON()
		{
			if(selectedEntities.Count >= 1)
			{
				List<Dictionary<String, Object>> entities = new List<Dictionary<String, Object>>(selectedEntities.Count);
				int index = 0;
				foreach (var selectedEntity in selectedEntities)
				{
					entities.Add(world.GetComponents<Component>(selectedEntity).ToDictionary(component => component.GetComponentClassName(), component => (Object)component));
					entities[index].Add("EntityID", selectedEntity);
					index++;
				}


				//JSON.Instance.Parameters.EnableAnonymousTypes = true;
				//String json = JSON.Instance.ToJSON(entities);
				String json = JsonConvert.SerializeObject(entities);
#if DEBUG
				//json = JSON.Instance.Beautify(json);
				//Console.WriteLine(json);
#endif
				return json;
			}
			else
			{
				return "[]";
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
		/// Forfeit the Game and return to the main menu
		/// </summary>
		/// <param name="sender">Not relevant</param>
		/// <param name="e">None</param>
		private void ForfeitGame(Object sender, EventArgs e)
		{
			((AOGame)Game).DestroyWorld();
		}


		/// <summary>
		/// Is this a valid place to build?
		/// </summary>
		/// <returns>True if it's legal to build here, false otherwise</returns>
		public virtual bool IsValidToBuildHere()
		{
			return GetBuildBlockers().Any();
		}


		/// <summary>
		/// Is this a valid place to build? This returns who is blocking you
		/// </summary>
		/// <returns>Returns a list of all entities blocking this location for construction</returns>
		public virtual List<Position> GetBuildBlockers()
		{
			List<Position> blockers = new List<Position>();

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
						blockers.Add(position);
					}
				}
			}

			return blockers;
		}
	}
}