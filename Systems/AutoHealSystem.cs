using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteroidOutpost.Components;
using AsteroidOutpost.Eventing;
using Microsoft.Xna.Framework;

namespace AsteroidOutpost.Systems
{
	class AutoHealSystem : GameComponent
	{
		private readonly World world;
		private readonly HitPointSystem hitPointSystem;


		public AutoHealSystem(AOGame game, World world, HitPointSystem hitPointSystem)
			: base(game)
		{
			this.world = world;
			this.hitPointSystem = hitPointSystem;
		}


		public override void Update(GameTime gameTime)
		{
			if (world.Paused) { return; }

			foreach (var autoHealer in world.GetComponents<AutoHeal>())
			{
				autoHealer.TimeSinceLastHit += gameTime.ElapsedGameTime;

				HitPoints hitPoints = world.GetComponent<HitPoints>(autoHealer);
				if(hitPoints.Armour < hitPoints.TotalArmour && autoHealer.TimeSinceLastHit.TotalSeconds >= autoHealer.Delay)
				{
					hitPointSystem.Heal(hitPoints, (autoHealer.Rate * (float)gameTime.ElapsedGameTime.TotalSeconds));
				}
			}

			base.Update(gameTime);
		}
	}
}
