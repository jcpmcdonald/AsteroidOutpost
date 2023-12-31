﻿using System;
using System.Collections.Generic;
using System.Linq;
using AsteroidOutpost.Components;
using Microsoft.Xna.Framework;

namespace AsteroidOutpost
{
	// Not *really* a component since it doesn't belong to entities
	public class PowerGrid
	{
		protected World world;

		public const int PowerConductingDistance = 220;
		internal readonly Dictionary<PowerGridNode, List<PowerGridNode>> powerNodes = new Dictionary<PowerGridNode, List<PowerGridNode>>(32);

		internal Dictionary<Tuple<PowerGridNode, PowerGridNode>, float> recentlyActiveLinks = new Dictionary<Tuple<PowerGridNode, PowerGridNode>, float>();


		public PowerGrid(World world)
			//: base(world)
		{
			this.world = world;
		}


		internal List<KeyValuePair<float, PowerGridNode>> GetAllPowerLinks(PowerGridNode node)
		{
			var allPowerLinks = new List<KeyValuePair<float, PowerGridNode>>(10);

			foreach (var powerNode in powerNodes.Keys)
			{
				if (powerNode != node)
				{
					// Note: Should I actually be basing power-distance on the power link location considering that the power link location is just to represent a 3D location in a 2D world?
					float distance = Vector2.Distance(powerNode.PowerLinkPointAbsolute(world), node.PowerLinkPointAbsolute(world));
					if (distance <= PowerConductingDistance)
					{
						allPowerLinks.Add(new KeyValuePair<float, PowerGridNode>(distance, powerNode));
					}
				}
			}


			if(node.ConductsPower)
			{
				return new List<KeyValuePair<float, PowerGridNode>>(allPowerLinks.Where(n => n.Value.ConductsPower || powerNodes[n.Value].Count == 0));
			}
			else
			{
				// Return the closest power conductor IF there's a valid connection
				// Else return all bad connections
				allPowerLinks.Sort((lhs, rhs) => lhs.Key.CompareTo(rhs.Key));
				KeyValuePair<float, PowerGridNode> theOne = allPowerLinks.FirstOrDefault(n => n.Value.ConductsPower && IsPowerRoutableBetween(node, n.Value));
				if(theOne.Equals(default(KeyValuePair<float, PowerGridNode>)))
				{

					return new List<KeyValuePair<float, PowerGridNode>>(allPowerLinks.Where(n => n.Value.ConductsPower));
				}

				return new List<KeyValuePair<float, PowerGridNode>>(1) { new KeyValuePair<float, PowerGridNode>(theOne.Key, theOne.Value) };
			}
		}


		
		internal void ConnectToPowerGrid(PowerGridNode newNode)
		{
			powerNodes.Add(newNode, new List<PowerGridNode>(6));

			List<KeyValuePair<float, PowerGridNode>> allPowerLinks = GetAllPowerLinks(newNode);

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
				PowerGridNode node = allPowerLinks.FirstOrDefault(n => n.Value != null && n.Value.ConductsPower && IsPowerRoutableBetween(newNode, n.Value)).Value;
				if(node != null)
				{
					Connect(newNode, node);
				}
			}
		}


		private void Connect(PowerGridNode nodeA, PowerGridNode nodeB)
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

		internal void Disconnect(PowerGridNode node)
		{
			if(node == null || !powerNodes.ContainsKey(node))
			{
				return;
			}

			List<PowerGridNode> connectedNodes = powerNodes[node];
			foreach (var connectedNode in connectedNodes)
			{
				powerNodes[connectedNode].Remove(node);
			}

			List<Tuple<PowerGridNode, PowerGridNode>> deadLinks = new List<Tuple<PowerGridNode, PowerGridNode>>();
			foreach (var recentlyActiveLink in recentlyActiveLinks.Keys)
			{
				if (recentlyActiveLink.Item1 == node || recentlyActiveLink.Item2 == node)
				{
					deadLinks.Add(recentlyActiveLink);
				}
			}
			foreach (var deadLink in deadLinks)
			{
				recentlyActiveLinks.Remove(deadLink);
			}

			//for(int i = recentlyActiveLinks.Count - 1; i >= 0; i--)
			//{
			//	var recentlyActiveLink = recentlyActiveLinks[i];
			//	if(recentlyActiveLink.Item1 == node || recentlyActiveLink.Item2 == node)
			//	{
			//		recentlyActiveLinks.RemoveAt(i);
			//	}
			//}

			powerNodes.Remove(node);
		}


