using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using AsteroidOutpost.Entities.Eventing;
using AsteroidOutpost.Interfaces;
using AsteroidOutpost.Networking;
using AsteroidOutpost.Screens;
using C3.XNA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace AsteroidOutpost.Entities.Structures
{
	public abstract class ConstructableEntity : Entity
	{
		
		
		
		// TODO: Think of a better name for "nearbyPower" because it can also contain consumers
		/// <summary>
		/// Store a list of all nearby power conductors, and if we're a power conductor, also store consumers
		/// </summary>
		//protected List<PowerConductor> nearbyPower;


		protected ConstructableEntity(World world, IComponentList componentList, Force theowningForce, Vector2 theCenter, int theRadius, int totalHitPoints)
			: base(world, componentList, theowningForce, theCenter, theRadius, totalHitPoints)
		{
		}


		protected ConstructableEntity(BinaryReader br)
			: base(br)
		{

			//mineralsToUpgrade = br.ReadInt32();
			//mineralsLeftToUpgrade = br.ReadSingle();
			//isUpgrading = br.ReadBoolean();

			// Deserialize the upgrades
			//InitializeUpgrades();

			//int constructedUpgradesCount = br.ReadInt32();
			//for (int i = 0; i < constructedUpgradesCount; i++)
			//{
			//    constructedUpgrades.Add(GetUpgradeByName(br.ReadString()));
			//}

			//if (isUpgrading)
			//{
			//    currentUpgrade = GetUpgradeByName(br.ReadString());
			//}
		}


		/// <summary>
		/// Serialize this Entity
		/// </summary>
		/// <param name="bw">The BinaryWriter to stream to</param>
		public override void Serialize(BinaryWriter bw)
		{
			// Always serialize the base first because we can't pick the deserialization order
			//base.Serialize(bw);

			//bw.Write(level);

			//bw.Write(mineralsLeftToConstruct);
			//bw.Write(isConstructing);

			//bw.Write(mineralsToUpgrade);
			//bw.Write(mineralsLeftToUpgrade);
			//bw.Write(isUpgrading);

			//// Serialize the upgrades
			//bw.Write(constructedUpgrades.Count);
			//foreach(Upgrade constructedUpgrade in constructedUpgrades)
			//{
			//    bw.Write(constructedUpgrade.Name);
			//}

			//if (isUpgrading)
			//{
			//    bw.Write(currentUpgrade.Name);
			//}
		}


		
		
		
		/// <summary>
		/// Is this upgrading?
		/// </summary>
		//public virtual bool IsUpgrading
		//{
		//    get { return isUpgrading; }
		//    private set { isUpgrading = value; }
		//}
		
		
		
		/// <summary>
		/// Returns a list of available upgrades
		/// </summary>
		/// <returns>List of available upgrades</returns>
		//public List<Upgrade> AvailableUpgrades()
		//{
		//    List<Upgrade> availUpgrades = new List<Upgrade>(allUpgrades);
		//    foreach(Upgrade u in constructedUpgrades)
		//    {
		//        availUpgrades.Remove(u);
		//    }
		//    for(int i = availUpgrades.Count - 1; i >= 0; i--)
		//    {
		//        if(!availUpgrades[i].CanUpgrade(constructedUpgrades))
		//        {
		//            availUpgrades.RemoveAt(i);
		//        }
		//    }
		//    return availUpgrades;
		//}


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

			
		//    currentUpgrade = u;
		//    mineralsToUpgrade = u.MineralCost;
		//    mineralsLeftToUpgrade = u.MineralCost;
		//    isUpgrading = true;

		//    if(UpgradeStartedEvent != null)
		//    {
		//        UpgradeStartedEvent(new EntityUpgradeStartedEventArgs(this, u));
		//    }

		//    if (!authoritative)
		//    {
		//        // Request an upgrade
		//        if (RequestUpgradeEvent == null)
		//        {
		//            // Nobody is listening to our cries
		//            Debugger.Break();
		//        }
		//        RequestUpgradeEvent(new EntityRequestUpgradeEventArgs(this, u));
		//    }
		//}



		//public void CancelUpgrade()
		//{
		//    CancelUpgrade(world.IsServer);
		//}

		//public void CancelUpgrade(bool authoritative)
		//{
		//    if (currentUpgrade == null || !isUpgrading)
		//    {
		//        // Note: This may be alright in a laggy network, but for now
		//        // You can't cancel an upgrade if you aren't upgrading
		//        Debugger.Break();
		//    }

			
		//    // Give the user back half of their spent minerals
		//    int mineralsToGiveBack = (int)(((mineralsToUpgrade - mineralsLeftToUpgrade) * 0.5) + 0.5);
		//    owningForce.SetMinerals(owningForce.GetMinerals() + mineralsToGiveBack);

		//    mineralsToUpgrade = 0;
		//    mineralsLeftToUpgrade = 0.0f;
		//    isUpgrading = false;

		//    if (UpgradeCancelledEvent != null)
		//    {
		//        UpgradeCancelledEvent(new EntityUpgradeCancelledEventArgs(this));
		//    }


		//    if (!authoritative)
		//    {
		//        // Request cancel
		//        if (RequestUpgradeCancelEvent == null)
		//        {
		//            // Nobody is listening to our cries
		//            Debugger.Break();
		//        }
		//        RequestUpgradeCancelEvent(new EntityRequestUpgradeCancelEventArgs(this));
		//    }
		////}


		//public void CancelConstruction()
		//{
		//    CancelConstruction(world.IsServer);
		//}

		//public void CancelConstruction(bool authoritative)
		//{
		//    if (!isConstructing)
		//    {
		//        // Note: This may be alright in a laggy network, but for now:
		//        Console.WriteLine("You can't cancel constructing if you aren't constructing");
		//        Debugger.Break();
		//    }


		//    int mineralsToGiveBack = (int)(((MineralsToConstruct - mineralsLeftToConstruct) * 0.5) + 0.5);
		//    owningForce.SetMinerals(owningForce.GetMinerals() + mineralsToGiveBack);

		//    if (authoritative)
		//    {
		//        SetDead(true);
		//    }
		//    else
		//    {
		//        // Request a cancel
		//        if(RequestConstructionCancelEvent == null)
		//        {
		//            // Nobody is listening to our cries
		//            Debugger.Break();
		//        }
		//        RequestConstructionCancelEvent(new EntityRequestConstructionCancelEventArgs(this));
		//    }
		//}
		
		
		
		/// <summary>
		/// Update this upgrading building
		/// </summary>
		/// <param name="deltaTime"></param>
		/// <returns></returns>
		//protected virtual bool UpdateUpgrading(TimeSpan deltaTime)
		//{
		//    if(isUpgrading && currentUpgrade != null)
		//    {
		//        float powerToUse = powerUsageRate * (float)deltaTime.TotalSeconds;
		//        float mineralsToUse = mineralUsageRate * (float)deltaTime.TotalSeconds;

		//        // BUG: There is a disconnect between the check for minerals (below) and the actual consumption of minerals. Could cause weird behaviour
		//        if (owningForce.GetMinerals() > mineralsToUse && world.PowerGrid[owningForce.ID].GetPower(this, powerToUse))
		//        {
		//            // Use some minerals toward my upgrade
		//            int temp = (int)mineralsLeftToConstruct;
		//            SetMineralsLeftToUpgrade(mineralsLeftToUpgrade - mineralsToUse);

		//            // If the minerals left to construct has increased by a whole number, subtract it from the force's minerals
		//            if (temp > (int)mineralsLeftToConstruct)
		//            {
		//                owningForce.SetMinerals(owningForce.GetMinerals() - (temp - (int)mineralsLeftToConstruct));
		//            }
		//        }
		//    }
		//    return true;
		//}


		/// <summary>
		/// Draw this entity to the screen
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="scaleModifier"></param>
		/// <param name="tint"></param>
		//public override void Draw(SpriteBatch spriteBatch, float scaleModifier, Color tint)
		//{
		//    if(IsConstructing)
		//    {
		//        DrawConstructing(spriteBatch, scaleModifier, tint);
		//    }
		//    //else if(isUpgrading)
		//    //{
		//    //    DrawUpgrading(spriteBatch, scaleModifier, tint);
		//    //}
		//    else
		//    {
		//        base.Draw(spriteBatch, scaleModifier, tint);

		//        //DrawPowerConnections(spriteBatch);
		//    }
		//}


		//public virtual void GetRangeRings(ref List<Tuple<int, Color, String>> rangeRingDefinition)
		//{
		//    rangeRingDefinition.Add(Tuple.Create(PowerGrid.PowerConductingDistance, new Color(200, 200, 200), "Power Range"));
		//}


		/// <summary>
		/// True if this node is active and ready to either conduct or produce power (not used for consumers)
		/// </summary>
		//public bool PowerStateActive
		//{
		//    get
		//    {
		//        return !isConstructing;
		//    }
		//}

		/// <summary>
		/// True if this conducts power
		/// </summary>
		//public bool ConductsPower { get; protected set; }


		/// <summary>
		/// True if this produces power  (note that power producers should also conduct power)
		/// </summary>
		//public bool ProducesPower { get; protected set; }
		
		
		/// <summary>
		/// Returns an offset from the center, showing where the power link should be displayed
		/// </summary>
		/// <returns>Returns an offset from the center, showing where the power link should be displayed</returns>
		//public Vector2 PowerLinkPointRelative { get; protected set; }


		/// <summary>
		/// Returns the absolute location showing where the power link should be displayed
		/// </summary>
		/// <returns>Returns the absolute location showing where the power link should be displayed</returns>
		//public Vector2 PowerLinkPointAbsolute
		//{
		//    get
		//    {
		//        return Position.Center + PowerLinkPointRelative;
		//    }
		//}


		//public void StartConstruction()
		//{
		//    // Setting these are redundant, but whatever
		//    IsConstructing = true;
		//    //IsUpgrading = false;

		//    // Hook me into the grid
		//    world.PowerGrid[owningForce.ID].ConnectToPowerGrid(this);
		//}
	}
}