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
		private Texture2D powerBar;

		public PowerGridSystem(AOGame game, World world)
			: base(game)
		{
			this.world = world;
			spriteBatch = new SpriteBatch(game.GraphicsDevice);
		}


		protected override void LoadContent()
		{
			powerBar = Texture2DEx.FromStreamWithPremultAlphas(Game.GraphicsDevice, File.OpenRead(@"..\Content\PowerBar.png"));
			base.LoadContent();
		}


		public override void Update(GameTime gameTime)
		{
			if (world.Paused) { return; }

			foreach(var producer in world.GetComponents<PowerProducer>().Where(p => p.ProducesPower))
			{
				var constructible = world.GetNullableComponent<Constructible>(producer);
				if(constructible != null && (constructible.IsBeingPlaced || constructible.IsConstructing))
				{
					// Ignore placing and constructing producers
					continue;
				}
				producer.AvailablePower += producer.PowerProductionRate * (float)gameTime.ElapsedGameTime.TotalSeconds;
			}
		}



		public override void Draw(GameTime gameTime)
		{
			spriteBatch.Begin();


			// Draw power level bars
			foreach (var producer in world.GetComponents<PowerProducer>())
			{
				if(producer.ProducesPower)
				{
					Position producerPosition = world.GetComponent<Position>(producer);

					const float invisiblePoint = 1.5f;
					const float fadePoint = 1.2f;
					if(world.ScaleFactor < invisiblePoint)
					{
						// Default to completely visible
						float fadePercent = 0.0f;
						if(world.ScaleFactor > fadePoint)
						{
							// Fade out as we get further away
							fadePercent = (world.ScaleFactor - fadePoint) / (invisiblePoint - fadePoint);
						}

						float percentFull = producer.AvailablePower / producer.MaxPower;
						float scale = 0.4f;
						int fillToHeight = (int)((powerBar.Height * percentFull) + 0.5f);

						// Draw the depleted part of the bar
						spriteBatch.Draw(powerBar,
						                 world.WorldToScreen(new Vector2(producerPosition.Left + 10,
						                                                 producerPosition.Top + 5)),
						                 new Rectangle(0,
						                               0,
						                               powerBar.Width,
						                               powerBar.Height - fillToHeight),
						                 Color.White * (1 - fadePercent),
						                 0f,
						                 Vector2.Zero,
						                 world.Scale(scale),
						                 SpriteEffects.None,
						                 0);

						// Draw the available part of the bar
						spriteBatch.Draw(powerBar,
						                 world.WorldToScreen(new Vector2(producerPosition.Left + 10,
						                                                 producerPosition.Top + 5 + ((powerBar.Height - fillToHeight) * scale))),
						                 new Rectangle(0,
						                               powerBar.Height - fillToHeight,
						                               powerBar.Width,
						                               fillToHeight),
						                 Color.Green * (1 - fadePercent),
						                 0f,
						                 Vector2.Zero,
						                 world.Scale(scale),
						                 SpriteEffects.None,
						                 0);
					}
				}
			}


			// Draw any entities being placed
			IEnumerable<Constructible> placingEntities = world.GetComponents<Constructible>();
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
