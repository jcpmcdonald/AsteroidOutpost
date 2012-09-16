﻿using System;
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
	class PowerGridSystem : DrawableGameComponent
	{
		private World world;
		private SpriteBatch spriteBatch;


		public PowerGridSystem(AOGame game, World world)
			: base(game)
		{
			this.world = world;
			spriteBatch = new SpriteBatch(game.GraphicsDevice);
		}


		public override void Update(GameTime gameTime)
		{
			foreach(var producer in world.GetComponents<PowerProducer>())
			{
				if(producer.ProducesPower)
				{
					producer.AvailablePower += producer.PowerProductionRate * (float)gameTime.ElapsedGameTime.TotalSeconds;
				}
			}
		}



		public override void Draw(GameTime gameTime)
		{
			spriteBatch.Begin();

			// Draw any entities being placed
			IEnumerable<Constructable> placingEntities = world.GetComponents<Constructable>();
			placingEntities =  placingEntities.Where(x => x.IsBeingPlaced).ToList();
			foreach(var placingEntity in placingEntities)
			{
				PowerGridNode relatedPowerNode = world.GetComponent<PowerGridNode>(placingEntity);
				PowerGrid relatedGrid = world.GetPowerGrid(placingEntity);
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

					spriteBatch.DrawLine(world.WorldToScreen(relatedPowerNode.PowerLinkPointAbsolute),
					                     world.WorldToScreen(powerLink.Value.PowerLinkPointAbsolute),
					                     linkColor);
				}
			}


			foreach (var grid in world.PowerGrid.Values)
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

						spriteBatch.DrawLine(world.WorldToScreen(linkToDraw.Item1.PowerLinkPointAbsolute),
						                     world.WorldToScreen(linkToDraw.Item2.PowerLinkPointAbsolute),
						                     color);

						linksAlreadyDrawn.Add(linkToDraw);
						linksAlreadyDrawn.Add(new Tuple<PowerGridNode, PowerGridNode>(linkToDraw.Item2, linkToDraw.Item1));
					}
				}

				// Wipe the recent list
				grid.recentlyActiveLinks.Clear();


				foreach (var nodeA in grid.powerNodes.Keys)
				{
					foreach (var nodeB in grid.powerNodes[nodeA])
					{
						var linkToDraw = new Tuple<PowerGridNode, PowerGridNode>(nodeA, nodeB);
						if (!linksAlreadyDrawn.Contains(linkToDraw))
						{
							if (nodeA.PowerStateActive && nodeB.PowerStateActive)
							{
								color = new Color((int)(70 + world.Scale(50)), (int)(70 + world.Scale(50)), 0, (int)(70 + world.Scale(50)));
							}
							else
							{
								color = new Color((int)(80 + world.Scale(50)), (int)(0 + world.Scale(50)), 0, (int)(0 + world.Scale(50)));
							}

							spriteBatch.DrawLine(world.WorldToScreen(nodeA.PowerLinkPointAbsolute),
							                     world.WorldToScreen(nodeB.PowerLinkPointAbsolute),
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
