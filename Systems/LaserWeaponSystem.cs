using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using AsteroidOutpost.Components;
using AsteroidOutpost.Screens;
using C3.XNA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AsteroidOutpost.Systems
{
	class LaserWeaponSystem : DrawableGameComponent
	{
		private readonly World world;
		private SpriteBatch spriteBatch;
		private readonly PowerGridSystem powerGridSystem;
		private readonly HitPointSystem hitPointSystem;

		public LaserWeaponSystem(Game game, World world, PowerGridSystem powerGridSystem, HitPointSystem hitPointSystem)
			: base(game)
		{
			this.world = world;
			this.powerGridSystem = powerGridSystem;
			this.hitPointSystem = hitPointSystem;
			spriteBatch = new SpriteBatch(game.GraphicsDevice);
		}

		public override void Update(GameTime gameTime)
		{
			if (world.Paused) { return; }

			foreach (var laser in world.GetComponents<LaserWeapon>())
			{
				Constructible constructible = world.GetNullableComponent<Constructible>(laser);
				if (constructible != null && (constructible.IsConstructing || constructible.IsBeingPlaced)) { return; }

				Position position = world.GetComponent<Position>(laser);
				List<int> possibleTargets = world.EntitiesInArea(position.Center, laser.Range);

				// Always pick the closest target
				Position closestTargetPosition = null;
				foreach (var possibleTarget in possibleTargets)
				{
					if(possibleTarget == laser.EntityID ||
						world.GetOwningForce(possibleTarget).Team == world.GetOwningForce(laser).Team ||
						world.GetOwningForce(possibleTarget).Team == Team.Neutral ||
						world.GetNullableComponent<Targetable>(possibleTarget) == null ||
						world.GetNullableComponent<Constructible>(possibleTarget) != null)
					{
						// Eliminate invalid targets
						continue;
					}


					Position possibleTargetPosition = world.GetComponent<Position>(possibleTarget);
					if (position.Distance(possibleTargetPosition) <= laser.Range &&
					    (closestTargetPosition == null || position.Distance(closestTargetPosition) > position.Distance(possibleTargetPosition)))
					{
						closestTargetPosition = possibleTargetPosition;
					}
				}

				if(closestTargetPosition == null)
				{
					laser.Target = null;
				}
				else
				{
					laser.Target = closestTargetPosition.EntityID;

					// Extract some power
					if(laser.PowerUsageRate > 0)
					{
						float powerToUse = laser.PowerUsageRate * (float)gameTime.ElapsedGameTime.TotalSeconds;
						Force owningForce = world.GetOwningForce(laser);
						if (powerGridSystem.GetPower(laser, powerToUse))
						{
							laser.HasPower = true;
						}
						else
						{
							laser.HasPower = false;
						}
					}
					else
					{
						// No power required for this laser
						laser.HasPower = true;
					}


					if(laser.HasPower)
					{
						HitPoints targetHitPoints = world.GetNullableComponent<HitPoints>(closestTargetPosition.EntityID);
						if (targetHitPoints != null)
						{
							float damage = (laser.Damage * (float)gameTime.ElapsedGameTime.TotalSeconds);
							hitPointSystem.InflictDamageOn(targetHitPoints, damage);
						}
						else
						{
							Console.WriteLine("Trying to shoot an invulnerable target! Target is Targetable, but has no HitPoints! What do we do?");
							Debugger.Break();
						}
					}

				}
			}
		}


		public override void Draw(GameTime gameTime)
		{
			spriteBatch.Begin();

			foreach (var laser in world.GetComponents<LaserWeapon>())
			{
				Constructible constructable = world.GetNullableComponent<Constructible>(laser);
				if (constructable != null && constructable.IsBeingPlaced)
				{
					// Draw attack range
					Position position = world.GetComponent<Position>(laser);
					spriteBatch.DrawEllipse(world.WorldToScreen(position.Center),
					                        world.Scale(laser.Range),
					                        Color.Red);
				}
				else if (laser.Target != null && laser.HasPower)
				{
					Position position = world.GetComponent<Position>(laser);
					Position targetPosition = world.GetNullableComponent<Position>(laser.Target.Value);
					if (targetPosition != null)
					{
						spriteBatch.DrawLine(world.WorldToScreen(position.Center),
						                     world.WorldToScreen(targetPosition.Center),
						                     laser.Color);
					}
				}
			}

			spriteBatch.End();
		}
	}
}
