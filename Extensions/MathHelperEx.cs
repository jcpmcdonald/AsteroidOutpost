using System;
using System.Collections.Generic;
using AsteroidOutpost.Components;
using Microsoft.Xna.Framework;

namespace AsteroidOutpost.Extensions
{
	internal static class MathHelperEx
	{
		/// <summary>
		/// Is the path between two positions obstructed by any other entity?
		/// </summary>
		/// <returns></returns>
		public static bool IsObstructed(World world, Position a, Position b)
		{
			// We can't be obstructed by ourselves, so create an ignore list
			List<int> ignoreList = new List<int>();
			ignoreList.Add(a.EntityID);
			ignoreList.Add(b.EntityID);

			return IsLineObstructed(world, a.Center, b.Center, ignoreList);
		}


		/// <summary>
		/// Is the line between two points obstructed by a entity?
		/// </summary>
		/// <param name="world">A reference to the game</param>
		/// <param name="pointA">The first point on the line</param>
		/// <param name="pointB">The second point on the line</param>
		/// <param name="ignoreList">A list of non-obstructable entities, these entities will be ignored. Use null if you don't want to ignore anything</param>
		/// <returns>True if an entity is blocking the line, false otherwise</returns>
		public static bool IsLineObstructed(World world, Vector2 pointA, Vector2 pointB, List<int> ignoreList)
		{
			// Check for obstacles in the way
			List<int> nearbyEntities = world.EntitiesInArea((int)(Math.Min(pointA.X, pointB.X) - 0.5),
			                                                (int)(Math.Min(pointA.Y, pointB.Y - 0.5)),
			                                                (int)(Math.Abs(pointA.X - pointB.X) + 0.5),
			                                                (int)(Math.Abs(pointA.Y - pointB.Y) + 0.5));

			foreach (int obstructingEntity in nearbyEntities)
			{
				if (ignoreList == null || !ignoreList.Contains(obstructingEntity))
				{
					Position obstructingPosition = world.GetComponent<Position>(obstructingEntity);
					if (!obstructingPosition.CanConductThrough && obstructingPosition.ShortestDistanceToLine(pointA, pointB) < obstructingPosition.Radius)
					{
						// It's obstructed
						return true;
					}
				}
			}

			// Good line
			return false;
		}
	}
}
