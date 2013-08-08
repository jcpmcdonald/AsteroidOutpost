using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using AsteroidOutpost.Components;
using Microsoft.Xna.Framework;

namespace AsteroidOutpost.Systems
{
	public class MissileSystem : GameComponent
	{
		private readonly World world;

		public MissileSystem(Game game, World world)
			: base(game)
		{
			this.world = world;
		}

		public override void Update(GameTime gameTime)
		{
			if (world.Paused) { return; }

			
			foreach (var missile in world.GetComponents<MissileProjectile>())
			{
				Position targetPosition = null;
				if(missile.Target != null)
				{
					targetPosition = world.GetNullableComponent<Position>(missile.Target.Value);
					if(targetPosition == null)
					{
						missile.Target = null;
					}
				}
				
				if(missile.Target != null)
				{
					Position position = world.GetComponent<Position>(missile);
					Velocity velocity = world.GetComponent<Velocity>(missile);
					float velocityMagnitude = velocity.CurrentVelocity.Length();
					velocityMagnitude += missile.Acceleration * (float)gameTime.ElapsedGameTime.TotalSeconds;
					if(velocity.CurrentVelocity == Vector2.Zero)
					{
						velocity.CurrentVelocity = targetPosition.Center - position.Center;
					}
					velocity.CurrentVelocity = Vector2.Normalize(velocity.CurrentVelocity) * velocityMagnitude;

					
					// Boom?
					if(position.Distance(targetPosition) <= missile.DetonationDistance + position.Radius + targetPosition.Radius)
					{
						HitPoints missileHitPoints = world.GetNullableComponent<HitPoints>(missile);
						if(missileHitPoints != null && !missileHitPoints.IsAlive())
						{
							Console.WriteLine("A dead missile is about to deal damage again");
							Debugger.Break();
						}

						HitPointSystem.InflictDamageOn(world.GetComponent<HitPoints>(missile.Target.Value), missile.Damage);

						
						if(missileHitPoints != null)
						{
							missileHitPoints.Armour = 0;
							//missileHitPoints.OnDeath(new EntityDyingEventArgs(missileHitPoints));
						}
						else
						{
							Console.WriteLine("You should not be deleting components manually. It causes issues");
							Debugger.Break();
							world.DeleteComponents(missile.EntityID);
						}
					}
				}
				else
				{
					// Delete ourselves, otherwise we'll just float around forever
					HitPoints missileHitPoints = world.GetNullableComponent<HitPoints>(missile);
					if(missileHitPoints != null)
					{
						missileHitPoints.Armour = 0;
						//missileHitPoints.OnDeath(new EntityDyingEventArgs(missileHitPoints));
					}
					else
					{
						Console.WriteLine("You should not be deleting components manually. It causes issues");
						Debugger.Break();
						world.DeleteComponents(missile.EntityID);
					}
				}
			}
			
			base.Update(gameTime);
		}
	}
}
