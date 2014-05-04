using AsteroidOutpost.Components;
using AsteroidOutpost.Interfaces;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsteroidOutpost.Systems
{
	class AIStrafeSystem : GameComponent
	{
		private World world;
		private readonly ProjectileLauncherSystem projectileLauncherSystem;


		public AIStrafeSystem(AOGame game, World world, ProjectileLauncherSystem projectileLauncherSystem)
			: base(game)
		{
			this.world = world;
			this.projectileLauncherSystem = projectileLauncherSystem;
		}


		public override void Update(GameTime gameTime)
		{
			if (world.Paused) { return; }

			var aiStrafeUsers = world.GetComponents<AIStrafe>();
			foreach (var aiStrafeUser in aiStrafeUsers)
			{
				Targeting targeting = world.GetComponent<Targeting>(aiStrafeUser);
				if (targeting.Target == null) { continue; }

				Position position = world.GetComponent<Position>(aiStrafeUser);
				Velocity velocity = world.GetComponent<Velocity>(aiStrafeUser);

				ProjectileLauncher missileLauncher = world.GetComponent<ProjectileLauncher>(aiStrafeUser);
				LaserWeapon laser = world.GetComponent<LaserWeapon>(aiStrafeUser);

				Position targetPosition = world.GetComponent<Position>(targeting.Target.Value);

				float distanceToTarget = position.Distance(targetPosition) - targetPosition.Radius - position.Radius;
				const int weaponGive = 40; // Just some number to help ships & towers get closer to each other and prevent float errors

				targeting.TargetVector = Vector2.Normalize(targetPosition.Center - position.Center - (velocity.CurrentVelocity * 5));


				if (targeting.Target != null)
				{
					// Rotate the tower to face the target:
					Animator towerAnimator = world.GetComponent<Animator>(aiStrafeUser);
					towerAnimator.SetOrientation(MathHelper.ToDegrees((float)Math.Atan2(targeting.TargetVector.X, -targeting.TargetVector.Y)), false);
				}


				switch (aiStrafeUser.State)
				{
					case AIStrafe.StrafeState.Approach:
						if (aiStrafeUser.MissileCount <= 0)
						{
							aiStrafeUser.State = AIStrafe.StrafeState.GetClose;
							break;
						}

						aiStrafeUser.DampenStrafeVelocity(gameTime);

						if (distanceToTarget - (missileLauncher.Range - weaponGive) > velocity.MinDistanceToStop())
						{
							velocity.AccelerationVector = targeting.TargetVector;

							if (velocity.AccelerationVector.Length() > 0)
							{
								velocity.CurrentVelocity += velocity.AccelerationVector * velocity.AccelerationMagnitude * (float)gameTime.ElapsedGameTime.TotalSeconds;
							}
						}
						else
						{
							velocity.Decelerate(gameTime);
						}

						if (inRange(position, targetPosition, missileLauncher.Range))
						{
							aiStrafeUser.State = AIStrafe.StrafeState.ShootMissiles;
							break;
						}
						break;



					case AIStrafe.StrafeState.ShootMissiles:
						if (aiStrafeUser.MissileCount <= 0)
						{
							aiStrafeUser.State = AIStrafe.StrafeState.GetClose;
							break;
						}
						if (distanceToTarget - weaponGive > missileLauncher.Range)
						{
							aiStrafeUser.State = AIStrafe.StrafeState.Approach;
							break;
						}
						
						velocity.Decelerate(gameTime);

						Vector2 tangent = Vector2.Normalize(new Vector2(targeting.TargetVector.Y, -targeting.TargetVector.X));
						aiStrafeUser.StrafeVelocity += 0.8f * tangent * aiStrafeUser.StrafeAccelerationMagnitude * (float)gameTime.ElapsedGameTime.TotalSeconds;
						aiStrafeUser.StrafeVelocity += 0.2f * targeting.TargetVector * aiStrafeUser.StrafeAccelerationMagnitude * (float)gameTime.ElapsedGameTime.TotalSeconds;

						if (projectileLauncherSystem.Fire(missileLauncher))
						{
							aiStrafeUser.MissileCount --;
						}

						break;



					case AIStrafe.StrafeState.GetClose:
						aiStrafeUser.DampenStrafeVelocity(gameTime);

						if (distanceToTarget - (laser.Range - weaponGive) > velocity.MinDistanceToStop())
						{
							velocity.AccelerationVector = targeting.TargetVector;

							if (velocity.AccelerationVector.Length() > 0)
							{
								velocity.CurrentVelocity += velocity.AccelerationVector * velocity.AccelerationMagnitude * (float)gameTime.ElapsedGameTime.TotalSeconds;
							}
						}
						else
						{
							velocity.Decelerate(gameTime);
						}

						if (inRange(position, targetPosition, laser.Range))
						{
							//aiStrafeUser.State = AIStrafe.StrafeState.FireLasers;
							break;
						}
						break;

					case AIStrafe.StrafeState.FireLasers:
						break;
				}


				position.Center += aiStrafeUser.StrafeVelocity * (float)gameTime.ElapsedGameTime.TotalSeconds;

			}
		}


		private bool inRange(Position position, Position targetPosition, float range)
		{
			return position.Distance(targetPosition) - position.Radius - targetPosition.Radius <= range;
		}


	}
}
