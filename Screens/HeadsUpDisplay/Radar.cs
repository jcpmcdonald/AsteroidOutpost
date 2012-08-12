using AsteroidOutpost.Entities;
using AsteroidOutpost.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AsteroidOutpost.Screens.HeadsUpDisplay
{
	class Radar
	{
		/*
		private readonly World world;
		private readonly AOHUD hud;
		private bool isDragging;
		
		
		public Radar(World world, AOHUD theHUD, int x, int y, int w, int h) : base(x, y, w, h)
		{
			this.world = world;
			hud = theHUD;
		}


		/// <summary>
		/// Draw this control
		/// </summary>
		/// <param name="spriteBatch">The spriteBatch to draw to</param>
		/// <param name="tint">The color used to tint this control. Use Color.White to draw this control normally</param>
		public void Draw(SpriteBatch spriteBatch, Color tint)
		{
			Rectangle focusScreen = hud.FocusScreen;
			
			// Draw the background
			spriteBatch.FillRectangle(Rect, new Color(20, 20, 20, 170));
			
			
			// Draw the entities
			// Draw the asteroids first
			foreach(Entity entity in world.Entities)
			{
				if(entity is Asteroid)
				{
					Vector2 mapLocation = new Vector2(entity.Position.Center.X / world.MapWidth * size.Width,
					                                  entity.Position.Center.Y / world.MapHeight * size.Height);
					spriteBatch.PutPixel(mapLocation + LocationAbs, ColorPalette.ApplyTint(Color.Gray, tint));
				}
			}
			foreach(Entity entity in world.Entities)
			{
				if(!(entity is Asteroid))
				{
					Vector2 mapLocation = new Vector2(entity.Position.Center.X / world.MapWidth * size.Width,
					                                  entity.Position.Center.Y / world.MapHeight * size.Height);

					Color color;
					if(entity.OwningForce == null)
					{
						color = Color.Magenta;
					}
					else if (entity.OwningForce.Team == Team.AI)
					{
						color = Color.Red;
					}
					else if (entity.OwningForce.Team == Team.Team1)
					{
						color = new Color(50, 250, 50);
					}
					else
					{
						color = Color.Purple;
					}

					spriteBatch.FillRectangle(new Vector2(-1, -1) + mapLocation + LocationAbs, new Vector2(2, 2), ColorPalette.ApplyTint(color, tint));
				}
			}


			// Draw the focus screen
			int miniFocusX = (int)(((double)focusScreen.X / world.MapWidth) * size.Width);// + (int)origin.X;
			int miniFocusY = (int)(((double)focusScreen.Y / world.MapHeight) * size.Height);// + (int)origin.Y;
			int miniFocusW = (int)(((double)focusScreen.Width / world.MapWidth) * size.Width);
			int miniFocusH = (int)(((double)focusScreen.Height / world.MapHeight) * size.Height);


			spriteBatch.DrawRectangle(new Vector2(miniFocusX, miniFocusY) + LocationAbs, new Vector2(miniFocusW, miniFocusH), ColorPalette.ApplyTint(Color.White, tint), 1);
			
		}


		/// <summary>
		/// Called when any mouse button is released
		/// </summary>
		/// <param name="mouse">A reference to the mouse</param>
		/// <param name="mouseButton">The mouse button that was released</param>
		protected void OnMouseUp(EnhancedMouseState mouse, MouseButton mouseButton, ref bool handled)
		{
			isDragging = false;
		}


		/// <summary>
		/// Called when any mouse button is pressed
		/// </summary>
		/// <param name="theMouse">A reference to the mouse</param>
		/// <param name="theMouseButton">The mouse button that was pressed</param>
		protected void OnMouseDown(EnhancedMouseState theMouse, MouseButton theMouseButton, ref bool handled)
		{
			isDragging = true;
			hud.FocusWorldPoint = new Vector2((theMouse.X - LocationAbs.X) / size.Width * world.MapWidth,
			                                  (theMouse.Y - LocationAbs.Y) / size.Height * world.MapHeight);

		}


		/// <summary>
		/// Called when the mouse moves over the control
		/// </summary>
		/// <param name="theMouse">A reference to the mouse</param>
		protected void OnMouseMove(EnhancedMouseState theMouse, ref bool handled)
		{
			if (isDragging)
			{
				hud.FocusWorldPoint = new Vector2((theMouse.X - LocationAbs.X) / size.Width * world.MapWidth,
				                                  (theMouse.Y - LocationAbs.Y) / size.Height * world.MapHeight);
			}
		}


		/// <summary>
		/// Called when the mouse has left the control's bounds
		/// </summary>
		/// <param name="theMouse">A reference to the mouse</param>
		protected void OnMouseLeave(EnhancedMouseState theMouse, ref bool handled)
		{
			isDragging = false;
		}

		*/
	}
}
