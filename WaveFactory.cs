using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using AsteroidOutpost.Entities;
using AsteroidOutpost.Screens;
using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;
using System.Globalization;

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
				// TODO: 2012-08-10 Fix spawning bad guys
				// Make something
				EntityFactory.Create("Spaceship", aiActor.PrimaryForce, new JObject{
					{ "Position", new JObject{
						{ "Center", String.Format(CultureInfo.InvariantCulture, "{0}, {1}", roughPosition.X + GlobalRandom.Next(-10, 10), roughPosition.Y + GlobalRandom.Next(-10, 10)) },
					}}
				});
				//new Dictionary<String, object>(){
				//        { "Sprite.Scale", 0.7f },
				//        { "Sprite.Set", null },
				//        { "Sprite.Animation", null },
				//        { "Sprite.Orientation", (float)GlobalRandom.Next(0, 359) },
				//        { "Transpose.Position", roughPosition },
				//        { "Transpose.Radius", 40 },
				//        { "OwningForce", aiActor.PrimaryForce }
				//    });
				//world.Add(new Ship1(world, world, aiActor.PrimaryForce, roughPosition));
				aiCreated += 100;
			}

		}
	}
}
