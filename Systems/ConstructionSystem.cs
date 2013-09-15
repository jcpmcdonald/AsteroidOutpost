using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using AsteroidOutpost.Components;
using AsteroidOutpost.Entities;
using AsteroidOutpost.Entities.Eventing;
using AsteroidOutpost.Eventing;
using AsteroidOutpost.Interfaces;
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
		private readonly PowerGridSystem powerGridSystem;

		private const float powerUsageRate = 12.0f;
		private const float mineralUsageRate = 30.0f;

		public event Action<ConstructionCompleteEventArgs> AnyConstructionCompletedEvent;
		public event Action<UpgradeCompleteEventArgs> AnyUpgradeCompletedEvent;

		public ConstructionSystem(AOGame game, World world, PowerGridSystem powerGridSystem)
			: base(game)
		{
			spriteBatch = new SpriteBatch(game.GraphicsDevice);
			this.world = world;
			this.powerGridSystem = powerGridSystem;
		}


		/// <summary>
		/// Update this constructing building
		/// </summary>
		public override void Update(GameTime gameTime)
		{
			if (world.Paused) { return; }

			List<IConstructible> constructibles = new List<IConstructible>();
			constructibles.AddRange(world.GetComponents<Constructing>());
			constructibles.AddRange(world.GetComponents<Upgrading>());

			foreach (var constructible in constructibles)
			{
				if(constructible is Constructing && ((Constructing)constructible).IsBeingPlaced)
				{
					continue;
				}

				float powerToUse = powerUsageRate * (float)gameTime.ElapsedGameTime.TotalSeconds;
				float mineralsToUse = mineralUsageRate * (float)gameTime.ElapsedGameTime.TotalSeconds;
				int deltaMinerals;

				// Check that we have enough power in the grid
				Force owningForce = world.GetOwningForce(constructible as Component);
				if (powerGridSystem.HasPower(constructible as Component, powerToUse))
				{
					// Check to see if the mineralsLeftToConstruct would pass an integer boundary
					deltaMinerals = (int)(constructible.MineralsConstructed + mineralsToUse) - (int)(constructible.MineralsConstructed);
					if (deltaMinerals != 0)
					{
						// If the force doesn't have enough minerals, we will halt the construction here until it does 
						if (owningForce.GetMinerals() >= deltaMinerals)
						{
							// Consume the resources
							powerGridSystem.GetPower(constructible as Component, powerToUse);
							constructible.MineralsConstructed += mineralsToUse;

							// Set the force's minerals
							owningForce.SetMinerals(owningForce.GetMinerals() - deltaMinerals);

							if (constructible.MineralsConstructed >= constructible.Cost)
							{
								// This construction is complete
								constructible.MineralsConstructed = constructible.Cost;

								OnConstructionComplete(constructible);

								world.DeleteComponent(constructible as Component);
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
						constructible.MineralsConstructed += mineralsToUse;

						// We should consume our little tidbit of power though:
						powerGridSystem.GetPower(constructible as Component, powerToUse);
					}
				}
			}

		}


		private void OnConstructionComplete(IConstructible constructible)
		{
			Constructing constructing = constructible as Constructing;
			if(constructing != null)
			{
				constructing.OnConstructionComplete(new ConstructionCompleteEventArgs(constructing));

				if (AnyConstructionCompletedEvent != null)
				{
					AnyConstructionCompletedEvent(new ConstructionCompleteEventArgs(constructing));
				}
			}
			else
			{
				Upgrading upgrading = constructible as Upgrading;
				if(upgrading != null)
				{
					world.UpgradeComplete(upgrading.EntityID);

					upgrading.OnUpgradeComplete(new UpgradeCompleteEventArgs(upgrading));

					if (AnyConstructionCompletedEvent != null)
					{
						AnyUpgradeCompletedEvent(new UpgradeCompleteEventArgs(upgrading));
					}
				}
			}

			
		}


		/// <summary>
		/// Draw this constructing entity to the screen
		/// </summary>
		public override void Draw(GameTime gameTime)
		{
			var constructables = world.GetComponents<Constructing>();

			spriteBatch.Begin();

			foreach (var constructable in constructables)
			{
				Position position = world.GetComponent<Position>(constructable.EntityID);
				if (constructable.IsBeingPlaced)
				{
					spriteBatch.DrawEllipse(world.WorldToScreen(position.Center), world.Scale(PowerGrid.PowerConductingDistance), Color.White);
				}
				else
				{
					float percentComplete = constructable.MineralsConstructed / constructable.Cost;

					// Draw a progress bar
					spriteBatch.FillRectangle(world.Scale(new Vector2(-position.Radius, position.Radius - 6)) + world.WorldToScreen(position.Center), world.Scale(new Vector2(position.Width, 6)), Color.Gray);
					spriteBatch.FillRectangle(world.Scale(new Vector2(-position.Radius, position.Radius - 6)) + world.WorldToScreen(position.Center), world.Scale(new Vector2(position.Width * percentComplete, 6)), Color.RoyalBlue);
				}
			}

			spriteBatch.End();
		}
	}
}
