using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using AsteroidOutpost.Components;
using Microsoft.Xna.Framework;

namespace AsteroidOutpost.Systems
{
	class ProjectileSystem : GameComponent
	{
		private readonly World world;
		private readonly HitPointSystem hitPointSystem;

		public ProjectileSystem(Game game, World world, HitPointSystem hitPointSystem)
			: base(game)
		{
			this.world = world;
			this.hitPointSystem = hitPointSystem;
		}


		public override void Update(GameTime gameTime)
		{
			if (world.Paused) { return; }

			
			foreach (var projectile in world.GetComponents<Projectile>())
			{
				//if(projectile.Target != null)
				//{

				Position position = world.GetComponent<Position>(projectile);
				Velocity velocity = world.GetComponent<Velocity>(projectile);
				//float velocityMagnitude = velocity.CurrentVelocity.Length();
				velocity.CurrentVelocity += projectile.AccelerationVector * projectile.AccelerationMagnitude * (float)gameTime.ElapsedGameTime.TotalSeconds;

					
				// Boom?
				List<int> targets = world.EntitiesInArea(position.Center, position.Radius, true);
				foreach(int possibleHit in targets.Where(e => e != projectile.EntityID))
				{
					if(world.GetOwningForce(possibleHit) == world.GetOwningForce(projectile)) { continue; }

					Position targetPosition = world.GetComponent<Position>(possibleHit);

					// TODO: Maybe rethink how the detonation distance should work
					if(position.Distance(targetPosition) <= projectile.DetonationDistance + position.Radius + targetPosition.Radius)
					{
						HitPoints projectileHitPoints = world.GetNullableComponent<HitPoints>(projectile);
						if(projectileHitPoints != null && !projectileHitPoints.IsAlive())
						{
							Console.WriteLine("A dead projectile is about to deal damage again");
							Debugger.Break();
						}

						HitPoints enemyHP = world.GetNullableComponent<HitPoints>(possibleHit);
						if(enemyHP != null)
						{
							hitPointSystem.InflictDamageOn(enemyHP, projectile.Damage);
						}

						
						if(projectileHitPoints != null)
						{
							projectileHitPoints.Armour = 0;
							//missileHitPoints.OnDeath(new EntityDyingEventArgs(missileHitPoints));
						}
						else
						{
							Console.WriteLine("You should not be deleting components manually. It causes issues");
							Debugger.Break();
							world.DeleteComponents(projectile.EntityID);
						}

						break;
					}
				}


				//}
				//else
				//{
				//    // Delete ourselves, otherwise we'll just float around forever
				//    HitPoints missileHitPoints = world.GetNullableComponent<HitPoints>(projectile);
				//    if(missileHitPoints != null)
				//    {
				//        missileHitPoints.Armour = 0;
				//        //missileHitPoints.OnDeath(new EntityDyingEventArgs(missileHitPoints));
				//    }
				//    else
				//    {
				//        Console.WriteLine("You should not be deleting components manually. It causes issues");
				//        Debugger.Break();
				//        world.DeleteComponents(projectile.EntityID);
				//    }
				//}
			}
			
			base.Update(gameTime);
		}
	}
}
