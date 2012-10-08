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
			foreach (var laser in world.GetComponents<LaserWeapon>())
			{
				Position position = world.GetComponent<Position>(laser);
				List<int> possibleTargets = world.EntitiesInArea(position.Center, laser.Range);

				// Always pick the closest target
				Position closestTargetPosition = null;
				foreach (var possibleTarget in possibleTargets)
				{
					if(possibleTarget == laser.EntityID ||
						world.GetOwningForce(possibleTarget).Team == world.GetOwningForce(laser.EntityID).Team ||
						world.GetOwningForce(possibleTarget).Team == Team.Neutral)
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

					HitPoints targetHitPoints = world.GetComponent<HitPoints>(laser.Target.Value);
					targetHitPoints.Armour -= (laser.Damage * (float)gameTime.ElapsedGameTime.TotalSeconds);
				}
			}
		}


		public override void Draw(GameTime gameTime)
		{
			spriteBatch.Begin();

			foreach (var laser in world.GetComponents<LaserWeapon>())
			{
				Constructable constructable = world.GetComponent<Constructable>(laser);
				if (constructable.IsBeingPlaced)
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
