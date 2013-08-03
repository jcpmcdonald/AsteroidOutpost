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
		// Events
		[EventReplication(EventReplication.ServerToClients)]
		public event Action<EntityArmourChangedEventArgs> ArmourChanged;

		[EventReplication(EventReplication.ServerToClients)]
		public event Action<EntityDyingEventArgs> Dying;


		public HitPoints(int entityID) : base(entityID) {}
		public HitPoints(int entityID, int totalArmour)
			: base(entityID)
		{
			TotalArmour = totalArmour;
			Armour = totalArmour;
		}


		public float Armour { get; set; }
		public int TotalArmour { get; set; }


		public bool IsAlive()
		{
			return Armour > 0;
		}


		public void OnArmourChanged(EntityArmourChangedEventArgs e)
		{
			if (ArmourChanged != null)
			{
				ArmourChanged(e);
			}
		}

		public void OnDeath(EntityDyingEventArgs e)
		{
			if(Dying != null)
			{
				Dying(e);
			}
		}
	}
}
