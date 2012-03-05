using System;
using System.Collections.Generic;

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
		public MultiEntityEventArgs(List<Entity> entity)
		{
			Entities = entity;
		}

		public List<Entity> Entities { get; private set; }
	}


	public class EntityUpgradeEventArgs : EntityEventArgs
	{

		public EntityUpgradeEventArgs(Component component, Upgrade theUpgrade)
			: base(component)
		{
			Upgrade = theUpgrade;
		}

		public Upgrade Upgrade { get; set; }
	}


	public class EntityEventArgs : EventArgs
	{
		public EntityEventArgs(Component component)
		{
			Component = component;
		}


		public Component Component { get; private set; }
	}


	public class EntityDyingEventArgs : EntityReflectiveEventArgs
	{
		//public EntityDyingEventArgs(Entity theEntity) : base(theEntity, "SetHitPoints", new object[] { 0.0f, true })
		public EntityDyingEventArgs(Component component)
			: base(component, "SetDead", new object[] { true, true })
		{
		}
	}
}