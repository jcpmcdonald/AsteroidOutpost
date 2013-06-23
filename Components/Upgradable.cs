using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using AsteroidOutpost.Entities;
using AsteroidOutpost.Entities.Eventing;
using AsteroidOutpost.Networking;
using AsteroidOutpost.Screens;
using Microsoft.Xna.Framework;

namespace AsteroidOutpost.Components
{
	public class Upgradable : Component
	{
		protected List<Upgrade> allUpgrades = new List<Upgrade>();
		protected List<Upgrade> constructedUpgrades = new List<Upgrade>();
		private float mineralsUpgraded;


		//[EventReplication(EventReplication.ClientToServer)]
		//public event Action<EntityRequestUpgradeEventArgs> RequestUpgradeEvent;

		//[EventReplication(EventReplication.ServerToClients)]
		//public event Action<EntityUpgradeStartedEventArgs> UpgradeStartedEvent;

		//[EventReplication(EventReplication.ServerToClients)]
		//public event Action<EntityUpgradeProgressEventArgs> UpgradeProgressChangedEvent;

		//[EventReplication(EventReplication.ClientToServer)]
		//public event Action<EntityRequestUpgradeCancelEventArgs> RequestUpgradeCancelEvent;

		//[EventReplication(EventReplication.ServerToClients)]
		//public event Action<EntityUpgradeCancelledEventArgs> UpgradeCancelledEvent;


		// Local event only
		//public static event Action<EntityUpgradeEventArgs> AnyUpgradeCompletedEvent;
		//public event Action<EntityUpgradeEventArgs> UpgradeCompletedEvent;

		public Upgradable(int entityID) : base(entityID) {}
		public Upgradable(int entityID, params Upgrade[] upgrades)
			: base(entityID)
		{
			CurrentUpgrade = null;
			allUpgrades.AddRange(upgrades);
		}

		public virtual bool IsUpgrading { get; set; }
		public Upgrade CurrentUpgrade { get; set; }


		//private Upgrade GetUpgradeByName(String upgradeName)
		//{
		//    foreach(Upgrade u in allUpgrades)
		//    {
		//        if(u.Name == upgradeName)
		//        {
		//            return u;
		//        }
		//    }
		//    return null;
		//}


		/// <summary>
		/// How many minerals does this constructible take to upgrade?
		/// </summary>
		public int MineralsToUpgrade { get; set; }


		/// <summary>
		/// How many minerals are left to upgrade this constructible?
		/// </summary>
		public float MineralsUpgraded
		{
			get
			{
				return mineralsUpgraded;
			}
			set
			{
				mineralsUpgraded = MathHelper.Clamp(value, 0, MineralsToUpgrade);;
			}
		}


		//public void SetMineralsLeftToUpgrade(float value)
		//{
		//    bool changed = (MineralsUpgraded != value);

		//    MineralsUpgraded = value;

		//    if(changed)
		//    {
		//        // TODO: 2012-08-08 Fix this Event
		//        //if (UpgradeProgressChangedEvent != null)
		//        //{
		//        //    UpgradeProgressChangedEvent(new EntityUpgradeProgressEventArgs(this, mineralsLeftToUpgrade));
		//        //}


		//        if (MineralsUpgraded <= 0.0)
		//        {
		//            MineralsUpgraded = 0.0f;
		//            IsUpgrading = false;

		//            constructedUpgrades.Add(CurrentUpgrade);
		//            CurrentUpgrade.Complete();

		//            if (AnyUpgradeCompletedEvent != null)
		//            {
		//                // TODO: 2012-08-08 Fix this Event
		//                //AnyUpgradeCompletedEvent(new EntityUpgradeEventArgs(this, currentUpgrade));
		//            }
		//            if (UpgradeCompletedEvent != null)
		//            {
		//                // TODO: 2012-08-08 Fix this Event
		//                //UpgradeCompletedEvent(new EntityUpgradeEventArgs(this, currentUpgrade));
		//            }
		//            CurrentUpgrade = null;
		//        }
		//    }
		//}
		
		
		
