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
		public static void CreateWave(AsteroidOutpostScreen theGame, int pointValue, Vector2 roughPosition)
		{
			Actor aiActor = null;
			foreach (Actor actor in theGame.Actors)
			{
				if (actor.Role == ActorRole.AI)
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
				theGame.AddComponent(new Ship1(theGame, theGame, aiActor.PrimaryForce, roughPosition));
				aiCreated += 100;
			}

		}
	}
}
