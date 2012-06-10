using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using AsteroidOutpost.Entities;
using AsteroidOutpost.Entities.Units;
using AsteroidOutpost.Screens;
using Microsoft.Xna.Framework;

namespace AsteroidOutpost
{
	public class WaveFactory
	{
		public static void CreateWave(World world, int pointValue, Vector2 roughPosition)
		{
			Controller aiActor = null;
			foreach (Controller actor in world.Controllers)
			{
				if (actor.Role == ControllerRole.AI)
				{
					aiActor = actor;
					break;
				}
			}


			if (aiActor == null)
			{
				// We didn't find an AI actor, that's weird
				Debugger.Break();
			}


			int aiCreated = 0;
			while (aiCreated < pointValue)
			{
				// Make something
				world.Add(new Ship1(world, world, aiActor.PrimaryForce, roughPosition));
				aiCreated += 100;
			}

		}
	}
}
