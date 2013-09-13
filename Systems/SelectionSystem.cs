using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using AsteroidOutpost.Components;
using AsteroidOutpost.Entities.Eventing;
using AsteroidOutpost.Eventing;
using Awesomium.Core;
using AwesomiumXNA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;

namespace AsteroidOutpost.Systems
{
	public class SelectionSystem : DrawableGameComponent
	{
		private SpriteBatch spriteBatch;
		private readonly World world;
		private AwesomiumComponent awesomium;

		private readonly List<int> selectedEntities = new List<int>();
		public event Action<MultiEntityEventArgs> SelectionChanged;


		// Used for debugging purposes
		public bool DrawEllipseGuides { get; set; }


		public SelectionSystem(AOGame game, World world)
			: base(game)
		{
			spriteBatch = new SpriteBatch(game.GraphicsDevice);
			this.world = world;
			awesomium = game.Awesomium;
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

				ProjectileLauncher projectileLauncher = world.GetNullableComponent<ProjectileLauncher>(selectedEntity);
				if(projectileLauncher != null)
				{
					spriteBatch.DrawEllipse(world.WorldToScreen(selectedEntityPosition.Center),
					                        world.Scale(projectileLauncher.Range),
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

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			UpdateSelection();
		}


		public override void Draw(GameTime gameTime)
		{
			spriteBatch.Begin();
			DrawSelectionCirclesFront(spriteBatch, Color.White);
			spriteBatch.End();
		}


		protected void OnMouseDown(object sender, MouseButton mouseButton, EnhancedMouseState theMouse)
		{
			// TODO: Start a multi-select, BUT only actually do something about it after they move... lets say 10px away from this location
		}


		public void OnMouseUp(object sender, MouseButton mouseButton, EnhancedMouseState theMouse)
		{
			bool clickHandled = false;

			if (mouseButton == MouseButton.LEFT)
			{
				if (!clickHandled)
				{
					// Did we click on a unit?
					Vector2 mouseMapCoords = world.ScreenToWorld(theMouse.X, theMouse.Y);

					// Grab a possible list of clicked entities by using a square-area search
					List<int> possiblyClickedEntities = world.EntitiesInArea(new Rectangle((int)(mouseMapCoords.X + 0.5), (int)(mouseMapCoords.Y + 0.5), 1, 1));
					foreach (int entity in possiblyClickedEntities)
					{
						// Make sure the unit was clicked
						if (world.GetComponent<Position>(entity).IsIntersecting(mouseMapCoords, 1))
						{
							Selectable selectable = world.GetNullableComponent<Selectable>(entity);
							if(selectable == null)
							{
								continue;
							}

							//if(theKeyboard.IsKeyDown(Keys.LeftShift) || theKeyboard.IsKeyDown(Keys.RightShift))
							//{
							//    // Multi-select
							//    selectedEntities.Add(selectable.EntityID);

							//    // Connect to the death event
							//    Perishable perishable = world.GetNullableComponent<Perishable>(entity);
							//    if(perishable != null)
							//    {
							//        perishable.Perishing += SelectedEntityDying;
							//    }
							//}
							//else
							//{
								// Single select

								// Disconnect from the death events
								foreach (Perishable selectedPerishable in selectedEntities.Select(selectedEntity => world.GetNullableComponent<Perishable>(selectedEntity)).Where(p => p != null))
								{
									selectedPerishable.Perishing -= SelectedEntityDying;
								}

								selectedEntities.Clear();
								selectedEntities.Add(selectable.EntityID);

								// Connect to the death event
								Perishable perishable = world.GetNullableComponent<Perishable>(entity);
								if(perishable != null)
								{
									perishable.Perishing += SelectedEntityDying;
								}
							//}

							OnSelectionChanged();

							clickHandled = true;
						}
					}
				}

				if (!clickHandled && selectedEntities.Count > 0)
				{
					ClearSelection();
				}
			}
		}


		public void ClearSelection()
		{
			// Deselect the selected unit(s)

			// Disconnect from the death events
			foreach (Perishable selectedPerishable in selectedEntities.Select(selectedEntity => world.GetNullableComponent<Perishable>(selectedEntity)).Where(p => p != null))
			{
				selectedPerishable.Perishing -= SelectedEntityDying;
			}

			selectedEntities.Clear();

			OnSelectionChanged();
		}



		private void SelectedEntityDying(EntityPerishingEventArgs e)
		{
			// Remove any deleted entities from the selection list
			selectedEntities.Remove(e.EntityID);

			e.Perishable.Perishing -= SelectedEntityDying;

			// Tell anyone who is interested in a selection change
			OnSelectionChanged();
		}


		
		protected void OnSelectionChanged()
		{
			if (SelectionChanged != null)
			{
				SelectionChanged(new MultiEntityEventArgs(selectedEntities));
			}

			UpdateContextMenu();

			SetSelection();
		}


		protected void UpdateContextMenu()
		{
			if(selectedEntities.Count == 1)
			{
				Constructing constructing = world.GetNullableComponent<Constructing>(selectedEntities[0]);
				if(constructing != null)
				{
					world.HUD.ContextMenu.SetPage("constructing");

					constructing.ConstructionComplete += ConstructibleOnConstructionComplete;
				}
				else
				{
					Selectable selectable = world.GetComponent<Selectable>(selectedEntities[0]);
					if(selectable.ContextMenu != null)
					{
						world.HUD.ContextMenu.SetPage(selectable.ContextMenu);
					}
					else
					{
						world.HUD.ContextMenu.SetPage("deselect");
					}
				}
			}
			else
			{
				world.HUD.ContextMenu.SetPage("main");
			}
		}


		private void ConstructibleOnConstructionComplete(ConstructionCompleteEventArgs args)
		{
			args.Constructing.ConstructionComplete -= ConstructibleOnConstructionComplete;

			if(selectedEntities.Count == 1 && args.EntityID == selectedEntities[0])
			{
				Selectable selectable = world.GetComponent<Selectable>(selectedEntities[0]);
				if(selectable.ContextMenu != null)
				{
					world.HUD.ContextMenu.SetPage(selectable.ContextMenu);
				}
				else
				{
					world.HUD.ContextMenu.SetPage("deselect");
				}
			}
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
				world.ExecuteAwesomiumJS("if(typeof SetSelection != 'undefined'){ SetSelection(" + GetSelectionJSON() + "); }");
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
				world.ExecuteAwesomiumJS(String.Format(CultureInfo.InvariantCulture, "UpdateSelection({0})", GetSelectionJSON()));
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


		public void CancelConstruction()
		{
			if(selectedEntities.Count == 1)
			{
				int entityID = selectedEntities[0];
				Constructing constructing = world.GetNullableComponent<Constructing>(entityID);
				if (constructing != null)
				{
					Perishable perishable = world.GetComponent<Perishable>(entityID);
					perishable.OnPerish(new EntityPerishingEventArgs(perishable));
					world.DeleteComponents(entityID);
				}
			}
		}
	}
}
