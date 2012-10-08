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
				Position position = world.GetComponent<Position>(missileWeapon);
				List<int> possibleTargets = world.EntitiesInArea(position.Center, missileWeapon.Range);

				// Always pick the closest target
				Position closestTargetPosition = null;
				foreach (var possibleTarget in possibleTargets)
				{
					if(possibleTarget == missileWeapon.EntityID ||
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

				if(closestTargetPosition == null)
				{
					missileWeapon.Target = null;
				}
				else
				{
					missileWeapon.Target = closestTargetPosition.EntityID;

					HitPoints targetHitPoints = world.GetComponent<HitPoints>(missileWeapon.Target.Value);
					targetHitPoints.Armour -= (missileWeapon.Damage * (float)gameTime.ElapsedGameTime.TotalSeconds);
				}
			}
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
