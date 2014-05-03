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
				Constructing constructing = world.GetNullableComponent<Constructing>(laser);
				if (constructing != null) { return; }

				Position position = world.GetComponent<Position>(laser);
				Targeting targeting = world.GetComponent<Targeting>(laser);

				if (targeting.Target.HasValue)
				{
					Position targetPosition = world.GetComponent<Position>(targeting.Target.Value);
					if (inRange(position, targetPosition, laser.Range))
					{
						// Extract some power
						if (laser.PowerUsageRate > 0)
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


						if (laser.HasPower)
						{
							HitPoints targetHitPoints = world.GetNullableComponent<HitPoints>(targeting.Target.Value);
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
		}


		private bool inRange(Position position, Position targetPosition, float range)
		{
			return position.Distance(targetPosition) - position.Radius - targetPosition.Radius <= range;
		}


		public override void Draw(GameTime gameTime)
		{
			spriteBatch.Begin();

			foreach (var laser in world.GetComponents<LaserWeapon>())
			{
				Constructing constructable = world.GetNullableComponent<Constructing>(laser);
				Position position = world.GetComponent<Position>(laser);
				if (constructable != null && constructable.IsBeingPlaced)
				{
					// Draw attack range
					spriteBatch.DrawEllipse(world.WorldToScreen(position.Center),
					                        world.Scale(laser.Range + position.Radius),
					                        Color.Red);
				}
				else
				{
					Targeting targeting = world.GetComponent<Targeting>(laser);
					if (targeting.Target != null)
					{
						Position targetPosition = world.GetComponent<Position>(targeting.Target.Value);
						if (targeting.Target != null && laser.HasPower && inRange(position, targetPosition, laser.Range))
						{
							if (targetPosition != null)
							{
								spriteBatch.DrawLine(world.WorldToScreen(position.Center),
								                     world.WorldToScreen(targetPosition.Center),
								                     laser.Color);
							}
						}
					}
				}
			}

			spriteBatch.End();
		}
	}
}
