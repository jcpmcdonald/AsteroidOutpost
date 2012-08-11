using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteroidOutpost.Components;
using AsteroidOutpost.Screens;
using C3.XNA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AsteroidOutpost.Systems
{
	class UpgradeSystem : DrawableGameComponent
	{
		private readonly World world;

		private const float powerUsageRate = 12.0f;
		private const float mineralUsageRate = 30.0f;

		public UpgradeSystem(AOGame game, World world)
			: base(game)
		{
			
			this.world = world;
		}

		public override void Update(GameTime gameTime)
		{
			List<Upgradable> upgradables = world.GetComponents<Upgradable>();

			foreach (var upgradable in upgradables)
			{
				if(upgradable.IsUpgrading && upgradable.CurrentUpgrade != null)
				{
					float powerToUse = powerUsageRate * (float)gameTime.ElapsedGameTime.TotalSeconds;
					float mineralsToUse = mineralUsageRate * (float)gameTime.ElapsedGameTime.TotalSeconds;

					// BUG: There is a disconnect between the check for minerals (below) and the actual consumption of minerals. Could cause weird behaviour
					Force owningForce = world.GetOwningForce(upgradable.EntityID);
					if (owningForce.GetMinerals() > mineralsToUse && world.PowerGrid[owningForce.ID].GetPower(upgradable.EntityID, powerToUse))
					{
						// Use some minerals toward my upgrade
						int temp = (int)upgradable.MineralsLeftToUpgrade;
						upgradable.SetMineralsLeftToUpgrade(upgradable.MineralsLeftToUpgrade - mineralsToUse);

						// If the minerals left to construct has increased by a whole number, subtract it from the force's minerals
						if (temp > (int)upgradable.MineralsLeftToUpgrade)
						{
							owningForce.SetMinerals(owningForce.GetMinerals() - (temp - (int)upgradable.MineralsLeftToUpgrade));
						}
					}
				}
			}

			base.Update(gameTime);
		}

		/// <summary>
		/// Draw this upgrading entity to the screen
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="scaleModifier"></param>
		/// <param name="tint"></param>
		protected void DrawUpgrading(SpriteBatch spriteBatch, float scaleModifier, Color tint)
		{
			float percentComplete = (float)((currentUpgrade.MineralCost - mineralsLeftToUpgrade) / currentUpgrade.MineralCost);

			base.Draw(spriteBatch, scaleModifier, tint);
			
			// Draw a progress bar
			//Vector2 selfCenterOnScreen = new Vector2(Center.X - world.Hud.FocusScreen.X, Center.Y - world.Hud.FocusScreen.Y);
			spriteBatch.FillRectangle(world.Scale(new Vector2(-Position.Radius, Position.Radius - 6)) + world.WorldToScreen(Position.Center), world.Scale(new Vector2(Position.Width, 6)), Color.Gray);
			spriteBatch.FillRectangle(world.Scale(new Vector2(-Position.Radius, Position.Radius - 6)) + world.WorldToScreen(Position.Center), world.Scale(new Vector2(Position.Width * percentComplete, 6)), Color.RoyalBlue);
			
			//DrawPowerConnections(spriteBatch);
		}
	}
}
