using AsteroidOutpost.Components;
using AsteroidOutpost.Interfaces;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsteroidOutpost.Systems
{
	class AIStrafeSystem : GameComponent
	{
		private World world;

		public AIStrafeSystem(AOGame game, World world)
			: base(game)
		{
			this.world = world;
		}


		public override void Update(GameTime gameTime)
		{
			if (world.Paused) { return; }

			var aiStrafeUsers = world.GetComponents<AIStrafe>();
			foreach (var aiStrafeUser in aiStrafeUsers)
			{
				Targeting targeting = world.GetComponent<Targeting>(aiStrafeUser);
				if (targeting.Target == null) { continue; }

				Position position = world.GetComponent<Position>(aiStrafeUser);
				Velocity velocity = world.GetComponent<Velocity>(aiStrafeUser);
				List<IWeapon> weapons = world.GetWeapons(aiStrafeUser);

				Position targetPosition = world.GetComponent<Position>(targeting.Target.Value);

				float distanceToTarget = position.Distance(targetPosition);

				const int weaponGive = 10; // Just some number to help ships & towers get closer to each other and prevent float errors




				switch (aiStrafeUser.State)
				{
				case AIStrafe.StrafeState.Approach:
					targeting.TargetVector = Vector2.Normalize(targetPosition.Center - position.Center - (velocity.CurrentVelocity * 5));

					break;

				case AIStrafe.StrafeState.ShootMissiles:
					break;

				case AIStrafe.StrafeState.GetClose:
					break;

				case AIStrafe.StrafeState.FireLasers:
					break;
				}

			}
		}
	}
}
