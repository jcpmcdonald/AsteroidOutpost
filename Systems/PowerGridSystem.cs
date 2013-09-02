using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using AsteroidOutpost.Components;
using AsteroidOutpost.Screens;
using C3.XNA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AsteroidOutpost.Systems
{
	class PowerGridSystem : DrawableGameComponent
	{
		private World world;
		private SpriteBatch spriteBatch;

		private readonly Dictionary<int, PowerGrid> powerGrid = new Dictionary<int, PowerGrid>(4);

		public PowerGridSystem(AOGame game, World world)
			: base(game)
		{
			this.world = world;
			spriteBatch = new SpriteBatch(game.GraphicsDevice);
		}


		public void ConnectToPowerGrid(PowerGridNode powerNode)
		{
			GetPowerGrid(powerNode).ConnectToPowerGrid(powerNode);
		}


		public void Disconnect(PowerGridNode node)
		{
			GetPowerGrid(node).Disconnect(node);
		}


		internal PowerGrid GetPowerGrid(Component componentInForce)
		{
			return GetPowerGrid(world.GetOwningForce(componentInForce));
		}

		internal PowerGrid GetPowerGrid(Force force)
		{
			if(!powerGrid.ContainsKey(force.ID))
			{
				powerGrid.Add(force.ID, new PowerGrid(world));
			}
			return powerGrid[force.ID];
		}


		/// <summary>
		/// Checks that the requested amount of power is available from a single power-source in the grid
		/// </summary>
		/// <param name="referenceComponent">Where in the power grid should we begin our quest for power</param>
		/// <param name="amount">The amount of power to retrieve</param>
		/// <returns>Returns true if successful, false otherwise</returns>
		public bool HasPower(Component referenceComponent, float amount)
		{
			return GetPowerGrid(referenceComponent).HasPower(referenceComponent.EntityID, amount);
		}


		/// <summary>
		/// Gets the requested amount of power from a single power-source that is able to provide it
		/// </summary>
		/// <param name="referenceComponent">Where in the power grid should we begin our quest for power</param>
		/// <param name="amount">The amount of power to retrieve</param>
		/// <returns>Returns true if successful, false otherwise</returns>
		public bool GetPower(Component referenceComponent, float amount)
		{
			return GetPowerGrid(referenceComponent).GetPower(referenceComponent.EntityID, amount);
		}


		public void PutPower(Component referenceComponent, float amount)
		{
			GetPowerGrid(referenceComponent).PutPower(referenceComponent.EntityID, amount);
		}


		public override void Draw(GameTime gameTime)
		{
			spriteBatch.Begin();


			// Draw any entities being placed
			IEnumerable<Constructible> placingEntities = world.GetComponents<Constructible>();
			placingEntities =  placingEntities.Where(x => x.IsBeingPlaced).ToList();
			foreach(var placingEntity in placingEntities)
			{
				PowerGridNode relatedPowerNode = world.GetComponent<PowerGridNode>(placingEntity);
				PowerGrid relatedGrid = GetPowerGrid(placingEntity);
				foreach (var powerLink in relatedGrid.GetAllPowerLinks(relatedPowerNode))
				{
					Color linkColor;
					if (relatedGrid.IsPowerRoutableBetween(relatedPowerNode, powerLink.Value))
					{
						linkColor = Color.Yellow;
					}
					else
					{
						linkColor = Color.Red;
					}

					spriteBatch.DrawLine(world.WorldToScreen(relatedPowerNode.PowerLinkPointAbsolute(world)),
					                     world.WorldToScreen(powerLink.Value.PowerLinkPointAbsolute(world)),
					                     linkColor);
				}
			}


			foreach (var grid in powerGrid.Values)
			{
				Color color;

				// TODO: This allocates a bunch of memory each draw, fix this!
				List<Tuple<PowerGridNode, PowerGridNode>> linksAlreadyDrawn = new List<Tuple<PowerGridNode, PowerGridNode>>(grid.powerNodes.Count * 6);

				// Draw all of the active links first
				foreach (var linkToDraw in grid.recentlyActiveLinks)
				{
					if (!linksAlreadyDrawn.Contains(linkToDraw))
					{
						color = new Color((int)(150 + world.Scale(50)), (int)(150 + world.Scale(50)), 0, (int)(150 + world.Scale(50)));

						spriteBatch.DrawLine(world.WorldToScreen(linkToDraw.Item1.PowerLinkPointAbsolute(world)),
						                     world.WorldToScreen(linkToDraw.Item2.PowerLinkPointAbsolute(world)),
						                     color);

						linksAlreadyDrawn.Add(linkToDraw);
						linksAlreadyDrawn.Add(new Tuple<PowerGridNode, PowerGridNode>(linkToDraw.Item2, linkToDraw.Item1));
					}
				}

				// Wipe the recent list
				grid.recentlyActiveLinks.Clear();


				foreach (var nodeA in grid.powerNodes.Keys)
				{
					Constructible constructibleA = world.GetNullableComponent<Constructible>(nodeA);

					foreach (var nodeB in grid.powerNodes[nodeA])
					{
						Constructible constructibleB = world.GetNullableComponent<Constructible>(nodeB);
						var linkToDraw = new Tuple<PowerGridNode, PowerGridNode>(nodeA, nodeB);
						if (!linksAlreadyDrawn.Contains(linkToDraw))
						{
							//if (nodeA.IsPowerStateActive(world) && nodeB.IsPowerStateActive(world))
							if((constructibleA != null && !constructibleA.IsBeingPlaced) || (constructibleB != null && !constructibleB.IsBeingPlaced))
							{
								// Draw a normal, yellow line
								color = new Color((int)(70 + world.Scale(50)), (int)(70 + world.Scale(50)), 0, (int)(70 + world.Scale(50)));
							}
							else
							{
								// Draw a red line to indicate that you can't connect
								color = new Color((int)(80 + world.Scale(50)), (int)(0 + world.Scale(50)), 0, (int)(0 + world.Scale(50)));
							}

							spriteBatch.DrawLine(world.WorldToScreen(nodeA.PowerLinkPointAbsolute(world)),
							                     world.WorldToScreen(nodeB.PowerLinkPointAbsolute(world)),
							                     color);

							linksAlreadyDrawn.Add(linkToDraw);
							linksAlreadyDrawn.Add(new Tuple<PowerGridNode, PowerGridNode>(nodeB, nodeA));
						}
					}
				}
			}

			spriteBatch.End();
			base.Draw(gameTime);
		}
	}
}
