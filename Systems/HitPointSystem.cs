using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using AsteroidOutpost.Components;
using AsteroidOutpost.Eventing;
using AsteroidOutpost.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AsteroidOutpost.Systems
{
	class HitPointSystem : DrawableGameComponent
	{
		private readonly World world;
		private ParticleEffectManager particleEffectManager;
		//private SpriteBatch spriteBatch;

		public HitPointSystem(AOGame game, World world)
			: base(game)
		{
			this.world = world;
			particleEffectManager = game.ParticleEffectManager;
			//spriteBatch = new SpriteBatch(game.GraphicsDevice);
		}

		public override void Update(GameTime gameTime)
		{
			if (world.Paused) { return; }

			// TODO: This code will need to be changed to allow a client to delete entities
			bool authoratative = world.IsServer;
			foreach (var hitPoints in world.GetComponents<HitPoints>())
			{
				if (authoratative && !hitPoints.IsAlive())
				{
					// We have just died.
					Perishable perishable = world.GetNullableComponent<Perishable>(hitPoints);   // Only Nullable so I can create a custom error
					if(perishable != null)
					{
						perishable.OnPerish(new EntityPerishingEventArgs(perishable));

						if(perishable.ParticleEffectOnPerish != null)
						{
							Position perishablePosition = world.GetComponent<Position>(perishable);
							particleEffectManager.Trigger(perishable.ParticleEffectOnPerish, perishablePosition.Center);
						}
					}
					else
					{
						EntityName nameComponent = world.GetNullableComponent<EntityName>(hitPoints);
						String name = "NULL";
						if(nameComponent != null) { name = nameComponent.Name; }
						Console.WriteLine("Every entity that has HitPoints is by definition Perishable, but {0} has only HitPoints. Please add Perishable.", name);
						Debugger.Break();
					}
					
					world.DeleteComponents(hitPoints.EntityID);
				}
			}
		}


		public void Heal(HitPoints target, float amount)
		{
			InflictDamageOn(target, -amount);
		}

		public void InflictDamageOn(HitPoints victim, float damage)
		{
			int initialArmour = (int)victim.Armour;
			victim.Armour = MathHelper.Clamp(victim.Armour - damage, 0, victim.TotalArmour);
			int delta = (int)(victim.Armour) - initialArmour;
			if(delta != 0)
			{
				victim.OnArmourChanged(new EntityArmourChangedEventArgs(victim, delta));
			}

			if(damage >= 0)
			{
				AutoHeal autoHeal = world.GetNullableComponent<AutoHeal>(victim);
				if (autoHeal != null)
				{
					autoHeal.TimeSinceLastHit = TimeSpan.Zero;
				}
			}
		}
	}
}