		/// <summary>
		/// Is power routable between the two nodes?
		/// </summary>
		internal bool IsPowerRoutableBetween(PowerGridNode powerNodeA, PowerGridNode powerNodeB)
		{
			// Check for obstacles in the way
			List<int> nearbyEntities = world.EntitiesInArea((int)(Math.Min(powerNodeA.PowerLinkPointAbsolute(world).X, powerNodeB.PowerLinkPointAbsolute(world).X) - 0.5),
			                                                (int)(Math.Min(powerNodeA.PowerLinkPointAbsolute(world).Y, powerNodeB.PowerLinkPointAbsolute(world).Y - 0.5)),
			                                                (int)(Math.Abs(powerNodeA.PowerLinkPointAbsolute(world).X - powerNodeB.PowerLinkPointAbsolute(world).X) + 0.5),
			                                                (int)(Math.Abs(powerNodeA.PowerLinkPointAbsolute(world).Y - powerNodeB.PowerLinkPointAbsolute(world).Y) + 0.5),
			                                                true);

			foreach (int obstructingEntityID in nearbyEntities)
			{
				if (obstructingEntityID != powerNodeA.EntityID && obstructingEntityID != powerNodeB.EntityID)
				{
					Position obstructingPosition = world.GetComponent<Position>(obstructingEntityID);
					if (obstructingPosition.ShortestDistanceToLine(powerNodeA.PowerLinkPointAbsolute(world), powerNodeB.PowerLinkPointAbsolute(world)) < obstructingPosition.Radius)
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
		/// <param name="startingEntityID">Where in the power grid should we begin our quest for power</param>
		/// <param name="amount">The amount of power to retrieve</param>
		/// <returns>Returns true if successful, false otherwise</returns>
		internal bool GetPower(int startingEntityID, float amount)
		{
			return GetPower(world.GetComponent<PowerGridNode>(startingEntityID), amount);
		}


		/// <summary>
		/// Gets the requested amount of power from a single power-source that is able to provide it
		/// </summary>
		/// <param name="startingLocation">Where in the power grid should we begin our quest for power</param>
		/// <param name="amount">The amount of power to retrieve</param>
		/// <returns>Returns true if successful, false otherwise</returns>
		internal bool GetPower(PowerGridNode startingLocation, float amount)
		{
			List<PowerGridNode> path;
			PowerStorage powerStorage = GetPowerStorageWithPower(startingLocation, amount, out path);
			if(powerStorage != null)
			{
				// Light up the path to the power source
				for (int iNode = 1; iNode < path.Count; iNode++)
				{
					var linkToAdd = new Tuple<PowerGridNode, PowerGridNode>(path[iNode - 1], path[iNode]);
					var linkToAddVariant = new Tuple<PowerGridNode, PowerGridNode>(path[iNode], path[iNode - 1]);
					if (recentlyActiveLinks.ContainsKey(linkToAdd))
					{
						recentlyActiveLinks[linkToAdd] += Math.Abs(amount);
					}
					else if (recentlyActiveLinks.ContainsKey(linkToAddVariant))
					{
						recentlyActiveLinks[linkToAddVariant] += Math.Abs(amount);
					}
					else
					{
						recentlyActiveLinks.Add(linkToAdd, Math.Abs(amount));
					}
				}

				return powerStorage.GetPower(amount);
			}
			return false;
		}


		internal void PutPower(int startingEntityID, float amount)
		{
			GetPower(world.GetComponent<PowerGridNode>(startingEntityID), -amount);
		}


		/// <summary>
		/// Checks that the requested amount of power is available from a single power-source in the grid
		/// </summary>
		/// <param name="startingEntityID">Where in the power grid should we begin our quest for power</param>
		/// <param name="amount">The amount of power to retrieve</param>
		/// <returns>Returns true if successful, false otherwise</returns>
		internal bool HasPower(int startingEntityID, float amount)
		{
			return HasPower(world.GetComponent<PowerGridNode>(startingEntityID), amount);
		}


		/// <summary>
		/// Checks that the requested amount of power is available from a single power-source in the grid
		/// </summary>
		/// <param name="startingLocation">Where in the power grid should we begin our quest for power</param>
		/// <param name="amount">The amount of power to retrieve</param>
		/// <returns>Returns true if successful, false otherwise</returns>
		internal bool HasPower(PowerGridNode startingLocation, float amount)
		{
			List<PowerGridNode> path;
			return GetPowerStorageWithPower(startingLocation, amount, out path) != null;
		}


		private PowerStorage GetPowerStorageWithPower(PowerGridNode startingLocation, float amount, out List<PowerGridNode> path)
		{
			// NOTE: This sorted list should be a Min Heap for best performance
			var toVisit = new SortedList<float, Tuple<PowerGridNode, PowerGridNode>>(powerNodes.Count);		// <Distance, <NodeToVisit, VisitedFrom>>
			var visited = new List<Tuple<PowerGridNode, PowerGridNode>>(powerNodes.Count);					// <VisitedNode, VisitedFrom>
			toVisit.Add(0, new Tuple<PowerGridNode, PowerGridNode>(startingLocation, null));

			PowerGridNode cursor = toVisit.Values[0].Item1;
			while (cursor != null)
			{
				float cursorDistance = toVisit.Keys[0];
				visited.Add(Tuple.Create(cursor, toVisit.Values[0].Item2));
				toVisit.RemoveAt(0);

				PowerStorage storage = world.GetNullableComponent<PowerStorage>(cursor);
				if (storage != null && (!storage.Locked || amount < 0))
				{
					float powerAfterUse = storage.AvailablePower - amount;
					if (powerAfterUse >= 0 && powerAfterUse <= storage.MaxPower)
					{
						// Power discovered, quit
						path = DecodePath(visited);
						return storage;
					}
				}

				// Add of my unvisited neighbours to the list, sorted by distance
				foreach (var linkedNode in powerNodes[cursor])
				{
					if (visited.All(v => v.Item1 != linkedNode) && world.GetNullableComponent<Constructing>(linkedNode) == null)
					{
						// Get the distance from the starting location to the linked node
						float nodeDistance = cursorDistance + Vector2.Distance(cursor.PowerLinkPointAbsolute(world), linkedNode.PowerLinkPointAbsolute(world));

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
							float collide = 0.0f;
							while(toVisit.ContainsKey(nodeDistance + collide))
							{
								collide += 0.001f;
							}
							toVisit.Add(nodeDistance + collide, Tuple.Create(linkedNode, cursor));
						}
					}
				}

				cursor = toVisit.Values.Count > 0 ? toVisit.Values[0].Item1 : null;
			}


			// If we get here, we have exhausted the power grid and there was no power to be had
			path = null;
			return null;
		}


		private List<PowerGridNode> DecodePath(List<Tuple<PowerGridNode, PowerGridNode>> visited)
		{
			var path = new List<PowerGridNode>{ visited[visited.Count - 1].Item1 };
			PowerGridNode search = visited[visited.Count - 1].Item2;

			while (search != null)
			{
				var node = visited.FirstOrDefault(visit => search == visit.Item1);
				search = node.Item2;
				path.Add(node.Item1);
			}

			return path;
		}
	}

}
