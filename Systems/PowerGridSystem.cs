using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AsteroidOutpost.Components;
using AsteroidOutpost.Extensions;
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
		private Texture2D powerStarvedTexture;
		private Texture2D powerLineTexture;

		private readonly Dictionary<int, PowerGrid> powerGrid = new Dictionary<int, PowerGrid>(4);

		public PowerGridSystem(AOGame game, World world)
			: base(game)
		{
			this.world = world;
			spriteBatch = new SpriteBatch(game.GraphicsDevice);
		}

		protected override void LoadContent()
		{
			base.LoadContent();
			powerStarvedTexture = Texture2D.FromStream(Game.GraphicsDevice, File.OpenRead(@"..\data\images\NoPowerSymbol.png"));
			powerLineTexture = Texture2DEx.FromStreamWithPremultAlphas(Game.GraphicsDevice, File.OpenRead(@"..\data\images\WhitePowerBeam.png"));
			
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
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);


			// Draw any entities being placed
			foreach(var placingEntity in world.GetComponents<Constructing>().Where(x => x.IsBeingPlaced))
			{
				PowerGridNode relatedPowerNode = world.GetComponent<PowerGridNode>(placingEntity);
				PowerGrid relatedGrid = GetPowerGrid(placingEntity);
				foreach (var powerLink in relatedGrid.GetAllPowerLinks(relatedPowerNode))
				{
					Color linkColor;
					if (relatedGrid.IsPowerRoutableBetween(relatedPowerNode, powerLink.Value))
					{
						linkColor = Color.Blue;
					}
					else
					{
						linkColor = Color.Red;
					}

					//spriteBatch.DrawLine(world.WorldToScreen(relatedPowerNode.PowerLinkPointAbsolute(world)),
					//                     world.WorldToScreen(powerLink.Value.PowerLinkPointAbsolute(world)),
					//                     linkColor);

					DrawPowerLine(spriteBatch,
					              world.WorldToScreen(relatedPowerNode.PowerLinkPointAbsolute(world)),
					              world.WorldToScreen(powerLink.Value.PowerLinkPointAbsolute(world)),
					              linkColor);
				}
			}


			// Draw power symbols above structures not getting enough power
			foreach (PowerGrid grid in powerGrid.Values)
			{
				foreach (PowerGridNode starvedNode in grid.powerNodes.Keys.Where(x => x.PowerStarved))
				{
					Position position = world.GetComponent<Position>(starvedNode);
					spriteBatch.Draw(powerStarvedTexture,
					                 world.WorldToScreen(position.Center),
					                 null,
					                 Color.White,
					                 0f,
					                 new Vector2(powerStarvedTexture.Width / 2f, powerStarvedTexture.Height / 2f), 
					                 1f / world.ScaleFactor,
					                 SpriteEffects.None,
					                 0);
				}
			}


			// Draw all power links
			foreach (var grid in powerGrid.Values)
			{
				Color color;

				// TODO: This allocates a bunch of memory each draw, fix this!
				List<Tuple<PowerGridNode, PowerGridNode>> linksAlreadyDrawn = new List<Tuple<PowerGridNode, PowerGridNode>>(grid.powerNodes.Count * 6);

				// Draw all of the active links first
				foreach (var linkToDraw in grid.recentlyActiveLinks)
				{
					if (!linksAlreadyDrawn.Contains(linkToDraw.Key))
					{
						//color = new Color((int)(150 + world.Scale(50)), (int)(150 + world.Scale(50)), 0, (int)(150 + world.Scale(50)));
						color = new Color(50, 50, (int)(200 + world.Scale(50)));

						//spriteBatch.DrawLine(world.WorldToScreen(linkToDraw.Item1.PowerLinkPointAbsolute(world)),
						//                     world.WorldToScreen(linkToDraw.Item2.PowerLinkPointAbsolute(world)),
						//                     color);
						DrawPowerLine(spriteBatch,
						              world.WorldToScreen(linkToDraw.Key.Item1.PowerLinkPointAbsolute(world)),
						              world.WorldToScreen(linkToDraw.Key.Item2.PowerLinkPointAbsolute(world)),
						              color,
									  1f + linkToDraw.Value);

						linksAlreadyDrawn.Add(linkToDraw.Key);
						linksAlreadyDrawn.Add(new Tuple<PowerGridNode, PowerGridNode>(linkToDraw.Key.Item2, linkToDraw.Key.Item1));
					}
				}

				// Wipe the recent list
				grid.recentlyActiveLinks.Clear();


				// Draw all remaining power links
				foreach (var nodeA in grid.powerNodes.Keys)
				{
					Constructing constructingA = world.GetNullableComponent<Constructing>(nodeA);

					foreach (var nodeB in grid.powerNodes[nodeA])
					{
						Constructing constructingB = world.GetNullableComponent<Constructing>(nodeB);
						var linkToDraw = new Tuple<PowerGridNode, PowerGridNode>(nodeA, nodeB);
						if (!linksAlreadyDrawn.Contains(linkToDraw))
						{
							//if (nodeA.IsPowerStateActive(world) && nodeB.IsPowerStateActive(world))
							if((constructingA != null && !constructingA.IsBeingPlaced) || (constructingB != null && !constructingB.IsBeingPlaced))
							{
								// Draw a normal line
								//color = new Color((int)(70 + world.Scale(50)), (int)(70 + world.Scale(50)), 0, (int)(70 + world.Scale(50)));
								color = new Color(0, 0, (int)(150 + world.Scale(50)));
							}
							else
							{
								// Draw a red line to indicate that you can't connect
								//color = new Color((int)(80 + world.Scale(50)), (int)(0 + world.Scale(50)), 0, (int)(0 + world.Scale(50)));
								color = new Color(0, 0, (int)(150 + world.Scale(50)));
							}

							//spriteBatch.DrawLine(world.WorldToScreen(nodeA.PowerLinkPointAbsolute(world)),
							//                     world.WorldToScreen(nodeB.PowerLinkPointAbsolute(world)),
							//                     color);
							DrawPowerLine(spriteBatch,
							              world.WorldToScreen(nodeA.PowerLinkPointAbsolute(world)),
							              world.WorldToScreen(nodeB.PowerLinkPointAbsolute(world)),
							              color);

							linksAlreadyDrawn.Add(linkToDraw);
							linksAlreadyDrawn.Add(new Tuple<PowerGridNode, PowerGridNode>(nodeB, nodeA));
						}
					}

					// Reset power starving on each power node
					nodeA.PowerStarved = false;
				}
			}


			spriteBatch.End();
			base.Draw(gameTime);
		}


		private void DrawPowerLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, float flow = 1)
		{
			//spriteBatch.Draw(powerLineTexture, start, null, Color.White,
			//                 (float)Math.Atan2(end.Y - start.Y, end.X - start.X),
			//                 new Vector2(0f, (float)powerLineTexture.Height / 2),
			//                 new Vector2(Vector2.Distance(start, end) / powerLineTexture.Width, world.Scale(1f)),
			//                 SpriteEffects.None, 0f);

			List<Vector2> points = MakeLightning(new List<Vector2> { start, end }, 3, world.Scale(MathHelper.Max(1.5f - flow, 0)));

			for (int i = 0; i < points.Count - 1; i++)
			{
				//spriteBatch.DrawLine(points[i], points[i + 1], new Color(200, 200, 255, 0), world.Scale(5 + GlobalRandom.Next(0f, 0.5f)));
				//spriteBatch.DrawLine(points[i], points[i + 1], new Color(100, 100, 200, 0), world.Scale(2 + GlobalRandom.Next(0f, 0.5f)));
				//spriteBatch.DrawLine(points[i], points[i + 1], Color.Blue, world.Scale(1));

				spriteBatch.Draw(powerLineTexture,
				                 points[i],
				                 null,
				                 color * flow,
				                 (float)Math.Atan2(points[i + 1].Y - points[i].Y,
				                                   points[i + 1].X - points[i].X),
				                 new Vector2(0f, (float)powerLineTexture.Height / 2),
				                 new Vector2(Vector2.Distance(points[i], points[i + 1]) / powerLineTexture.Width, world.Scale(0.25f * Math.Min(4, flow * flow))),
				                 SpriteEffects.None,
				                 0f);
			}
		}


		private List<Vector2> MakeLightning(List<Vector2> points, int itterations, float noise)
		{
			if(itterations <= 0) { return points; }
			if (noise <= 0){ return points; }

			for (int i = points.Count - 1; i >= 1; i--)
			{
				Vector2 segment = points[i - 1] - points[i];
				Vector2 perpendicular = new Vector2(segment.Y, -segment.X);

				Vector2 midPoint = (points[i - 1] + points[i]) / 2f;

				midPoint += Vector2.Normalize(perpendicular) * GlobalRandom.Next(-noise, noise);

				points.Insert(i, midPoint);
				i--;
			}

			return MakeLightning(points, itterations - 1, noise);
		}
	}
}
