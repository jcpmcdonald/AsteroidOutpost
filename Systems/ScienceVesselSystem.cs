using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteroidOutpost.Components;
using Microsoft.Xna.Framework;

namespace AsteroidOutpost.Systems
{
	class ScienceVesselSystem : DrawableGameComponent
	{
		private readonly World world;
		private readonly PowerGridSystem powerGridSystem;


		public ScienceVesselSystem(Game game, World world, PowerGridSystem powerGridSystem)
				: base(game)
		{
			this.world = world;
			this.powerGridSystem = powerGridSystem;
		}


		public override void Update(GameTime gameTime)
		{
			if (world.Paused) { return; }

			var scienceVessels = world.GetComponents<ScienceVessel>();
			foreach (var scienceVessel in scienceVessels)
			{
				if (world.GetNullableComponent<Constructing>(scienceVessel) != null && !scienceVessel.Overload)
				{
					// Ignore placing and constructing
					continue;
				}

				var battery = world.GetComponent<PowerStorage>(scienceVessel);

				float powerToConsume = scienceVessel.PowerConsumptionRate * (float)gameTime.ElapsedGameTime.TotalSeconds;
				bool gotPower = powerGridSystem.GetPower(scienceVessel, powerToConsume);
				if (!gotPower)
				{
					// Pull from my battery
					if (battery.AvailablePower >= powerToConsume)
					{
						// Bypass the power access lock
						battery.AvailablePower -= powerToConsume;
						gotPower = true;
					}
				}

				if (!gotPower)
				{
					scienceVessel.Overload = true;;
					return;
				}

				// Force-Fill up my battery
				//float requiredPower = battery.MaxPower - battery.AvailablePower;
				//if (requiredPower > 0)
				//{
				//	float replenishAmount = Math.Min(requiredPower, scienceVessel.StoredPowerReplenishRate * (float)gameTime.ElapsedGameTime.TotalSeconds);
				//	gotPower = powerGridSystem.GetPower(scienceVessel, replenishAmount);
				//	if (gotPower)
				//	{
				//		if (replenishAmount == requiredPower)
				//		{
				//			battery.AvailablePower = battery.MaxPower;
				//		}
				//		else
				//		{
				//			battery.AvailablePower += replenishAmount;
				//		}
				//	}
				//}
			}

			base.Update(gameTime);
		}
	}
}
