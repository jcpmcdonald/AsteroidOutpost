using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using AsteroidOutpost.Components;
using AsteroidOutpost.Entities;
using AsteroidOutpost.Eventing;
using AsteroidOutpost.Screens;
using C3.XNA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using System.Globalization;

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
			if (world.Paused) { return; }

			foreach (var missileLauncher in world.GetComponents<MissileWeapon>())
			{
				Constructible constructible = world.GetComponent<Constructible>(missileLauncher);
				if(constructible.IsBeingPlaced || constructible.IsConstructing)
				{
					continue;
				}

				missileLauncher.TimeSinceLastShot += gameTime.ElapsedGameTime;
				Position closestTargetPosition = AcquireTarget(missileLauncher);
				missileLauncher.Target = closestTargetPosition != null ? (int?)closestTargetPosition.EntityID : null;

				// See if we can shoot yet
				if(missileLauncher.Target != null && missileLauncher.TimeSinceLastShot.TotalMilliseconds > missileLauncher.FireRate)
				{
					Position position = world.GetComponent<Position>(missileLauncher);
					Position targetPosition = world.GetComponent<Position>(missileLauncher.Target.Value);
					Vector2 accelerationVector = Vector2.Normalize(targetPosition.Center - position.Center);

					int missileID = EntityFactory.Create("Missile", world.GetOwningForce(missileLauncher), new JObject{
						{ "Position", new JObject{
							{ "Center", String.Format(CultureInfo.InvariantCulture, "{0}, {1}", position.Center.X, position.Center.Y) },
						}},
						{ "MissileProjectile", new JObject{
							{ "Damage", missileLauncher.Damage },
							{ "Acceleration", missileLauncher.Acceleration },
							{ "Target", missileLauncher.Target }
						}},
					});
					//new Dictionary<String, object>(){
					//    { "Sprite.Scale", 0.7f },
					//    { "Sprite.Set", null },
					//    { "Sprite.Animation", null },
					//    { "Sprite.Orientation", 0f },
					//    //{ "Sprite.RotateFrame", true },
					//    { "Transpose.Position", position.Center },
					//    { "Transpose.Radius", 10 },
					//    { "OwningForce", world.GetOwningForce(missileLauncher) },
					//    { "TargetEntityID", missileLauncher.Target }
					//});

					Animator missileAnimator = world.GetComponent<Animator>(missileID);
					missileAnimator.SetOrientation(MathHelper.ToDegrees((float)Math.Atan2(accelerationVector.X, -accelerationVector.Y)), true);

					missileLauncher.TimeSinceLastShot = TimeSpan.Zero;
				}

				if(missileLauncher.Target != null)
				{
					Position position = world.GetComponent<Position>(missileLauncher);
					Position targetPosition = world.GetComponent<Position>(missileLauncher.Target.Value);
					Vector2 directionToTarget = Vector2.Normalize(targetPosition.Center - position.Center);
					
					if(directionToTarget.Length() > 1.00001)
					{
						Debugger.Break();
					}

					// Rotate the tower to face the target:
					Animator towerAnimator = world.GetComponent<Animator>(missileLauncher);
					towerAnimator.SetOrientation(MathHelper.ToDegrees((float)Math.Atan2(directionToTarget.X, -directionToTarget.Y)), false);
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
					world.GetOwningForce(possibleTarget).Team == world.GetOwningForce(missileWeapon).Team ||
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
				Constructible constructable = world.GetComponent<Constructible>(missileWeapon);
				if (constructable.IsBeingPlaced)
				{
					// Draw attack range
					Position position = world.GetComponent<Position>(missileWeapon);
					spriteBatch.DrawEllipse(world.WorldToScreen(position.Center),
					                        world.Scale(missileWeapon.Range),
					                        Color.Red);
				}
			}

			spriteBatch.End();
		}
	}
}
