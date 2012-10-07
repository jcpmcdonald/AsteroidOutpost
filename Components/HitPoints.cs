using System;
using System.IO;
using AsteroidOutpost.Entities;
using AsteroidOutpost.Entities.Eventing;
using AsteroidOutpost.Eventing;
using AsteroidOutpost.Interfaces;
using AsteroidOutpost.Networking;
using AsteroidOutpost.Screens;
using Microsoft.Xna.Framework;

namespace AsteroidOutpost.Components
{
	public class HitPoints : Component
	{
		private float armour;

		// Events
		[EventReplication(EventReplication.ServerToClients)]
		public event Action<EntityArmourChangedEventArgs> ArmourChangedEvent;

		[EventReplication(EventReplication.ServerToClients)]
		public event Action<EntityDyingEventArgs> DyingEvent;


		public HitPoints(World world, int entityID, int totalArmour)
			: base(world, entityID)
		{
			TotalArmour = totalArmour;
			armour = totalArmour;
		}


		/// <summary>
		/// Gets the current hit points
		/// </summary>
		/// <value> The current hit points </value>
		public float Armour
		{
			get
			{
				return armour;
			}
			set
			{
				int initialHitPoints = (int)armour;
				armour = MathHelper.Clamp(value, 0, TotalArmour);
				int delta = (int)(armour) - initialHitPoints;

				// Tell everyone that's interested in hit point changes
				if (delta != 0 && ArmourChangedEvent != null)
				{
					ArmourChangedEvent(new EntityArmourChangedEventArgs(this, delta));
				}
			}
		}


		/// <summary>
		/// Gets the total hit points
		/// </summary>
		/// <value> The total hit points </value>
		public int TotalArmour { get; set; }


		public void OnDeath()
		{
			if(DyingEvent != null)
			{
				DyingEvent(new EntityDyingEventArgs(this));
			}
		}
	}
}
