using AsteroidOutpost.Components;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsteroidOutpost.Systems
{
	public class AIBasicSystem : GameComponent
	{
		private World world;

		public AIBasicSystem(AOGame game, World world)
			: base(game)
		{
			this.world = world;
		}

		public override void Update(GameTime gameTime)
		{
			if (world.Paused) { return; }

			//var aiBasicUsers = world.GetComponents<AIBasic>();
			//foreach(var aiBasicUser in aiBasicUsers)
			//{
			//	Position position = world.GetComponent<Position>(aiBasicUser);
				
			//	if (position.Distance(targetPosition) - (primaryWeapon.Range - weaponGive) > MinDistanceToStop(velocity, vehicle))
			//	{
			//		vehicle.AccelerationVector = vehicle.TargetVector; // + vehicle.Cohesion + vehicle.Separation + vehicle.Alignment;

			//		if (vehicle.AccelerationVector.Length() > 0)
			//		{
			//			velocity.CurrentVelocity += vehicle.AccelerationVector * vehicle.AccelerationMagnitude * (float)gameTime.ElapsedGameTime.TotalSeconds;
			//		}
			//	}
			//	else
			//	{
			//		Decelerate(velocity, vehicle, gameTime);
			//	}
			//}
		}

	}
}
