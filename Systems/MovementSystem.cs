using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteroidOutpost.Components;
using AsteroidOutpost.Interfaces;
using AsteroidOutpost.Screens;
using Microsoft.Xna.Framework;

namespace AsteroidOutpost.Systems
{
	class MovementSystem : GameComponent
	{
		private World world;


		private const int flockRadius = 800;
		private const float cohesionFactor = 0.5f;
		private const float separationFactor = 1f;
		private const float alignmentFactor = 0.5f;

		public MovementSystem(Game game, World world)
			: base(game)
		{
			this.world = world;
		}


		public override void Update(GameTime gameTime)
		{
			List<FleetMovementBehaviour> fleetMovementBehaviours = world.GetComponents<FleetMovementBehaviour>();
			//List<IGrouping<int, FleetMovementBehaviour>> fleetDictionary = fleetMovementBehaviours.GroupBy(x => x.FleetID).ToList();
			Dictionary<int, List<FleetMovementBehaviour>> fleetDictionary = fleetMovementBehaviours.GroupBy(x => x.FleetID).ToDictionary(group => group.Key, group => group.ToList());
			

			foreach (var fleet in fleetDictionary.Values)
			{
				List<Position> fleetPositions = fleet.Select(x => world.GetComponent<Position>(x)).ToList();
				foreach (var vehicle in fleet)
				{
					Position position = world.GetComponent<Position>(vehicle);
					if (vehicle.Target == null)
					{
						// Find a suitable target
						int vehicleEntityID = vehicle.EntityID;
						var livingThings = world.GetComponents<HitPoints>().Where(x => x.EntityID != vehicleEntityID &&
						                                                               x.GetHitPoints() > 0 &&
						                                                               world.GetOwningForce(x.EntityID).Team != Team.Neutral &&
						                                                               world.GetOwningForce(x.EntityID).Team != world.GetOwningForce(vehicleEntityID).Team);

						var livingThingPositions = livingThings.Select(x => world.GetComponent<Position>(x));
						Position closestLivingThing = null;
						foreach (var livingThingPosition in livingThingPositions)
						{
							if(closestLivingThing == null || position.Distance(livingThingPosition) < position.Distance(closestLivingThing))
							{
								closestLivingThing = livingThingPosition;
							}
						}
						
						if(closestLivingThing != null)
						{
							vehicle.Target = closestLivingThing.EntityID;
						}
						else
						{
							vehicle.Target = null;
						}
					}

					if(vehicle.Target != null)
					{
						Position targetPosition = world.GetComponent<Position>(vehicle.Target.Value);
						List<IWeapon> weapons = world.GetWeapons(vehicle);
						IWeapon primaryWeapon = weapons.First(x => x.Range == weapons.Min(y => y.Range));

						if (position.Distance(targetPosition) - primaryWeapon.Range > MinDistanceToStop(position, vehicle))
						{
							// Move toward the target and flock with my flock-mates
							vehicle.AccelerationVector = targetPosition.Center - position.Center - position.Velocity;
							vehicle.AccelerationVector.Normalize();

							Vector2 cohesion = Cohere(position, fleetPositions) * cohesionFactor;
							Vector2 separation = Separate(position, fleetPositions) * separationFactor;
							Vector2 alignment = Vector2.Zero; // align(fleetPositionDictionary) * alignmentFactor;

							vehicle.AccelerationVector = (vehicle.AccelerationVector * 5) + cohesion + separation + alignment;
							vehicle.AccelerationVector.Normalize();

							Animator animator = world.GetComponent<Animator>(vehicle);
							animator.SetOrientation(MathHelper.ToDegrees((float)Math.Atan2(vehicle.AccelerationVector.X, -vehicle.AccelerationVector.Y)), true);

							AccelerateAlong(position, vehicle, gameTime);
						}
						else
						{
							Decelerate(position, vehicle, gameTime);
						}

					}
				}

				//// Find my flock-mates
				//List<Position> flockMates = new List<Position>(10);
				//foreach (var possibleFlockMate in fleetMovementBehaviours)
				//{
				//    if(possibleFlockMate == self || possibleFlockMate.IsDead())
				//    {
				//        continue;
				//    }

				//    Position possibleFlockMatePosition = world.GetComponent<Position>(possibleFlockMate);
				//    if (world.GetOwningForce(possibleFlockMate.EntityID).Team == world.GetOwningForce(self.EntityID).Team &&
				//        position.Distance(possibleFlockMatePosition) <= flockRadius)
				//    {
				//        flockMates.Add(possibleFlockMatePosition);
				//    }
				//}

			}
		}


