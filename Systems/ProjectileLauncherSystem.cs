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
	class ProjectileLauncherSystem : DrawableGameComponent
	{
		private readonly World world;
		private SpriteBatch spriteBatch;

		public ProjectileLauncherSystem(Game game, World world)
			: base(game)
		{
			this.world = world;
			spriteBatch = new SpriteBatch(game.GraphicsDevice);
		}


		public override void Update(GameTime gameTime)
		{
			if (world.Paused) { return; }

			foreach (var projectileLauncher in world.GetComponents<ProjectileLauncher>())
			{
				projectileLauncher.TimeSinceLastShot += gameTime.ElapsedGameTime;

				Constructing constructing = world.GetNullableComponent<Constructing>(projectileLauncher);
				if (constructing != null || projectileLauncher.FireMode == ProjectileLauncher.FiringMode.HoldFire) { continue; }


				Fire(projectileLauncher);
				
			}


		}


		private bool inRange(Position position, Position targetPosition, float range)
		{
			return position.Distance(targetPosition) - position.Radius - targetPosition.Radius <= range;
		}


		public bool Fire(ProjectileLauncher projectileLauncher)
		{
			bool hasFired = false;

			Position position = world.GetComponent<Position>(projectileLauncher);
			Targeting targeting = world.GetComponent<Targeting>(projectileLauncher);

			if (targeting.Target.HasValue)
			{
				Position targetPosition = world.GetComponent<Position>(targeting.Target.Value);

				if (projectileLauncher.TimeSinceLastShot.TotalMilliseconds > projectileLauncher.FireRate &&
				    inRange(position, targetPosition, projectileLauncher.Range))
				{
					hasFired = true;

					Vector2 accelerationVector = Vector2.Normalize(targetPosition.Center - position.Center);

					Matrix sprayMatrix = Matrix.CreateRotationZ(GlobalRandom.Next(-projectileLauncher.Spray, projectileLauncher.Spray));
					Vector2.Transform(ref accelerationVector, ref sprayMatrix, out accelerationVector);

					Vector2 initialVelocity = accelerationVector * GlobalRandom.Next(projectileLauncher.InitialVelocityMin, projectileLauncher.InitialVelocityMax);


					int projectileID = world.Create(projectileLauncher.ProjectileType, world.GetOwningForce(projectileLauncher), new JObject{
						{
							"Position", new JObject{
								{ "Center", String.Format(CultureInfo.InvariantCulture, "{0}, {1}", position.Center.X, position.Center.Y) },
							}
						},{
							"Velocity", new JObject{
								{ "CurrentVelocity", String.Format(CultureInfo.InvariantCulture, "{0}, {1}", initialVelocity.X, initialVelocity.Y) },
							}
						},{
							"Projectile", new JObject{
								{ "AccelerationVector", String.Format(CultureInfo.InvariantCulture, "{0}, {1}", accelerationVector.X, accelerationVector.Y) }
							}
						},
					});

					if (projectileLauncher.Damage.HasValue)
					{
						world.GetComponent<Projectile>(projectileID).Damage = projectileLauncher.Damage.Value;
					}

					Animator projectileAnimator = world.GetComponent<Animator>(projectileID);
					projectileAnimator.SetOrientation(MathHelper.ToDegrees((float)Math.Atan2(accelerationVector.X, -accelerationVector.Y)), true);

					projectileLauncher.TimeSinceLastShot = TimeSpan.Zero;
				}

			}

			return hasFired;
		}


		public override void Draw(GameTime gameTime)
		{
			spriteBatch.Begin();

			foreach (var missileWeapon in world.GetComponents<ProjectileLauncher>())
			{
				Constructing constructable = world.GetNullableComponent<Constructing>(missileWeapon);
				if (constructable != null && constructable.IsBeingPlaced)
				{
					// Draw attack range
					Position position = world.GetComponent<Position>(missileWeapon);
					spriteBatch.DrawEllipse(world.WorldToScreen(position.Center),
					                        world.Scale(missileWeapon.Range + position.Radius),
					                        Color.Red);
				}
			}

			spriteBatch.End();
		}
	}
}
