using AsteroidOutpost.Components;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsteroidOutpost.Systems
{
	public class TargetingSystem : GameComponent
	{
		private World world;

		public TargetingSystem(AOGame game, World world)
			: base(game)
		{
			this.world = world;
		}

		public override void Update(GameTime gameTime)
		{
			if (world.Paused) { return; }

			var targetingEntities = world.GetComponents<Targeting>();
			foreach(var targeting in targetingEntities)
			{
				if (targeting.Target == null)
				{
					targeting.Target = FindClosestTarget(targeting);
				}
			}

			base.Update(gameTime);
		}


		private int? FindClosestTarget(Component exampleComponent)
		{
			
			// Find a suitable target
			Position position = world.GetComponent<Position>(exampleComponent.EntityID);
			var livingThings = world.GetComponents<HitPoints>().Where(x => x.EntityID != exampleComponent.EntityID &&
			                                                               x.IsAlive() &&
			                                                               world.GetOwningForce(x).Team != Team.Neutral &&
			                                                               world.GetOwningForce(x).Team != world.GetOwningForce(exampleComponent.EntityID).Team);

			var livingThingPositions = livingThings.Select(x => world.GetComponent<Position>(x));
			Position closestLivingThing = null;
			foreach (var livingThingPosition in livingThingPositions)
			{
				var targetable = world.GetNullableComponent<Targetable>(livingThingPosition);
				if (targetable != null && (closestLivingThing == null || position.Distance(livingThingPosition) < position.Distance(closestLivingThing)))
				{
					closestLivingThing = livingThingPosition;
				}
			}

			if (closestLivingThing != null)
			{
				return closestLivingThing.EntityID;
			}
			return null;
		}


		//private Position AcquireTarget(ProjectileLauncher projectileLauncher)
		//{
		//	Position position = world.GetComponent<Position>(projectileLauncher);
		//	List<int> possibleTargets = world.EntitiesInArea(position.Center, projectileLauncher.Range);

		//	// Always pick the closest target
		//	Position closestTargetPosition = null;
		//	foreach (var possibleTarget in possibleTargets)
		//	{
		//		if (possibleTarget == projectileLauncher.EntityID ||
		//			world.GetOwningForce(possibleTarget).Team == world.GetOwningForce(projectileLauncher).Team ||
		//			world.GetOwningForce(possibleTarget).Team == Team.Neutral ||
		//			world.GetNullableComponent<Targetable>(possibleTarget) == null ||
		//			world.GetNullableComponent<Constructing>(possibleTarget) != null)
		//		{
		//			// Eliminate invalid targets
		//			continue;
		//		}


		//		Position possibleTargetPosition = world.GetComponent<Position>(possibleTarget);
		//		if (position.Distance(possibleTargetPosition) <= projectileLauncher.Range &&
		//			(closestTargetPosition == null || position.Distance(closestTargetPosition) > position.Distance(possibleTargetPosition)))
		//		{
		//			closestTargetPosition = possibleTargetPosition;
		//		}
		//	}

		//	return closestTargetPosition;
		//}

	}
}
