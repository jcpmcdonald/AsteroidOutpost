using System;
using System.Collections.Generic;
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
		//private SpriteBatch spriteBatch;

		public HitPointSystem(Game game, World world)
			: base(game)
		{
			this.world = world;
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
					// We have just died
					hitPoints.OnDeath(new EntityDyingEventArgs(hitPoints));
					world.DeleteComponents(hitPoints.EntityID);
				}
			}
		}


		public static void InflictDamageOn(HitPoints victim, float damage)
		{
			int initialArmour = (int)victim.Armour;
			victim.Armour = MathHelper.Clamp(victim.Armour - damage, 0, victim.TotalArmour);
			int delta = (int)(victim.Armour) - initialArmour;
			if(delta != 0)
			{
				victim.OnArmourChanged(new EntityArmourChangedEventArgs(victim, delta));
			}
		}
	}
}
