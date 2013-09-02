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
		private SpriteBatch spriteBatch;
		private readonly PowerGridSystem powerGridSystem;

		private const float powerUsageRate = 12.0f;
		private const float mineralUsageRate = 30.0f;

		public event Action<int> UpgradeCompletedEvent;

		public UpgradeSystem(AOGame game, World world, PowerGridSystem powerGridSystem)
			: base(game)
		{
			spriteBatch = new SpriteBatch(game.GraphicsDevice);
			this.world = world;
			this.powerGridSystem = powerGridSystem;
		}


		public override void Update(GameTime gameTime)
		{
			if (world.Paused) { return; }

			var upgradables = world.GetComponents<Upgradable>().Where(x => x.IsUpgrading && x.CurrentUpgrade != null);

			foreach (var upgradable in upgradables)
			{
				float powerToUse = powerUsageRate * (float)gameTime.ElapsedGameTime.TotalSeconds;
				float mineralsToUse = mineralUsageRate * (float)gameTime.ElapsedGameTime.TotalSeconds;
				int deltaMinerals;
					
				Force owningForce = world.GetOwningForce(upgradable);
				if (powerGridSystem.HasPower(upgradable, powerToUse))
				{
					// Check to see if the mineralsLeftToConstruct would pass an integer boundary
					deltaMinerals = (int)(upgradable.MineralsUpgraded + mineralsToUse) - (int)(upgradable.MineralsUpgraded);
					if (deltaMinerals != 0)
					{
						// If the force doesn't have enough minerals, we will halt the construction here until it does 
						if (owningForce.GetMinerals() >= deltaMinerals)
						{
							// Consume the resources
							powerGridSystem.GetPower(upgradable, powerToUse);
							upgradable.MineralsUpgraded += mineralsToUse;

							// Set the force's minerals
							owningForce.SetMinerals(owningForce.GetMinerals() - deltaMinerals);

							if (upgradable.MineralsUpgraded >= upgradable.MineralsToUpgrade)
							{
								// This construction is complete
								upgradable.MineralsUpgraded = upgradable.MineralsToUpgrade;
								upgradable.IsUpgrading = false;

								if (UpgradeCompletedEvent != null)
								{
									UpgradeCompletedEvent(upgradable.EntityID);
								}
							}
						}
						else
						{
							// Construction Halts, no progress, no consumption
						}
					}
					else
					{
						// We have not passed an integer boundary, so just keep track of the change locally
						// We'll get around to subtracting this from the force's minerals when we pass an integer boundary
						upgradable.MineralsUpgraded += mineralsToUse;

						// We should consume our little tidbit of power though:
						powerGridSystem.GetPower(upgradable, powerToUse);
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
					float percentComplete = upgradable.MineralsUpgraded / upgradable.CurrentUpgrade.MineralCost;
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
