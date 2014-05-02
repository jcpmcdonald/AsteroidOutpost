using AsteroidOutpost.Components;
using AsteroidOutpost.Interfaces;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsteroidOutpost.Systems
{
	public class AIBasicSystem : GameComponent
	{
		private World world;

		public AIBasicSystem(AOGame game, World world)
			: base(game)
		{
			this.world = world;
		}

		public override void Update(GameTime gameTime)
		{
			if (world.Paused) { return; }

			var aiBasicUsers = world.GetComponents<AIBasic>();
			foreach (var aiBasicUser in aiBasicUsers)
			{
				Targeting targeting = world.GetComponent<Targeting>(aiBasicUser);
				if (targeting.Target == null) { continue; }

				Position position = world.GetComponent<Position>(aiBasicUser);
				Velocity velocity = world.GetComponent<Velocity>(aiBasicUser);
				List<IWeapon> weapons = world.GetWeapons(aiBasicUser);

				Position targetPosition = world.GetComponent<Position>(targeting.Target.Value);

				float distanceToTarget = position.Distance(targetPosition);
				IWeapon bestWeapon = weapons.Find(x => x.Range  == weapons.Min(y => y.Range));
				const int weaponGive = 10; // Just some number to help ships & towers get closer to each other and prevent float errors

				targeting.TargetVector = Vector2.Normalize(targetPosition.Center - position.Center - (velocity.CurrentVelocity * 5));

				if (distanceToTarget - (bestWeapon.Range - weaponGive) > velocity.MinDistanceToStop())
				{
					velocity.AccelerationVector = targeting.TargetVector; // + vehicle.Cohesion + vehicle.Separation + vehicle.Alignment;

					if (velocity.AccelerationVector.Length() > 0)
					{
						velocity.CurrentVelocity += velocity.AccelerationVector * velocity.AccelerationMagnitude * (float)gameTime.ElapsedGameTime.TotalSeconds;
					}
				}
				else
				{
					velocity.Decelerate(gameTime);
				}


				if (targeting.TargetVector != Vector2.Zero)
				{
					Animator animator = world.GetComponent<Animator>(aiBasicUser);
					var facing = Vector2.Normalize(targetPosition.Center - position.Center);
					animator.SetOrientation(MathHelper.ToDegrees((float)Math.Atan2(facing.X, -facing.Y)), true);
				}
			}
		}

		

	}
}
