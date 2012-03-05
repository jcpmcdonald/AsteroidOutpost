using System;
using System.Collections.Generic;

namespace AsteroidOutpost.Entities
{
	public class Upgrade
	{
		private readonly String name;
		private readonly String description;
		private readonly int mineralCost;

		readonly List<Upgrade> prerequisites = new List<Upgrade>();
		
		// This class will call this delegate when the upgrade has finished
		public delegate void UpgradeComplete(Upgrade upgrade);
		private readonly UpgradeComplete callbackFunction;
		
		
		public Upgrade(String theName, String theDescription, int theMineralCost, UpgradeComplete callback)
		{
			name = theName;
			description = theDescription;
			mineralCost = theMineralCost;
			callbackFunction = callback;
		}
		
		public Upgrade(String theName, String theDescription, int theMineralCost, UpgradeComplete callback, params Upgrade[] thePrerequisites)
		{
			name = theName;
			description = theDescription;
			mineralCost = theMineralCost;
			callbackFunction = callback;
			
			foreach(Upgrade u in thePrerequisites)
			{
				prerequisites.Add(u);
			}
		}
		
		
		public String Name{ get{ return name; } }
		public String Description{ get{ return description; } }
		public int MineralCost{ get{ return mineralCost; } }

		internal bool CanUpgrade(List<Upgrade> constructedUpgrades)
		{
			bool canUpgrade = true;
			foreach(Upgrade u in prerequisites)
			{
				if(!constructedUpgrades.Contains(u))
				{
					canUpgrade = false;
				}
			}
			return canUpgrade;
		}
		
		
		/// <summary>
		/// Call this function when the upgrade is complete
		/// </summary>
		public void Complete()
		{
			if(callbackFunction != null)
			{
				callbackFunction(this);
			}
		}
	}
}