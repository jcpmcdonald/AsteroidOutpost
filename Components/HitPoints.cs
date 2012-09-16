using System;
using System.IO;
using AsteroidOutpost.Entities;
using AsteroidOutpost.Entities.Eventing;
using AsteroidOutpost.Eventing;
using AsteroidOutpost.Interfaces;
using AsteroidOutpost.Networking;
using AsteroidOutpost.Screens;

namespace AsteroidOutpost.Components
{
	public class HitPoints : Component
	{
		private int totalHitPoints = 100;
		private float hitPoints = 100;
		
		
		// Events
		[EventReplication(EventReplication.ServerToClients)]
		public event Action<EntityHitPointsChangedEventArgs> HitPointsChangedEvent;


		public HitPoints(World world, int entityID, int totalHitPoints)
			: base(world, entityID)
		{
			this.totalHitPoints = totalHitPoints;
			hitPoints = totalHitPoints;
		}


		//public HitPoints(BinaryReader br)
		//    : base(br)
		//{
		//    totalHitPoints = br.ReadInt32();
		//    hitPoints = br.ReadSingle();
		//}

		//public override void Serialize(BinaryWriter bw)
		//{
		//    // Always serialize the base first because we can't pick the deserialization order
		//    base.Serialize(bw);

		//    bw.Write(totalHitPoints);
		//    bw.Write(hitPoints);
		//}


		/// <summary>
		/// Sets the hit-points of this entity
		/// </summary>
		/// <param name="value">What to set the hit points to</param>
		public void SetHitPoints(float value)
		{
			SetHitPoints(value, world.IsServer);
		}


		/// <summary>
		/// Sets the hit-points of this entity
		/// </summary>
		/// <param name="value">What to set the hit points to</param>
		/// <param name="authoratative">If you are not authoritative, you will be treated like a client with no control</param>
		public void SetHitPoints(float value, bool authoratative)
		{
			int initialHitPoints = (int)hitPoints;

			// Hit points can't go below zero
			hitPoints = Math.Max(0.0f, value);
			// Or above the max
			hitPoints = Math.Min(value, totalHitPoints);

			// Tell everyone that's interested in hit point changes
			if (initialHitPoints != hitPoints && HitPointsChangedEvent != null)
			{
				//HitPointsChangedEvent(new EntityHitPointsChangedEventArgs(this, hitPoints, (int)(hitPoints) - initialHitPoints));
			}

			if (authoratative && hitPoints <= 0.0f)
			{
				// We have just died
				SetDead(true);
			}
		}


		/// <summary>
		/// Gets the current hit points
		/// </summary>
		/// <returns>The current hit points</returns>
		public float GetHitPoints()
		{
			return hitPoints;
		}


		/// <summary>
		/// Gets the total hit points
		/// </summary>
		/// <returns>The total hit points</returns>
		public int GetTotalHitPoints()
		{
			return totalHitPoints;
		}
	}
}
