using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteroidOutpost.Components;
using AsteroidOutpost.Entities;
using AsteroidOutpost.Screens;
using C3.XNA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AsteroidOutpost.Systems
{
	class ConstructionSystem : DrawableGameComponent
	{
		private readonly World world;
		private SpriteBatch spriteBatch;

		private const float powerUsageRate = 12.0f;
		private const float mineralUsageRate = 30.0f;

		public ConstructionSystem(AOGame game, World world)
			: base(game)
		{
			spriteBatch = new SpriteBatch(game.GraphicsDevice);
			this.world = world;
		}


		//public override void Update(GameTime gameTime)
		//{
		//    List<Constructable> constructables = world.GetComponents<Constructable>();

		//    foreach (var constructable in constructables)
		//    {
		//        if(constructable.IsConstructing)
		//        {
		//            float powerToUse = powerUsageRate * (float)gameTime.ElapsedGameTime.TotalSeconds;
		//            float mineralsToUse = mineralUsageRate * (float)gameTime.ElapsedGameTime.TotalSeconds;

		//            // BUG: There is a disconnect between the check for minerals (below) and the actual consumption of minerals. Could cause weird behaviour
		//            Force owningForce = world.GetOwningForce(constructable.EntityID);
		//            if (owningForce.GetMinerals() > mineralsToUse && world.GetPowerGrid(owningForce).GetPower(constructable.EntityID, powerToUse))
		//            {
		//                // Use some minerals toward my construction
		//                int temp = (int)constructable.MineralsLeftToConstruct;
		//                constructable.MineralsLeftToConstruct -= mineralsToUse;

		//                // If the minerals left to construct has increased by a whole number, subtract it from the force's minerals
		//                if (temp > (int)constructable.MineralsLeftToConstruct)
		//                {
		//                    owningForce.SetMinerals(owningForce.GetMinerals() - (temp - (int)constructable.MineralsLeftToConstruct));
		//                }
		//            }
		//        }
		//    }

		//    base.Update(gameTime);
		//}

		
		
		
		/// <summary>
		/// Update this constructing building
		/// </summary>
		public override void Update(GameTime gameTime)
		{
			List<Constructable> constructables = world.GetComponents<Constructable>().Where(x => x.IsConstructing).ToList();

			foreach (var constructable in constructables)
			{
				float powerToUse = powerUsageRate * (float)gameTime.ElapsedGameTime.TotalSeconds;
				float mineralsToUse = mineralUsageRate * (float)gameTime.ElapsedGameTime.TotalSeconds;
				int delta;

				// Check that we have enough power in the grid
				Force owningForce = world.GetOwningForce(constructable);
				if (world.PowerGrid[owningForce.ID].HasPower(constructable.EntityID, powerToUse))
				{
					// Check to see if the mineralsLeftToConstruct would pass an integer boundary
					delta = (int)Math.Ceiling(constructable.MineralsLeftToConstruct) - (int)Math.Ceiling(constructable.MineralsLeftToConstruct - mineralsToUse);
					if (delta != 0)
					{
						// If the force doesn't have enough minerals, we will halt the construction here until it does 
						if (owningForce.GetMinerals() >= delta)
						{
							// Consume the resources
							world.PowerGrid[owningForce.ID].GetPower(constructable.EntityID, powerToUse);
							constructable.MineralsLeftToConstruct -= mineralsToUse;

							// Set the force's minerals
							owningForce.SetMinerals(owningForce.GetMinerals() - delta);
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
						constructable.MineralsLeftToConstruct -= mineralsToUse;

						// We should consume our little tidbit of power though:
						world.PowerGrid[owningForce.ID].GetPower(constructable.EntityID, powerToUse);
					}
				}
			}

		}


		/// <summary>
		/// Draw this constructing entity to the screen
		/// </summary>
		public override void Draw(GameTime gameTime)
		{
			List<Constructable> constructables = world.GetComponents<Constructable>().Where(x => x.IsConstructing || x.IsBeingPlaced).ToList();

			spriteBatch.Begin();

			foreach (var constructable in constructables)
			{
				Position position = world.GetComponent<Position>(constructable.EntityID);
				if (constructable.IsBeingPlaced)
				{
					spriteBatch.DrawEllipse(world.WorldToScreen(position.Center), world.Scale(PowerGrid.PowerConductingDistance), Color.White, world.HUD.DrawEllipseGuides);
				}
				else if (constructable.IsConstructing)
				{
					float percentComplete = (float)(constructable.MineralsToConstruct - constructable.MineralsLeftToConstruct) / constructable.MineralsToConstruct;

					// Draw a progress bar
					spriteBatch.FillRectangle(world.Scale(new Vector2(-position.Radius, position.Radius - 6)) + world.WorldToScreen(position.Center), world.Scale(new Vector2(position.Width, 6)), Color.Gray);
					spriteBatch.FillRectangle(world.Scale(new Vector2(-position.Radius, position.Radius - 6)) + world.WorldToScreen(position.Center), world.Scale(new Vector2(position.Width * percentComplete, 6)), Color.RoyalBlue);
				}
			}

			spriteBatch.End();
		}
	}
}
