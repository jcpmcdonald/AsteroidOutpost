using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using AsteroidOutpost.Components;
using AsteroidOutpost.Entities;
using AsteroidOutpost.Screens;
using C3.XNA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AsteroidOutpost.Systems
{
	class MissileWeaponSystem : DrawableGameComponent
	{
		private readonly World world;
		private SpriteBatch spriteBatch;

		public MissileWeaponSystem(Game game, World world)
			: base(game)
		{
			this.world = world;
			spriteBatch = new SpriteBatch(game.GraphicsDevice);
		}


		public override void Update(GameTime gameTime)
		{
			foreach (var missileWeapon in world.GetComponents<MissileWeapon>())
			{
				missileWeapon.TimeSinceLastShot += gameTime.ElapsedGameTime;
				Position closestTargetPosition = AcquireTarget(missileWeapon);
				missileWeapon.Target = closestTargetPosition != null ? (int?)closestTargetPosition.EntityID : null;

				// See if we can shoot yet
				if(missileWeapon.Target != null && missileWeapon.TimeSinceLastShot.TotalMilliseconds > missileWeapon.FireRate)
				{
					Position position = world.GetComponent<Position>(missileWeapon);
					Position targetPosition = world.GetComponent<Position>(missileWeapon.Target.Value);
					Vector2 accelerationVector = Vector2.Normalize(targetPosition.Center - position.Center);

					int missileID = EntityFactory.Create("Missile", new Dictionary<String, object>(){
						{ "Sprite.Scale", 0.7f },
						{ "Sprite.Set", null },
						{ "Sprite.Animation", null },
						{ "Sprite.Orientation", 0f },
						//{ "Sprite.RotateFrame", true },
						{ "Transpose.Position", position.Center },
						{ "Transpose.Radius", 10 },
						{ "OwningForce", world.GetOwningForce(missileWeapon.EntityID) },
						{ "TargetEntityID", missileWeapon.Target }
					});

					Animator missileAnimator = world.GetComponent<Animator>(missileID);
					missileAnimator.SetOrientation(MathHelper.ToDegrees((float)Math.Atan2(accelerationVector.X, -accelerationVector.Y)), true);

					missileWeapon.TimeSinceLastShot = TimeSpan.Zero;
				}

				if(missileWeapon.Target != null)
				{
					Position position = world.GetComponent<Position>(missileWeapon);
					Position targetPosition = world.GetComponent<Position>(missileWeapon.Target.Value);
					Vector2 directionToTarget = Vector2.Normalize(targetPosition.Center - position.Center);
					
					if(directionToTarget.Length() > 1.00001)
					{
						Debugger.Break();
					}

					// Rotate the tower to face the target:
					Animator towerAnimator = world.GetComponent<Animator>(missileWeapon);
					towerAnimator.SetOrientation(MathHelper.ToDegrees((float)Math.Atan2(directionToTarget.X, -directionToTarget.Y)), false);
				}
			}


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

				}
			}
		}


		private Position AcquireTarget(MissileWeapon missileWeapon)
		{
			Position position = world.GetComponent<Position>(missileWeapon);
			List<int> possibleTargets = world.EntitiesInArea(position.Center, missileWeapon.Range);

			// Always pick the closest target
			Position closestTargetPosition = null;
			foreach (var possibleTarget in possibleTargets)
			{
				if (possibleTarget == missileWeapon.EntityID ||
					world.GetOwningForce(possibleTarget).Team == world.GetOwningForce(missileWeapon.EntityID).Team ||
					world.GetOwningForce(possibleTarget).Team == Team.Neutral)
				{
					// Eliminate invalid targets
					continue;
				}


				Position possibleTargetPosition = world.GetComponent<Position>(possibleTarget);
				if (position.Distance(possibleTargetPosition) <= missileWeapon.Range &&
					(closestTargetPosition == null || position.Distance(closestTargetPosition) > position.Distance(possibleTargetPosition)))
				{
					closestTargetPosition = possibleTargetPosition;
				}
			}

			return closestTargetPosition;
		}


		public override void Draw(GameTime gameTime)
		{
			spriteBatch.Begin();

			foreach (var missileWeapon in world.GetComponents<MissileWeapon>())
			{
				Constructable constructable = world.GetComponent<Constructable>(missileWeapon);
				if (constructable.IsBeingPlaced)
				{
					// Draw attack range
					Position position = world.GetComponent<Position>(missileWeapon);
					spriteBatch.DrawEllipse(world.WorldToScreen(position.Center),
					                        missileWeapon.Range,
					                        Color.Red,
					                        world.HUD.DrawEllipseGuides);
				}
			}

			spriteBatch.End();
		}
	}
}
