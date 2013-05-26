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

		public event Action<int> ConstructionCompletedEvent;

		public ConstructionSystem(AOGame game, World world)
			: base(game)
		{
			spriteBatch = new SpriteBatch(game.GraphicsDevice);
			this.world = world;
		}

		
		
		
		/// <summary>
		/// Update this constructing building
		/// </summary>
		public override void Update(GameTime gameTime)
		{
			var constructables = world.GetComponents<Constructible>().Where(x => x.IsConstructing);

			foreach (var constructable in constructables)
			{
				float powerToUse = powerUsageRate * (float)gameTime.ElapsedGameTime.TotalSeconds;
				float mineralsToUse = mineralUsageRate * (float)gameTime.ElapsedGameTime.TotalSeconds;
				int deltaMinerals;

				// Check that we have enough power in the grid
				Force owningForce = world.GetOwningForce(constructable);
				if (world.PowerGrid[owningForce.ID].HasPower(constructable.EntityID, powerToUse))
				{
					// Check to see if the mineralsLeftToConstruct would pass an integer boundary
					deltaMinerals = (int)(constructable.MineralsConstructed + mineralsToUse) - (int)(constructable.MineralsConstructed);
					if (deltaMinerals != 0)
					{
						// If the force doesn't have enough minerals, we will halt the construction here until it does 
						if (owningForce.GetMinerals() >= deltaMinerals)
						{
							// Consume the resources
							world.PowerGrid[owningForce.ID].GetPower(constructable.EntityID, powerToUse);
							constructable.MineralsConstructed += mineralsToUse;

							// Set the force's minerals
							owningForce.SetMinerals(owningForce.GetMinerals() - deltaMinerals);

							if (constructable.MineralsConstructed >= constructable.MineralsToConstruct)
							{
								// This construction is complete
								constructable.MineralsConstructed = constructable.MineralsToConstruct;
								constructable.IsConstructing = false;

								if (ConstructionCompletedEvent != null)
								{
									ConstructionCompletedEvent(constructable.EntityID);
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
						constructable.MineralsConstructed += mineralsToUse;

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
			List<Constructible> constructables = world.GetComponents<Constructible>().Where(x => x.IsConstructing || x.IsBeingPlaced).ToList();

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
					float percentComplete = constructable.MineralsConstructed / constructable.MineralsToConstruct;

					// Draw a progress bar
					spriteBatch.FillRectangle(world.Scale(new Vector2(-position.Radius, position.Radius - 6)) + world.WorldToScreen(position.Center), world.Scale(new Vector2(position.Width, 6)), Color.Gray);
					spriteBatch.FillRectangle(world.Scale(new Vector2(-position.Radius, position.Radius - 6)) + world.WorldToScreen(position.Center), world.Scale(new Vector2(position.Width * percentComplete, 6)), Color.RoyalBlue);
				}
			}

			spriteBatch.End();
		}
	}
}