		/// <summary>
		/// Stick together with your friends
		/// </summary>
		/// <param name="flockMates">A list of flock-mates to keep together with</param>
		/// <returns>A vector that determines the direction and magnitude to move in</returns>
		private Vector2 Cohere(Position position, List<Position> flockMates)
		{
			const int neighbourDistance = 200;
			Vector2 sum = Vector2.Zero;
			int count = 0;

			foreach (var mate in flockMates)
			{
				if(mate != position && position.Distance(mate) > 0 && position.Distance(mate) < neighbourDistance)
				{
					sum += mate.Center;
					count++;
				}
			}

			if(count > 0)
			{
				sum = Vector2.Normalize(sum / count);
			}

			return sum;
		}


		/// <summary>
		/// Separate from your friends
		/// </summary>
		/// <param name="flockMates">A list of flock-mates to keep your distance from</param>
		/// <returns>A vector that determines the direction and magnitude to move in</returns>
		private Vector2 Separate(Position position, List<Position> flockMates)
		{
			const int separationDistance = 50;
			Vector2 mean = Vector2.Zero;
			int count = 0;


			foreach(var mate in flockMates)
			{
				float distance = position.Distance(mate);
				if(distance > 0 && distance < separationDistance)
				{
					mean += Vector2.Normalize(position.Center - mate.Center) / distance * 10;
					count++;
				}
			}

			if(count > 1)
			{
				mean /= count;
			}
			return mean;
		}

		private Vector2 Align(Position position, List<Position> flockMates)
		{
			return Vector2.Zero;
		}


		///// <summary>
		///// A helper method to set the current Orientation for the animator
		///// </summary>
		///// <param name="direction">The direction the graphic should look like it's facing</param>
		//private void SetOrientation(FleetMovementBehaviour vehicle, Vector2 direction)
		//{
		//    Animator animator = world.GetComponent<Animator>(vehicle);
		//    //animator.SpriteAnimator.
		//    float angle = (float)Math.Atan2(direction.X, -direction.Y);
		//    float angleStep = 360.0f / animator.SpriteAnimator. .OrientationLookup.Count;
		//    float orientation = ((int)((MathHelper.ToDegrees(angle) + (angleStep / 2)) / angleStep)) * angleStep;

		//    while (orientation < 0)
		//    {
		//        orientation += 360f;
		//    }
		//    while (orientation >= 360)
		//    {
		//        orientation -= 360f;
		//    }

		//    animator.SetOrientation(orientation);
		//    angleDiff = angle - MathHelper.ToRadians(orientation);
		//}


		private float MinDistanceToStop(Position position, FleetMovementBehaviour vehicle)
		{
			return (float)(0.5 * vehicle.AccelerationMagnitude * Math.Pow(position.Velocity.Length() / vehicle.AccelerationMagnitude, 2.0));
		}
		
		
		internal void AccelerateAlong(Position position, FleetMovementBehaviour vehicle, GameTime gameTime)
		{
			vehicle.AccelerationVector = Vector2.Normalize(vehicle.AccelerationVector);
			position.Velocity += vehicle.AccelerationVector * vehicle.AccelerationMagnitude * (float)gameTime.ElapsedGameTime.TotalSeconds;
		}

		protected void Decelerate(Position position, FleetMovementBehaviour vehicle, GameTime gameTime)
		{
			if (position.Velocity.X != 0 || position.Velocity.Y != 0)
			{
				if ((vehicle.AccelerationMagnitude * (float)gameTime.ElapsedGameTime.TotalSeconds) >= position.Velocity.Length())
				{
					position.Velocity = Vector2.Zero;
				}
				else
				{
					position.Velocity -= Vector2.Normalize(position.Velocity) * vehicle.AccelerationMagnitude * (float)gameTime.ElapsedGameTime.TotalSeconds;
				}
			}
		}
	}
}
