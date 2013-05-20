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

		private SpriteBatch spriteBatch;

		public UpgradeSystem(AOGame game, World world)
			: base(game)
		{
			spriteBatch = new SpriteBatch(game.GraphicsDevice);
			this.world = world;
		}


		public override void Update(GameTime gameTime)
		{
			List<Upgradable> upgradables = (List<Upgradable>)world.GetComponents<Upgradable>().Where(x => x.IsUpgrading && x.CurrentUpgrade != null);

			foreach (var upgradable in upgradables)
			{
				if(upgradable.IsUpgrading && upgradable.CurrentUpgrade != null)
				{
					float powerToUse = powerUsageRate * (float)gameTime.ElapsedGameTime.TotalSeconds;
					float mineralsToUse = mineralUsageRate * (float)gameTime.ElapsedGameTime.TotalSeconds;

					// BUG: There is a disconnect between the check for minerals (below) and the actual consumption of minerals. Could cause weird behaviour
					Force owningForce = world.GetOwningForce(upgradable);
					if (owningForce.GetMinerals() > mineralsToUse && world.GetPowerGrid(owningForce).GetPower(upgradable.EntityID, powerToUse))
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
		public override void Draw(GameTime gameTime)
		{
			spriteBatch.Begin();

			foreach (var upgradable in world.GetComponents<Upgradable>())
			{
				if (upgradable.IsUpgrading)
				{
					float percentComplete = (float)((upgradable.CurrentUpgrade.MineralCost - upgradable.MineralsLeftToUpgrade) / upgradable.CurrentUpgrade.MineralCost);
					Position position = world.GetComponent<Position>(upgradable.EntityID);

					// Draw a progress bar
					spriteBatch.FillRectangle(world.Scale(new Vector2(-position.Radius, position.Radius - 6)) + world.WorldToScreen(position.Center), world.Scale(new Vector2(position.Width, 6)), Color.Gray);
					spriteBatch.FillRectangle(world.Scale(new Vector2(-position.Radius, position.Radius - 6)) + world.WorldToScreen(position.Center), world.Scale(new Vector2(position.Width * percentComplete, 6)), Color.RoyalBlue);
				}
			}

			spriteBatch.End();

			base.Draw(gameTime);
		}
	}
}
