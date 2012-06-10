using System.Diagnostics;
using AsteroidOutpost.Screens;
using C3.XNA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace AsteroidOutpost.Entities.Weapons
{
	class Laser : Weapon
	{

		private enum LaserState
		{
			IDLE,
			SHOOTING
		}

		private LaserState state = LaserState.IDLE;

		public Laser(World world, Entity theOwner)
			: base(world, theOwner)
		{
			OptimumRange = 130;
			MaxRange = 150;
		}


		public override void Shoot(Entity theTarget)
		{
			// Make sure we are in-range
			if (owner.Position.Distance(theTarget.Position) <= MaxRange)
			{
				// Shoot a frikkin' laser beam!
				state = LaserState.SHOOTING;
				target = theTarget;
			}
			else
			{
				state = LaserState.IDLE;
				target = null;
			}
		}


		public override void Update(TimeSpan deltaTime)
		{
			if(state == LaserState.SHOOTING)
			{
				// Make sure we are in-range
				if (owner.Position.Distance(target.Position) <= MaxRange && target.HitPoints.Get() > 0)
				{
					Debug.Assert(target != owner && target.OwningForce != owner.OwningForce, "Why are you hitting yourself? Why are you hitting yourself?");
					target.HitPoints.Set(target.HitPoints.Get() - (float)(10.0 * deltaTime.TotalSeconds));
				}
				else
				{
					state = LaserState.IDLE;
					target = null;
				}
			}
		}


		public override void Draw(SpriteBatch spriteBatch, Color tint)
		{
			if (state == LaserState.SHOOTING)
			{
				//Vector2 ownerCenterOnScreen = new Vector2(owner.Center.X - world.Hud.FocusScreen.X, owner.Center.Y - world.Hud.FocusScreen.Y);
				//Vector2 targetCenterOnScreen = new Vector2(target.Center.X - world.Hud.FocusScreen.X, target.Center.Y - world.Hud.FocusScreen.Y);
				spriteBatch.DrawLine(world.WorldToScreen(owner.Position.Center), world.WorldToScreen(target.Position.Center), Color.Red);
			}
		}
	}
}