		/// <summary>
		/// Returns a list of available upgrades
		/// </summary>
		/// <returns>List of available upgrades</returns>
		public List<Upgrade> AvailableUpgrades()
		{
			List<Upgrade> availUpgrades = new List<Upgrade>(allUpgrades);
			foreach(Upgrade u in constructedUpgrades)
			{
				availUpgrades.Remove(u);
			}
			for(int i = availUpgrades.Count - 1; i >= 0; i--)
			{
				if(!availUpgrades[i].CanUpgrade(constructedUpgrades))
				{
					availUpgrades.RemoveAt(i);
				}
			}
			return availUpgrades;
		}


		//public void StartUpgrade(String upgradeName)
		//{
		//    StartUpgrade(GetUpgradeByName(upgradeName), world.IsServer);
		//}

		//public void StartUpgrade(String upgradeName, bool authoritative)
		//{
		//    StartUpgrade(GetUpgradeByName(upgradeName), authoritative);
		//}

		//public void StartUpgrade(Upgrade u)
		//{
		//    StartUpgrade(u, world.IsServer);
		//}

		//public void StartUpgrade(Upgrade u, bool authoritative)
		//{
		//    if (u == null)
		//    {
		//        throw new ArgumentNullException("u");
		//    }

			
		//    CurrentUpgrade = u;
		//    MineralsToUpgrade = u.MineralCost;
		//    MineralsUpgraded = u.MineralCost;
		//    IsUpgrading = true;

		//    // TODO: 2012-08-08 Fix this Event
		//    //if(UpgradeStartedEvent != null)
		//    //{
		//    //    UpgradeStartedEvent(new EntityUpgradeStartedEventArgs(this, u));
		//    //}

		//    if (!authoritative)
		//    {
		//        // Request an upgrade
		//        // TODO: 2012-08-08 Fix this Event
		//        //if (RequestUpgradeEvent == null)
		//        //{
		//        //    // Nobody is listening to our cries
		//        //    Debugger.Break();
		//        //}
		//        //RequestUpgradeEvent(new EntityRequestUpgradeEventArgs(this, u));
		//    }
		//}



		//public void CancelUpgrade()
		//{
		//    CancelUpgrade(world.IsServer);
		//}

		//public void CancelUpgrade(bool authoritative)
		//{
		//    if (CurrentUpgrade == null || !IsUpgrading)
		//    {
		//        // Note: This may be alright in a laggy network, but for now
		//        // You can't cancel an upgrade if you aren't upgrading
		//        Debugger.Break();
		//    }

			
		//    // TODO: 2012-08-08 Give the user back half of their minerals for canceling
		//    // Give the user back half of their spent minerals
		//    int mineralsToGiveBack = (int)(((MineralsToUpgrade - MineralsUpgraded) * 0.5) + 0.5);
		//    //owningForce.SetMinerals(owningForce.GetMinerals() + mineralsToGiveBack);

		//    MineralsToUpgrade = 0;
		//    MineralsUpgraded = 0.0f;
		//    IsUpgrading = false;

		//    // TODO: 2012-08-08 Fix this Event
		//    //if (UpgradeCancelledEvent != null)
		//    //{
		//    //    UpgradeCancelledEvent(new EntityUpgradeCancelledEventArgs(this));
		//    //}


		//    if (!authoritative)
		//    {
		//        // Request cancel
		//        // TODO: 2012-08-08 Fix this Event
		//        //if (RequestUpgradeCancelEvent == null)
		//        //{
		//        //    // Nobody is listening to our cries
		//        //    Debugger.Break();
		//        //}
		//        //RequestUpgradeCancelEvent(new EntityRequestUpgradeCancelEventArgs(this));
		//    }
		//}

	}
}
