using AsteroidOutpost.Components;
using AsteroidOutpost.Entities;
using AsteroidOutpost.Interfaces;
using C3.XNA;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using XNASpriteLib;

namespace AsteroidOutpost.Systems
{
	class AIStrafeSystem : DrawableGameComponent
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
				const int weaponGive = 30; // Just some number to help ships & towers get closer to each other and prevent float errors

				targeting.TargetVector = Vector2.Normalize(targetPosition.Center - position.Center - (velocity.CurrentVelocity * 5));


				if (targeting.Target != null)
				{
					// Rotate the tower to face the target:
					Animator animator = world.GetComponent<Animator>(aiStrafeUser);
					animator.SetOrientation(MathHelper.ToDegrees((float)Math.Atan2(targeting.TargetVector.X, -targeting.TargetVector.Y)), false);
				}


				Vector2 vectorToTarget;
				float desiredDistanceFromTarget;
				switch (aiStrafeUser.State)
				{
					case AIStrafe.StrafeState.Approach:
						if (aiStrafeUser.MissileCount <= 0)
						{
							const AIStrafe.StrafeState newState = AIStrafe.StrafeState.GetClose;
							aiStrafeUser.State = newState;
							goto case newState;
						}

						if (inRange(position, targetPosition, missileLauncher.Range))
						{
							const AIStrafe.StrafeState newState = AIStrafe.StrafeState.ShootMissiles;
							aiStrafeUser.State = newState;
							goto case newState;
						}

						desiredDistanceFromTarget = missileLauncher.Range + position.Radius + targetPosition.Radius - weaponGive;
						vectorToTarget = position.Center - targetPosition.Center + velocity.CurrentVelocity;
						vectorToTarget *= (1 - (desiredDistanceFromTarget / vectorToTarget.Length()));
						aiStrafeUser.DesiredPosition = position.Center - vectorToTarget;

						break;



					case AIStrafe.StrafeState.ShootMissiles:
						if (aiStrafeUser.MissileCount <= 0)
						{
							const AIStrafe.StrafeState newState = AIStrafe.StrafeState.GetClose;
							aiStrafeUser.State = newState;
							goto case newState;
						}
						if (!inRange(position, targetPosition, missileLauncher.Range))
						{
							const AIStrafe.StrafeState newState = AIStrafe.StrafeState.Approach;
							aiStrafeUser.State = newState;
							goto case newState;
						}
						
						//desiredDistanceFromTarget = missileLauncher.Range + position.Radius + targetPosition.Radius - weaponGive;
						vectorToTarget = Vector2.Normalize(targetPosition.Center - position.Center);
						float angle = (float)Math.Atan2(vectorToTarget.X, -vectorToTarget.Y);
						float rotationRate = 0.08f; // * (float)gameTime.ElapsedGameTime.TotalSeconds;
						angle += rotationRate;
						aiStrafeUser.DesiredPosition = targetPosition.Center - new Vector2((float)Math.Sin(angle), -(float)Math.Cos(angle)) * position.Distance(targetPosition);


						if (projectileLauncherSystem.Fire(missileLauncher))
						{
							aiStrafeUser.MissileCount--;
						}

						break;



					case AIStrafe.StrafeState.GetClose:
						//aiStrafeUser.DampenStrafeVelocity(gameTime);

						desiredDistanceFromTarget = laser.Range + position.Radius + targetPosition.Radius - weaponGive;
						vectorToTarget = position.Center - targetPosition.Center + velocity.CurrentVelocity;
						vectorToTarget *= (1 - (desiredDistanceFromTarget / vectorToTarget.Length()));
						aiStrafeUser.DesiredPosition = position.Center - vectorToTarget;


						if (inRange(position, targetPosition, laser.Range))
						{
							//aiStrafeUser.State = AIStrafe.StrafeState.FireLasers;
							break;
						}
						break;

					case AIStrafe.StrafeState.FireLasers:
						break;
				}


				moveTowardDesiredPos(aiStrafeUser, position, velocity, gameTime);

			}
		}


		private bool inRange(Position position, Position targetPosition, float range)
		{
			return position.Distance(targetPosition) - position.Radius - targetPosition.Radius <= range;
		}


		private void moveTowardDesiredPos(AIStrafe aiStrafeUser, Position position, Velocity velocity, GameTime gameTime)
		{
			float distanceToTarget = position.Distance(aiStrafeUser.DesiredPosition);
			if (distanceToTarget > velocity.MinDistanceToStop())
			{
				velocity.AccelerationVector = -Vector2.Normalize(position.Center - aiStrafeUser.DesiredPosition + velocity.CurrentVelocity);

				if (velocity.AccelerationVector.Length() > 0)
				{
					velocity.CurrentVelocity += velocity.AccelerationVector * velocity.AccelerationMagnitude * (float)gameTime.ElapsedGameTime.TotalSeconds;
				}
			}
			else
			{
				velocity.Decelerate(gameTime);
			}
		}
	}
}
