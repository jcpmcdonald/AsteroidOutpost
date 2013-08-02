using System;
using System.Collections.Generic;
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

		public LaserWeaponSystem(Game game, World world)
			: base(game)
		{
			this.world = world;
			spriteBatch = new SpriteBatch(game.GraphicsDevice);
		}

		public override void Update(GameTime gameTime)
		{
			if (world.Paused) { return; }

			foreach (var laser in world.GetComponents<LaserWeapon>())
			{
				Constructible constructible = world.GetNullableComponent<Constructible>(laser);
				if (constructible != null && constructible.IsConstructing || constructible.IsBeingPlaced) { return; }

				laser.Firing = false;

				Position position = world.GetComponent<Position>(laser);
				List<int> possibleTargets = world.EntitiesInArea(position.Center, laser.Range);

				// Always pick the closest target
				Position closestTargetPosition = null;
				foreach (var possibleTarget in possibleTargets)
				{
					if(possibleTarget == laser.EntityID ||
						world.GetOwningForce(possibleTarget).Team == world.GetOwningForce(laser).Team ||
						world.GetOwningForce(possibleTarget).Team == Team.Neutral ||
						world.GetNullableComponent<HitPoints>(possibleTarget) == null)
					{
						// Eliminate invalid targets
						continue;
					}

					Constructible constructibleTarget = world.GetNullableComponent<Constructible>(possibleTarget);
					if(constructibleTarget != null && constructibleTarget.IsBeingPlaced)
					{
						// Eliminate targets being constructed
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

					laser.Firing = true;
					HitPoints targetHitPoints = world.GetComponent<HitPoints>(closestTargetPosition.EntityID);
					targetHitPoints.Armour -= (laser.Damage * (float)gameTime.ElapsedGameTime.TotalSeconds);
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
					                        laser.Range,
					                        Color.Red,
					                        world.HUD.DrawEllipseGuides);
				}
				else if (laser.Target != null)
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
