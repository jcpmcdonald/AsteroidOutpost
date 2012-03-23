﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteroidOutpost.Components;
using AsteroidOutpost.Entities;
using AsteroidOutpost.Entities.Eventing;
using AsteroidOutpost.Entities.Structures;
using AsteroidOutpost.Interfaces;
using AsteroidOutpost.Screens;
using C3.XNA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AsteroidOutpost
{
	public class PowerGrid
	{
		private AsteroidOutpostScreen theGame;

		public const int PowerConductingDistance = 220;
		private readonly Dictionary<IPowerGridNode, List<IPowerGridNode>> powerNodes = new Dictionary<IPowerGridNode, List<IPowerGridNode>>(32);

		private List<Tuple<IPowerGridNode, IPowerGridNode>> recentlyActiveLinks = new List<Tuple<IPowerGridNode, IPowerGridNode>>();


		public PowerGrid(AsteroidOutpostScreen theGame)
		{
			this.theGame = theGame;
		}


		internal List<KeyValuePair<float, IPowerGridNode>> GetAllPowerLinks(IPowerGridNode node)
		{
			var allPowerLinks = new List<KeyValuePair<float, IPowerGridNode>>(10);

			foreach (var powerNode in powerNodes.Keys)
			{
				if (powerNode != node)
				{
					// Note: Should I actually be basing power-distance on the power link location considering that the power link location is just to represent a 3D location in a 2D world?
					float distance = Vector2.Distance(powerNode.PowerLinkPointAbsolute, node.PowerLinkPointAbsolute);
					if (distance <= PowerConductingDistance)
					{
						allPowerLinks.Add(new KeyValuePair<float, IPowerGridNode>(distance, powerNode));
					}
				}
			}


			if(node.ConductsPower)
			{
				return new List<KeyValuePair<float, IPowerGridNode>>(allPowerLinks.Where(n => n.Value.ConductsPower || powerNodes[n.Value].Count == 0));
			}
			else
			{
				// Return the closest power conductor IF there's a valid connection
				// Else return all bad connections
				allPowerLinks.Sort((lhs, rhs) => lhs.Key.CompareTo(rhs.Key));
				KeyValuePair<float, IPowerGridNode> theOne = allPowerLinks.FirstOrDefault(n => n.Value.ConductsPower && IsPowerRoutableBetween(node, n.Value));
				if(theOne.Equals(default(KeyValuePair<float, IPowerGridNode>)))
				{

					return new List<KeyValuePair<float, IPowerGridNode>>(allPowerLinks.Where(n => n.Value.ConductsPower));
				}

				return new List<KeyValuePair<float, IPowerGridNode>>(1) { new KeyValuePair<float, IPowerGridNode>(theOne.Key, theOne.Value) };
			}
		}


		
		internal void ConnectToPowerGrid(IPowerGridNode newNode)
		{
			powerNodes.Add(newNode, new List<IPowerGridNode>(6));
			newNode.DyingEvent += NodeDying;

			List<KeyValuePair<float, IPowerGridNode>> allPowerLinks = GetAllPowerLinks(newNode);

			if(newNode.ConductsPower)
			{
				// Connect to all of the other conductors
				foreach (var node in allPowerLinks.Where(n => n.Value.ConductsPower && IsPowerRoutableBetween(newNode, n.Value)))
				{
					Connect(newNode, node.Value);
				}

				// And connect to any lonely non-conductors
				foreach (var node in allPowerLinks.Where(n => !n.Value.ConductsPower && IsPowerRoutableBetween(newNode, n.Value) && powerNodes[n.Value].Count == 0))
				{
					Connect(newNode, node.Value);
				}
			}
			else
			{
				// Only connect to the closest power source
				IPowerGridNode node = allPowerLinks.FirstOrDefault(n => n.Value != null && n.Value.ConductsPower && IsPowerRoutableBetween(newNode, n.Value)).Value;
				if(node != null)
				{
					Connect(newNode, node);
				}
			}
		}


		private void Connect(IPowerGridNode nodeA, IPowerGridNode nodeB)
		{
			if (!powerNodes[nodeA].Contains(nodeB))
			{
				powerNodes[nodeA].Add(nodeB);
			}
			if (!powerNodes[nodeB].Contains(nodeA))
			{
				powerNodes[nodeB].Add(nodeA);
			}
		}


		public void NodeDying(EntityReflectiveEventArgs e)
		{
			IPowerGridNode node = e.Entity as IPowerGridNode;
			if (node != null)
			{
				Disconnect(node);
			}
		}

		internal void Disconnect(IPowerGridNode node)
		{
			if(node == null || !powerNodes.ContainsKey(node))
			{
				return;
			}

			// Time of death:   13:57
			node.DyingEvent -= NodeDying;

			List<IPowerGridNode> connectedNodes = powerNodes[node];
			foreach (var connectedNode in connectedNodes)
			{
				powerNodes[connectedNode].Remove(node);
			}

			powerNodes.Remove(node);
		}


		/// <summary>
		/// Is power routable between the two nodes?
		/// </summary>
		internal bool IsPowerRoutableBetween(IPowerGridNode powerNodeA, IPowerGridNode powerNodeB)
		{
			// Check for obstacles in the way
			List<Entity> nearbyEntities = theGame.EntitiesInArea((int)(Math.Min(powerNodeA.PowerLinkPointAbsolute.X, powerNodeB.PowerLinkPointAbsolute.X) - 0.5),
			                                                     (int)(Math.Min(powerNodeA.PowerLinkPointAbsolute.Y, powerNodeB.PowerLinkPointAbsolute.Y - 0.5)),
			                                                     (int)(Math.Abs(powerNodeA.PowerLinkPointAbsolute.X - powerNodeB.PowerLinkPointAbsolute.X) + 0.5),
			                                                     (int)(Math.Abs(powerNodeA.PowerLinkPointAbsolute.Y - powerNodeB.PowerLinkPointAbsolute.Y) + 0.5));

			foreach (Entity obstructingEntity in nearbyEntities)
			{
				if (obstructingEntity.Solid && obstructingEntity != powerNodeA && obstructingEntity != powerNodeB)
				{
					if (obstructingEntity.Position.ShortestDistanceToLine(powerNodeA.PowerLinkPointAbsolute, powerNodeB.PowerLinkPointAbsolute) < obstructingEntity.Radius.Value)
					{
						// It's obstructed
						return false;
					}
				}
			}

			// Good line
			return true;
		}


		/// <summary>
		/// Gets the requested amount of power from a single power-source that is able to provide it
		/// </summary>
		/// <param name="startingLocation">Where in the power grid should we begin our quest for power</param>
		/// <param name="amount">The amount of power to retrieve</param>
		/// <returns>Returns true if successful, false otherwise</returns>
		internal bool GetPower(IPowerGridNode startingLocation, float amount)
		{
			List<IPowerGridNode> path;
			IPowerProducer powerProducer = GetProducerWithPower(startingLocation, amount, out path);
			if(powerProducer != null)
			{
				for (int iNode = 1; iNode < path.Count; iNode++)
				{
					var linkToAdd = new Tuple<IPowerGridNode, IPowerGridNode>(path[iNode - 1], path[iNode]);
					var linkToAddVariant = new Tuple<IPowerGridNode, IPowerGridNode>(path[iNode], path[iNode - 1]);
					if (!recentlyActiveLinks.Contains(linkToAdd) && !recentlyActiveLinks.Contains(linkToAddVariant))
					{
						recentlyActiveLinks.Add(linkToAdd);
					}
				}

				return powerProducer.GetPower(amount);
			}
			return false;
		}


		/// <summary>
		/// Checks that the requested amount of power is available from a single power-source in the grid
		/// </summary>
		/// <param name="startingLocation">Where in the power grid should we begin our quest for power</param>
		/// <param name="amount">The amount of power to retrieve</param>
		/// <returns>Returns true if successful, false otherwise</returns>
		internal bool HasPower(IPowerGridNode startingLocation, float amount)
		{
			List<IPowerGridNode> path;
			return GetProducerWithPower(startingLocation, amount, out path) != null;
		}


		private IPowerProducer GetProducerWithPower(IPowerGridNode startingLocation, float amount, out List<IPowerGridNode> path)
		{
			// NOTE: This sorted list should be a Min Heap for best performance
			var toVisit = new SortedList<float, Tuple<IPowerGridNode, IPowerGridNode>>(powerNodes.Count);		// <Distance, <NodeToVisit, VisitedFrom>>
			var visited = new List<Tuple<IPowerGridNode, IPowerGridNode>>(powerNodes.Count);					// <VisitedNode, VisitedFrom>
			toVisit.Add(0, new Tuple<IPowerGridNode, IPowerGridNode>(startingLocation, null));

			IPowerGridNode cursor = toVisit.Values[0].Item1;
			while (cursor != null)
			{
				float cursorDistance = toVisit.Keys[0];
				visited.Add(Tuple.Create(cursor, toVisit.Values[0].Item2));
				toVisit.RemoveAt(0);

				if (cursor.ProducesPower)
				{
					// OOoo, a power source
					IPowerProducer producer = cursor as IPowerProducer;
					if (producer != null)
					{
						if (producer.AvailablePower >= amount)
						{
							// Power discovered, quit
							path = DecodePath(visited);
							return producer;
						}
					}
				}

				// Add of my unvisited neighbours to the list, sorted by distance
				foreach (var linkedNode in powerNodes[cursor])
				{
					if (!visited.Any(v => v.Item1 == linkedNode) && linkedNode.PowerStateActive)
					{
						// Get the distance from the starting location to the linked node
						float nodeDistance = cursorDistance + Vector2.Distance(cursor.PowerLinkPointAbsolute, linkedNode.PowerLinkPointAbsolute);

						// Find out if the linked node already exists in the toVisit list
						int indexOfNode = toVisit.ToList().FindIndex(v => v.Value.Item1 == linkedNode);

						if (indexOfNode >= 0)
						{
							// See if the new path is shorter
							if (toVisit.ElementAt(indexOfNode).Key > nodeDistance)
							{
								// Replace this node, we're closer
								toVisit.RemoveAt(indexOfNode);
								toVisit.Add(nodeDistance, Tuple.Create(linkedNode, cursor));
							}
						}
						else
						{
							// There is no existing path the linked node, add one
							toVisit.Add(nodeDistance, Tuple.Create(linkedNode, cursor));
						}
					}
				}

				cursor = toVisit.Values.Count > 0 ? toVisit.Values[0].Item1 : null;
			}


			// If we get here, we have exhausted the power grid and there was no power to be had
			path = null;
			return null;
		}


		private List<IPowerGridNode> DecodePath(List<Tuple<IPowerGridNode, IPowerGridNode>> visited)
		{
			var path = new List<IPowerGridNode>{ visited[visited.Count - 1].Item1 };
			IPowerGridNode search = visited[visited.Count - 1].Item2;

			bool done = false;
			while (search != null)
			{
				var node = visited.FirstOrDefault(visit => search == visit.Item1);
				search = node.Item2;
				path.Add(node.Item1);
			}

			return path;
		}


		

		public void Draw(SpriteBatch spriteBatch)
		{
			Color color;

			// TODO: This allocates a bunch of memory each draw, fix this!
			List<Tuple<IPowerGridNode, IPowerGridNode>> linksAlreadyDrawn = new List<Tuple<IPowerGridNode, IPowerGridNode>>(powerNodes.Count * 6);

			// Draw all of the active links first
			foreach (var linkToDraw in recentlyActiveLinks)
			{
				if (!linksAlreadyDrawn.Contains(linkToDraw))
				{
					color = new Color((int)(150 + theGame.Scale(50)), (int)(150 + theGame.Scale(50)), 0, (int)(150 + theGame.Scale(50)));

					spriteBatch.DrawLine(theGame.WorldToScreen(linkToDraw.Item1.PowerLinkPointAbsolute),
					                         theGame.WorldToScreen(linkToDraw.Item2.PowerLinkPointAbsolute),
					                         color);

					linksAlreadyDrawn.Add(linkToDraw);
					linksAlreadyDrawn.Add(new Tuple<IPowerGridNode, IPowerGridNode>(linkToDraw.Item2, linkToDraw.Item1));
				}
			}

			// Wipe the recent list
			recentlyActiveLinks.Clear();


			foreach (var nodeA in powerNodes.Keys)
			{
				foreach (var nodeB in powerNodes[nodeA])
				{
					var linkToDraw = new Tuple<IPowerGridNode, IPowerGridNode>(nodeA, nodeB);
					if (!linksAlreadyDrawn.Contains(linkToDraw))
					{
						if (nodeA.PowerStateActive && nodeB.PowerStateActive)
						{
							color = new Color((int)(70 + theGame.Scale(50)), (int)(70 + theGame.Scale(50)), 0, (int)(70 + theGame.Scale(50)));
						}
						else
						{
							color = new Color((int)(80 + theGame.Scale(50)), (int)(0 + theGame.Scale(50)), 0, (int)(0 + theGame.Scale(50)));
						}

						spriteBatch.DrawLine(theGame.WorldToScreen(nodeA.PowerLinkPointAbsolute),
						                     theGame.WorldToScreen(nodeB.PowerLinkPointAbsolute),
						                     color);

						linksAlreadyDrawn.Add(linkToDraw);
						linksAlreadyDrawn.Add(new Tuple<IPowerGridNode, IPowerGridNode>(nodeB, nodeA));
					}
				}
			}
		}
	}

}
