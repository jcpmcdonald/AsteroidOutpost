using System;
using System.IO;
using AsteroidOutpost.Entities.Eventing;
using AsteroidOutpost.Interfaces;
using AsteroidOutpost.Networking;
using AsteroidOutpost.Screens;
using Microsoft.Xna.Framework;

namespace AsteroidOutpost.Entities.Units
{
	public abstract class Ship : Entity
	{


		//protected float desiredAngle;
		//protected float angle;
		//protected float acceleration;
		//protected Vector2 accelerationVector = new Vector2(0, 1);
		protected float accelerationMagnitude;

		private Entity target;
		private int targetID;

		[EventReplication(EventReplication.ServerToClients)]
		public event Action<EntityTargetChangedEventArgs> TargetChangedEvent;

		protected Ship(World world, IComponentList componentList, Force theowningForce, Vector2 theCenter, int theRadius)
			: base(world, componentList, theowningForce, theCenter, theRadius, 30)
		{
		}


		protected Ship(BinaryReader br) : base(br)
		{
			//angle = br.ReadSingle();
			//accelerationVector = br.ReadVector2();
			accelerationMagnitude = br.ReadSingle();

			targetID = br.ReadInt32();
		}


		/// <summary>
		/// Serialize this Entity
		/// </summary>
		/// <param name="bw">The BinaryWriter to stream to</param>
		public override void Serialize(BinaryWriter bw)
		{
			base.Serialize(bw);

			//bw.Write(angle);
			//bw.Write(accelerationVector);
			bw.Write(accelerationMagnitude);

			if (target != null)
			{
				bw.Write(target.EntityID);
			}
			else
			{
				bw.Write(-1);
			}
		}


		/// <summary>
		/// After deserializing, this should be called to link this object to other objects
		/// </summary>
		/// <param name="world"></param>
		//public override void PostDeserializeLink(World world)
		//{
		//    base.PostDeserializeLink(world);

		//    if (targetID >= 0)
		//    {
		//        target = world.GetEntity(targetID);
		//    }
		//    else
		//    {
		//        target = null;
		//    }
		//}


		public Entity Target
		{
			get { return target; }
		}

		public void SetTarget(int entityID)
		{
			SetTarget(entityID, world.IsServer);
		}

		public void SetTarget(int entityID, bool authoritative)
		{
			Entity newTarget = null;
			if(entityID >= 0)
			{
				newTarget = world.GetEntity(entityID);
			}
			if (newTarget != null)
			{
				SetTarget(newTarget, authoritative);
			}
		}

		public void SetTarget(Entity entity)
		{
			SetTarget(entity, world.IsServer);
		}

		public void SetTarget(Entity entity, bool authoritative)
		{
			//if (!authoritative)
			//{
			//	return;
			//}

			// Detach from the old target
			if (target != null)
			{
				target.DyingEvent -= targetDied;
			}

			if(target != entity)
			{
				target = entity;

				if(TargetChangedEvent != null)
				{
					TargetChangedEvent(new EntityTargetChangedEventArgs(this, target));
				}
			}

			// And attach to the new target
			if (target != null)
			{
				target.DyingEvent += targetDied;
			}
		}



		public void targetDied(EntityDyingEventArgs e)
		{
			// Clear the target so that our controller recognizes that we have nothing to do
			SetTarget(null);
		}


		internal void accelerateAlong(Vector2 accelerationVector, TimeSpan deltaTime)
		{
			accelerationVector.Normalize();
			Position.Velocity += accelerationVector * accelerationMagnitude * (float)deltaTime.TotalSeconds;
		}

		protected void decelerate(TimeSpan deltaTime)
		{
			if (Position.Velocity.X != 0 || Position.Velocity.Y != 0)
			{
				if (accelerationMagnitude >= Position.Velocity.Length())
				{
					Position.Velocity = Vector2.Zero;
				}
				else
				{
					Position.Velocity -= Vector2.Normalize(Position.Velocity) * accelerationMagnitude;
				}
			}
		}


		
		protected float minDistanceToStop()
		{
			return (float)(0.5 * accelerationMagnitude * Math.Pow(Position.Velocity.Length() / accelerationMagnitude, 2.0));
		}

	}
}