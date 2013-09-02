using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteroidOutpost.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AsteroidOutpost.Systems
{
	class PowerProductionSystem : GameComponent
	{
		private World world;
		private readonly PowerGridSystem powerGridSystem;


		public PowerProductionSystem(AOGame game, World world, PowerGridSystem powerGridSystem)
			: base(game)
		{
			this.world = world;
			this.powerGridSystem = powerGridSystem;
		}


		public override void Update(GameTime gameTime)
		{
			if (world.Paused) { return; }

			foreach(var producer in world.GetComponents<PowerProducer>())
			{
				if(world.GetNullableComponent<Constructible>(producer) != null)
				{
					// Ignore placing and constructing producers
					continue;
				}

				float powerProduced = producer.PowerProductionRate * (float)gameTime.ElapsedGameTime.TotalSeconds;
				powerGridSystem.PutPower(producer, powerProduced);
			}
		}
	}
}
