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
		private SpriteBatch spriteBatch;
		private World world;
		private readonly ProjectileLauncherSystem projectileLauncherSystem;

		private static SpriteFont font;


		public AIStrafeSystem(AOGame game, World world, ProjectileLauncherSystem projectileLauncherSystem)
			: base(game)
		{
			spriteBatch = new SpriteBatch(game.GraphicsDevice);
			this.world = world;
			this.projectileLauncherSystem = projectileLauncherSystem;
		}


		/// <summary>
		/// Called when graphics resources need to be loaded. Override this method to load any component-specific graphics resources.
		/// </summary>
		protected override void LoadContent()
		{
			font = Game.Content.Load<SpriteFont>("Fonts\\ControlFont");
			base.LoadContent();
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
						if (distanceToTarget - weaponGive > missileLauncher.Range)
						{
							const AIStrafe.StrafeState newState = AIStrafe.StrafeState.Approach;
							aiStrafeUser.State = newState;
							goto case newState;
						}
						
						desiredDistanceFromTarget = missileLauncher.Range + position.Radius + targetPosition.Radius - weaponGive;
						vectorToTarget = Vector2.Normalize(targetPosition.Center - position.Center);
						float angle = (float)Math.Atan2(vectorToTarget.X, -vectorToTarget.Y);
						float rotationRate = 0.08f; // * (float)gameTime.ElapsedGameTime.TotalSeconds;
						angle += rotationRate;
						aiStrafeUser.DesiredPosition = targetPosition.Center - new Vector2((float)Math.Sin(angle), -(float)Math.Cos(angle)) * position.Distance(targetPosition);

						//position.Center = aiStrafeUser.DesiredPosition;

						//Vector2 tangent = Vector2.Normalize(new Vector2(vectorToTarget.Y, -vectorToTarget.X));
						//Vector2 asdf = tangent - (aiStrafeUser.StrafeVelocity * (float)gameTime.ElapsedGameTime.TotalSeconds);
						//aiStrafeUser.StrafeVelocity += Vector2.Normalize(asdf) * aiStrafeUser.StrafeAccelerationMagnitude * (float)gameTime.ElapsedGameTime.TotalSeconds;
						//aiStrafeUser.StrafeVelocity += 0.2f * targeting.TargetVector * aiStrafeUser.StrafeAccelerationMagnitude * (float)gameTime.ElapsedGameTime.TotalSeconds;

						if (projectileLauncherSystem.Fire(missileLauncher))
						{
							aiStrafeUser.MissileCount--;
						}

						break;



					case AIStrafe.StrafeState.GetClose:
						//aiStrafeUser.DampenStrafeVelocity(gameTime);


						//moveIntoRange(distanceToTarget,
						//			  (laser.Range - weaponGive),
						//			  velocity,
						//			  targeting.TargetVector,
						//			  gameTime);


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


		public override void Draw(GameTime gameTime)
		{
			spriteBatch.Begin();

			var aiStrafeUsers = world.GetComponents<AIStrafe>();
			foreach (var aiStrafeUser in aiStrafeUsers)
			{
				Targeting targeting = world.GetComponent<Targeting>(aiStrafeUser);
				if (targeting.Target.HasValue)
				{
					Position position = world.GetComponent<Position>(aiStrafeUser);
					Position targetPosition = world.GetComponent<Position>(targeting.Target.Value);

					Vector2 toTarget = Vector2.Normalize(position.Center - targetPosition.Center);

					//spriteBatch.DrawLine(world.WorldToScreen(position.Center), world.WorldToScreen(aiStrafeUser.DesiredPosition), Color.White);

					//spriteBatch.DrawString(font,
					//					   "" + toTarget.Length(),
					//					   world.WorldToScreen(position.Center - (toTarget / 2)),
					//					   Color.White);

					//spriteBatch.DrawCircle(world.WorldToScreen(aiStrafeUser.DesiredPosition), world.Scale(10), 15, Color.Red);


					
					//Vector2 vectorToTarget = Vector2.Normalize(targetPosition.Center - position.Center);
					//float distanceToTarget = position.Distance(targetPosition);

					//int times = 10;
					//float angle = (float)Math.Atan2(vectorToTarget.X, -vectorToTarget.Y);
					//float rotationRate = 0.18f / times;
					//for (int i = 0; i < times; i++)
					//{
					//	float a = angle + (rotationRate * i);
					//	// Vector2 offset = new Vector2((float)(distanceToTarget * Math.Sin(a)), -(float)(distanceToTarget * Math.Cos(a)));
					//	Vector2 offset = new Vector2((float)Math.Sin(a), -(float)Math.Cos(a)) * distanceToTarget;
					//	Vector2 pos = targetPosition.Center - offset;
					//	spriteBatch.DrawCircle(world.WorldToScreen(pos), world.Scale(i + 3), 15, Color.White);
					//}

					//Console.WriteLine(rotationRate * 1);
					//Console.WriteLine(rotationRate * 10);


					//float angle1 = (float)Math.Atan2(vectorToTarget.X, -vectorToTarget.Y);
					//float angle2 = (float)Math.Atan2(vectorToTarget.X, -vectorToTarget.Y);
					//float angle3 = (float)Math.Atan2(vectorToTarget.X, -vectorToTarget.Y);
					//float rotationRate = 0.18f;  // about 10 degrees
					//angle1 += rotationRate;
					//angle2 += rotationRate * 2;
					//angle3 += rotationRate * 3;
					//Vector2 pos1 = targetPosition.Center - new Vector2((float)(distanceToTarget * Math.Sin(angle1)), -(float)(distanceToTarget * Math.Cos(angle1)));
					//Vector2 pos2 = targetPosition.Center - new Vector2((float)(distanceToTarget * Math.Sin(angle2)), -(float)(distanceToTarget * Math.Cos(angle2)));
					//Vector2 pos3 = targetPosition.Center - new Vector2((float)(distanceToTarget * Math.Sin(angle3)), -(float)(distanceToTarget * Math.Cos(angle3)));
					//spriteBatch.DrawCircle(world.WorldToScreen(pos1), world.Scale(10), 15, Color.White);
					//spriteBatch.DrawCircle(world.WorldToScreen(pos2), world.Scale(10), 15, Color.White);
					//spriteBatch.DrawCircle(world.WorldToScreen(pos3), world.Scale(10), 15, Color.White);

				}
			}

			spriteBatch.End();
			base.Draw(gameTime);
		}
	}
}
