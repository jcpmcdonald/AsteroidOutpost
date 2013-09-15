using System;
using System.Collections.Generic;
using AsteroidOutpost.Components;
using AsteroidOutpost.Interfaces;

namespace AsteroidOutpost.Entities.Eventing
{
	public class AccumulationEventArgs : EventArgs, IQuantifiable
	{
		public AccumulationEventArgs(int amount)
		{
			Quantity = amount;
		}

		public int Quantity { get; private set; }
		public int Delta { get { return Quantity; } }
	}


	public class MultiEntityEventArgs : EventArgs
	{
		public MultiEntityEventArgs(List<int> entityIDs)
		{
			EntityIDs = entityIDs;
		}

		public List<int> EntityIDs { get; private set; }
	}


	//public class EntityUpgradeEventArgs : EntityEventArgs
	//{

	//    public EntityUpgradeEventArgs(int entityID, Upgrade theUpgrade)
	//        : base(entityID)
	//    {
	//        Upgrade = theUpgrade;
	//    }

	//    public Upgrade Upgrade { get; set; }
	//}


	public class EntityEventArgs : EventArgs
	{
		public EntityEventArgs(int entityID)
		{
			EntityID = entityID;
		}


		public int EntityID { get; private set; }
	}


	public class ComponentEventArgs : EventArgs
	{
		public ComponentEventArgs(Component component)
		{
			Component = component;
		}


		public Component Component { get; private set; }
	}
}