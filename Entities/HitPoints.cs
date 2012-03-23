﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AsteroidOutpost.Entities.Eventing;
using AsteroidOutpost.Interfaces;
using AsteroidOutpost.Networking;
using AsteroidOutpost.Screens;

namespace AsteroidOutpost.Entities
{
	public class HitPoints : Component
	{
		private int totalHitPoints = 100;
		private float hitPoints = 100;
		
		
		// Events
		[EventReplication(EventReplication.ServerToClients)]
		public event Action<EntityHitPointsChangedEventArgs> HitPointsChangedEvent;


		public HitPoints(AsteroidOutpostScreen theGame, IComponentList componentList, int totalHitPoints)
			: base(theGame, componentList)
		{
			this.totalHitPoints = totalHitPoints;
			hitPoints = totalHitPoints;
		}


		public HitPoints(BinaryReader br)
			: base(br)
		{
			totalHitPoints = br.ReadInt32();
			hitPoints = br.ReadSingle();
		}

		public override void Serialize(BinaryWriter bw)
		{
			// Always serialize the base first because we can't pick the deserialization order
			base.Serialize(bw);

			bw.Write(totalHitPoints);
			bw.Write(hitPoints);
		}


		/// <summary>
		/// Sets the hit-points of this entity
		/// </summary>
		/// <param name="value">What to set the hit points to</param>
		public void Set(float value)
		{
			Set(value, theGame.IsServer);
		}


		/// <summary>
		/// Sets the hit-points of this entity
		/// </summary>
		/// <param name="value">What to set the hit points to</param>
		/// <param name="authoratative">If you are not authoritative, you will be treated like a client with no control</param>
		public void Set(float value, bool authoratative)
		{
			if (!authoratative)
			{
				return;
			}

			int initialHitPoints = (int)hitPoints;

			// Hit points can't go below zero
			hitPoints = Math.Max(0.0f, value);
			// Or above the max
			hitPoints = Math.Min(value, totalHitPoints);

			// Tell everyone that's interested in hit point changes
			if (initialHitPoints != hitPoints && HitPointsChangedEvent != null)
			{
				HitPointsChangedEvent(new EntityHitPointsChangedEventArgs(this, hitPoints, (int)(hitPoints) - initialHitPoints));
			}

			if (hitPoints <= 0.0f)
			{
				// We have just died
				SetDead(true);
			}
		}


		/// <summary>
		/// Gets the current hit points
		/// </summary>
		/// <returns>The current hit points</returns>
		public float Get()
		{
			return hitPoints;
		}


		/// <summary>
		/// Gets the total hit points
		/// </summary>
		/// <returns>The total hit points</returns>
		public int GetTotal()
		{
			return totalHitPoints;
		}
	}
}
