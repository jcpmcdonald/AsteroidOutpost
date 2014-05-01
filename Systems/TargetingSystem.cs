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
					targeting.Target = FindClosestTarget(targeting.EntityID);
				}
			}

			base.Update(gameTime);
		}


		private int? FindClosestTarget(int entityID)
		{
			
			// Find a suitable target
			Position position = world.GetComponent<Position>(entityID);
			var livingThings = world.GetComponents<HitPoints>().Where(x => x.EntityID != entityID &&
																			x.IsAlive() &&
																			world.GetOwningForce(x).Team != Team.Neutral &&
																			world.GetOwningForce(x).Team != world.GetOwningForce(entityID).Team);

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


	}
}
