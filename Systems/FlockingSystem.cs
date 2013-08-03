using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteroidOutpost.Components;
using AsteroidOutpost.Interfaces;
using AsteroidOutpost.Screens;
using C3.XNA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AsteroidOutpost.Systems
{
	class FlockingSystem : DrawableGameComponent
	{
		private World world;
		private SpriteBatch spriteBatch;


		//private const int flockRadius = 800;
		//private const float cohesionFactor = 0.5f;
		//private const float separationFactor = 1f;
		//private const float alignmentFactor = 0.5f;

		public FlockingSystem(Game game, World world)
			: base(game)
		{
			this.world = world;
			spriteBatch = new SpriteBatch(game.GraphicsDevice);
		}


		public override void Update(GameTime gameTime)
		{
			if (world.Paused) { return; }

			IEnumerable<Flocking> flockers = world.GetComponents<Flocking>();
			//List<IGrouping<int, FleetMovementBehaviour>> fleetDictionary = fleetMovementBehaviours.GroupBy(x => x.FleetID).ToList();
			Dictionary<int, List<Flocking>> fleetDictionary = flockers.GroupBy(x => x.FlockID).ToDictionary(group => group.Key, group => group.ToList());
			

			foreach (var fleet in fleetDictionary.Values)
			{
				List<Position> fleetPositions = fleet.Select(x => world.GetComponent<Position>(x)).ToList();
				foreach (var vehicle in fleet)
				{

					if(vehicle.Target != null)
					{
						Position targetPosition = world.GetNullableComponent<Position>(vehicle.Target.Value);
						if(targetPosition == null)
						{
							vehicle.Target = null;
						}
					}

					Position position = world.GetComponent<Position>(vehicle);
					Velocity velocity = world.GetComponent<Velocity>(vehicle);
					if (vehicle.Target == null)
					{
						// Find a suitable target
						int vehicleEntityID = vehicle.EntityID;
						var livingThings = world.GetComponents<HitPoints>().Where(x => x.EntityID != vehicleEntityID &&
						                                                               x.IsAlive() &&
						                                                               world.GetOwningForce(x).Team != Team.Neutral &&
						                                                               world.GetOwningForce(x).Team != world.GetOwningForce(vehicleEntityID).Team);

						var livingThingPositions = livingThings.Select(x => world.GetComponent<Position>(x));
						Position closestLivingThing = null;
						foreach (var livingThingPosition in livingThingPositions)
						{
							var targetable = world.GetNullableComponent<Targetable>(livingThingPosition);
							if(targetable != null && (closestLivingThing == null || position.Distance(livingThingPosition) < position.Distance(closestLivingThing)))
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
						const int weaponGive = 10; // Just some number to help ships & towers get closer to each other and prevent float errors

						vehicle.TargetVector = Vector2.Normalize(targetPosition.Center - position.Center - (velocity.CurrentVelocity * 5)); // * vehicle.TargetVectorFactor;

						if (position.Distance(targetPosition) - (primaryWeapon.Range - weaponGive) > MinDistanceToStop(velocity, vehicle))
						{
							vehicle.AccelerationVector = vehicle.TargetVector; // + vehicle.Cohesion + vehicle.Separation + vehicle.Alignment;

							if (vehicle.AccelerationVector.Length() > 0)
							{
								velocity.CurrentVelocity += vehicle.AccelerationVector * vehicle.AccelerationMagnitude * (float)gameTime.ElapsedGameTime.TotalSeconds;
							}
						}
						else
						{
							Decelerate(velocity, vehicle, gameTime);
						}

						vehicle.Cohesion = Cohere(position, fleetPositions, vehicle.CohereNeighbourDistance, vehicle.SeparationDistance) * vehicle.CohesionFactor;
						vehicle.Separation = Separate(position, fleetPositions, vehicle.SeparationDistance) * vehicle.SeparationFactor;
						vehicle.Alignment = Align(position, fleetPositions) * vehicle.AlignmentFactor;

						float boidsAcceleration =  vehicle.AccelerationMagnitude * vehicle.AccelerationFactor;
						vehicle.BoidsVelocity = vehicle.BoidsVelocity * vehicle.BoidVelocityTrim;
						vehicle.BoidsVelocity += (vehicle.Cohesion + vehicle.Separation + vehicle.Alignment) * boidsAcceleration * (float)gameTime.ElapsedGameTime.TotalSeconds;


						if (vehicle.TargetVector != Vector2.Zero)
						{
							Animator animator = world.GetComponent<Animator>(vehicle);
							var facing = Vector2.Normalize(targetPosition.Center - position.Center);
							animator.SetOrientation(MathHelper.ToDegrees((float)Math.Atan2(facing.X, -facing.Y)), true);
						}
					}
				}


				foreach (var vehicle in fleet)
				{
					Position position = world.GetComponent<Position>(vehicle);
					//velocity.CurrentVelocity += vehicle.BoidsVelocity;
					position.Center += vehicle.BoidsVelocity;
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
		private Vector2 Cohere(Position myPosition, List<Position> flockMates, float neighbourDistance, float separationDistance)
		{
			Vector2 sum = Vector2.Zero;
			int count = 0;
			float satisfiedDistance = (separationDistance * 3);

			foreach (var matePosition in flockMates)
			{
				if (matePosition != myPosition &&
					myPosition.Distance(matePosition) >= satisfiedDistance &&
					myPosition.Distance(matePosition) < neighbourDistance)
				{
					sum += matePosition.Center;
					count++;
				}
			}

			if(count > 0)
			{
				sum = sum / (float)count;
				float distanceFromMean = myPosition.Distance(sum) - satisfiedDistance;
				sum = Vector2.Normalize(sum - myPosition.Center) * (distanceFromMean / (neighbourDistance - satisfiedDistance));
			}

			return sum;
		}


		/// <summary>
		/// Separate from your friends
		/// </summary>
		/// <param name="flockMates">A list of flock-mates to keep your distance from</param>
		/// <returns>A vector that determines the direction and magnitude to move in</returns>
		private Vector2 Separate(Position myPosition, List<Position> flockMates, float separationDistance)
		{
			Vector2 mean = Vector2.Zero;
			int count = 0;

			foreach(var nearbyEntity in world.EntitiesInArea(myPosition.Center, (int)separationDistance, true))
			{
				var nearbyPosition = world.GetComponent<Position>(nearbyEntity);
				if(myPosition != nearbyPosition)
				{
					float distance = myPosition.Distance(nearbyPosition);
					if (distance > 0 && distance < (separationDistance + nearbyPosition.Radius))
					{
						mean += Vector2.Normalize(myPosition.Center - nearbyPosition.Center) * ((separationDistance + nearbyPosition.Radius) / distance);
						count++;
					}
				}
			}

			if(count > 0)
			{
				mean = mean / count;
				//mean = Vector2.Normalize(mean);
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


		private float MinDistanceToStop(Velocity velocity, Flocking vehicle)
		{
			return (float)(0.5 * vehicle.AccelerationMagnitude * Math.Pow(velocity.CurrentVelocity.Length() / vehicle.AccelerationMagnitude, 2.0));
		}
		
		
		internal void AccelerateAlong(Velocity velocity, Flocking vehicle, GameTime gameTime)
		{
			
		}

		protected void Decelerate(Velocity velocity, Flocking vehicle, GameTime gameTime)
		{
			if (velocity.CurrentVelocity != Vector2.Zero)
			{
				if ((vehicle.AccelerationMagnitude * (float)gameTime.ElapsedGameTime.TotalSeconds) >= velocity.CurrentVelocity.Length())
				{
				//	velocity.CurrentVelocity = Vector2.Zero;
				}
				else
				{
					velocity.CurrentVelocity -= Vector2.Normalize(velocity.CurrentVelocity) * vehicle.AccelerationMagnitude * (float)gameTime.ElapsedGameTime.TotalSeconds;
				}
			}
		}


		public override void Draw(GameTime gameTime)
		{
			spriteBatch.Begin();
			foreach (var fleetBehaviour in world.GetComponents<Flocking>().Where(x => x.DebugView))
			{
				float scale = 20f;
				Position position = world.GetComponent<Position>(fleetBehaviour);
				spriteBatch.DrawLine(world.WorldToScreen(position.Center),
				                     world.WorldToScreen(position.Center + (fleetBehaviour.TargetVector) * scale),
				                     Color.Yellow,
				                     2);

				spriteBatch.DrawLine(world.WorldToScreen(position.Center),
				                     world.WorldToScreen(position.Center + (fleetBehaviour.Cohesion) * scale),
				                     Color.Red,
				                     2);

				spriteBatch.DrawLine(world.WorldToScreen(position.Center),
				                     world.WorldToScreen(position.Center + (fleetBehaviour.Separation) * scale),
				                     Color.Green,
				                     2);

				spriteBatch.DrawLine(world.WorldToScreen(position.Center),
				                     world.WorldToScreen(position.Center + (fleetBehaviour.Alignment) * scale),
				                     Color.Blue,
				                     2);
			}
			spriteBatch.End();
			base.Draw(gameTime);
		}
	}
}
