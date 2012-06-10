using System;
using System.Collections.Generic;
using AsteroidOutpost.Entities;
using AsteroidOutpost.Entities.Eventing;
using AsteroidOutpost.Entities.Structures;
using C3.XNA;
using C3.XNA.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AsteroidOutpost.Screens.HeadsUpDisplay
{
	class SelectionInfo : Container
	{
		private readonly List<Entity> selectedEntities;			// This is a reference to the HUD's selected entity list

		private World world;
		private readonly AOHUD hud;
		
		public SelectionInfo(World world, int x, int y, int w, int h, AOHUD theHUD, List<Entity> theSelectedEntities) : base(x, y, w, h)
		{
			this.world = world;
			hud = theHUD;

			selectedEntities = theSelectedEntities;
			hud.SelectionChanged += hud_selectionChanged;
		}
		
		
		private void hud_selectionChanged(MultiEntityEventArgs e)
		{
			// Change the Upgrade button list
			if(selectedEntities == null || selectedEntities.Count == 0)
			{
				// Nothing selected
				controls.Clear();
			}
			else if(selectedEntities.Count == 1)
			{
				if (selectedEntities[0] is ConstructableEntity)
				{
					ConstructableEntity constructableEntity = (ConstructableEntity)selectedEntities[0];
					refreshUpgradeButtons(constructableEntity);
				}
				else
				{
					controls.Clear();
				}
			}
		}


		private void refreshUpgradeButtons(ConstructableEntity constructableEntity)
		{
			controls.Clear();
			if (hud.LocalActor != null && constructableEntity.OwningForce == hud.LocalActor.PrimaryForce)
			{
				int currY = 25;
				if (!constructableEntity.IsConstructing && !constructableEntity.IsUpgrading)
				{
					foreach (Upgrade upgrade in constructableEntity.AvailableUpgrades())
					{
						Button button = new Button(upgrade.Name, 250, currY, 200, 20);
						button.Tag = upgrade;
						button.Click += upgradeButton_Clicked;
						AddControl(button);
						currY += 30;
					}
				}
				else
				{
					if (constructableEntity.IsUpgrading)
					{
						Button button = new Button("Cancel", 250, currY, 200, 20);
						button.Click += cancelButton_Clicked;
						AddControl(button);
						//currY += 30;
					}
					if (constructableEntity.IsConstructing)
					{
						Button button = new Button("Cancel", 250, currY, 200, 20);
						button.Click += cancelButton_Clicked;
						AddControl(button);
						//currY += 30;
					}
				}
			}
		}


		/// <summary>
		/// This will get called when the upgrade button is clicked
		/// </summary>
		/// <param name="sender">A reference to the upgrade button that was clicked</param>
		/// <param name="e"></param>
		private void upgradeButton_Clicked(object sender, EventArgs e)
		{
			if(sender is Button && !world.Paused)
			{
				Button clickedButton = (Button)sender;
				if(clickedButton.Tag is Upgrade && selectedEntities.Count == 1 && selectedEntities[0] is ConstructableEntity)
				{
					ConstructableEntity constructableEntity = (ConstructableEntity)selectedEntities[0];
					if(!constructableEntity.IsConstructing && !constructableEntity.IsUpgrading)
					{
						constructableEntity.StartUpgrade((Upgrade)clickedButton.Tag);
					}

					refreshUpgradeButtons(constructableEntity);
				}
			}
		}


		/// <summary>
		/// This will get called when the cancel button is clicked
		/// </summary>
		/// <param name="sender">A reference to the cancel button that was clicked</param>
		/// <param name="e"></param>
		private void cancelButton_Clicked(object sender, EventArgs e)
		{
			// Doesn't hurt to re-check the prerequisites
			if(selectedEntities.Count == 1 && selectedEntities[0] is ConstructableEntity)
			{
				ConstructableEntity constructableEntity = (ConstructableEntity)selectedEntities[0];

				if (constructableEntity.IsUpgrading)
				{
					constructableEntity.CancelUpgrade();
				}
				else if (constructableEntity.IsConstructing)
				{
					constructableEntity.CancelConstruction();
				}

				refreshUpgradeButtons(constructableEntity);
			}
		}


		/// <summary>
		/// Draw this control
		/// </summary>
		/// <param name="spriteBatch">The spriteBatch to draw to</param>
		/// <param name="tint">The color used to tint this control. Use Color.White to draw this control normally</param>
		public override void Draw(SpriteBatch spriteBatch, Color tint)
		{
			if(selectedEntities == null || selectedEntities.Count == 0)
			{
				// Nothing selected, draw something
				// TODO: According to windows, should we even be drawing anything here?
				spriteBatch.FillRectangle(Rect, ColorPalette.ApplyTint(ColorPalette.ControlDark, tint));
			}
			else if(selectedEntities.Count == 1)
			{
				Entity entity = selectedEntities[0];

				// TODO: According to windows, should we even be drawing anything here?
				spriteBatch.FillRectangle(Rect, ColorPalette.ApplyTint(ColorPalette.ControlDark, tint));

				// Draw the unit's Picture:
				spriteBatch.FillRectangle(new Vector2(5, 5) + LocationAbs, new Vector2(90, 90), ColorPalette.ApplyTint(ColorPalette.ControlLight, tint));
				// TODO: Draw a picture of the unit

				// Draw the unit's name:
				spriteBatch.DrawString(Fonts.ControlFont, entity.Name, new Vector2(100, 5) + LocationAbs, ColorPalette.ApplyTint(ColorPalette.WindowText, tint), 0, Vector2.Zero, 1f, SpriteEffects.None, 0);

				// Each element will be drawn on one line
				List<String> infoLines = new List<string>();


				if(entity is Asteroid)
				{
					// TODO: Draw a bar that shows how many minerals this asteroid has
					infoLines.Add((int)((Asteroid)entity).GetMinerals() + " / " + ((Asteroid)entity).StartingMinerals + "  Minerals");
				}
				else if(entity is ConstructableEntity)
				{
					ConstructableEntity constructableEntity = (ConstructableEntity)entity;

					if (constructableEntity.IsConstructing)
					{
						// Show how far along the construction is
						infoLines.Add("Constructing  " + (constructableEntity.MineralsToConstruct - constructableEntity.MineralsLeftToConstruct) + " / " + constructableEntity.MineralsToConstruct);
					}
					else if (constructableEntity.IsUpgrading)
					{
						// Show how far along the upgrade is
						infoLines.Add("Upgrading  " + (constructableEntity.MineralsToUpgrade - constructableEntity.MineralsLeftToUpgrade) + " / " + constructableEntity.MineralsToUpgrade);
					}
					else
					{
						infoLines.Add("HP  " + (int)constructableEntity.HitPoints.Get() + " / " + constructableEntity.HitPoints.GetTotal());
						infoLines.Add("Level  " + constructableEntity.Level);

#if DEBUG
						infoLines.Add("Team: " + constructableEntity.OwningForce.Team);
#endif
					}
				}

				int drawX = 100;
				int drawY = 25;
				foreach(String line in infoLines)
				{
					spriteBatch.DrawString(Fonts.ControlFont,
					                       line,
					                       new Vector2(drawX, drawY) + LocationAbs,
										   ColorPalette.ApplyTint(ColorPalette.WindowText, tint),
					                       0,
					                       Vector2.Zero,
					                       1f,
					                       SpriteEffects.None,
					                       0);
					drawY += 16;
				}
			}

			base.Draw(spriteBatch, tint);
		}

	}
}