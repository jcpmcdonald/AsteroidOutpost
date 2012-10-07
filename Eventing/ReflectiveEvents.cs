using System;
using AsteroidOutpost.Components;
using AsteroidOutpost.Interfaces;
using AsteroidOutpost.Networking;
using Microsoft.Xna.Framework;

namespace AsteroidOutpost.Eventing
{
	//public class EntityReflectiveEventArgs : ReflectiveEventArgs
	//{
	//    public EntityReflectiveEventArgs(Entity entity, String theRemoteMethodName, object[] theRemoteMethodParameters)
	//        : base(entity, theRemoteMethodName, theRemoteMethodParameters)
	//    {
	//        Entity = entity;
	//    }

	//    public Entity Entity { get; private set; }
	//}

	public class ComponentReflectiveEventArgs : ReflectiveEventArgs
	{
		public ComponentReflectiveEventArgs(Component component, String theRemoteMethodName, object[] theRemoteMethodParameters)
			: base(component, theRemoteMethodName, theRemoteMethodParameters)
		{
			Component = component;
		}

		public Component Component { get; private set; }
	}


	public class EntityMovedEventArgs : ComponentReflectiveEventArgs
	{
		public EntityMovedEventArgs(Position position, Vector2 delta)
			: base(position, "SetCenter", new object[] { position.Center })
		{
			NewPosition = position.Center;
			Delta = delta;
		}

		public Vector2 NewPosition { get; private set; }
		public Vector2 Delta { get; private set; }
	}


	public class EntityArmourChangedEventArgs : ComponentReflectiveEventArgs, IQuantifiable
	{
		public EntityArmourChangedEventArgs(HitPoints hitPoints, int delta)
			: base(hitPoints, "SetArmour", new object[] { hitPoints.Armour })
		{
			NewArmour = hitPoints.Armour;
			Delta = delta;
		}

		public float NewArmour { get; private set; }
		public int Quantity { get { return (int)Delta; } }
		public int Delta { get; private set; }
	}


	//public class EntityPowerLevelChangedEventArgs : EntityReflectiveEventArgs, IQuantifiable
	//{
	//    public EntityPowerLevelChangedEventArgs(Entity entity, float thePowerLevel, int delta)
	//        : base(entity, "SetCurrentPower", new object[] { thePowerLevel })
	//    {
	//        PowerLevel = thePowerLevel;
	//        Delta = delta;
	//    }

	//    public double PowerLevel { get; private set; }
	//    public int Quantity { get { return (int)PowerLevel; } }
	//    public int Delta { get; private set; }
	//}


	//public class EntityMineralValueChangedEventArgs : EntityReflectiveEventArgs
	//{
	//    public EntityMineralValueChangedEventArgs(Entity entity, int mineralValue)
	//        : base(entity, "SetMinerals", new object[] { mineralValue, true })
	//    {
	//        MineralValue = mineralValue;
	//    }

	//    public int MineralValue { get; private set; }
	//}


	//public class EntityTargetChangedEventArgs : EntityReflectiveEventArgs
	//{
	//    public EntityTargetChangedEventArgs(Entity entity, Entity newTarget)
	//        : base(entity, "SetTarget", new object[] { newTarget == null ? -1 : newTarget.EntityID, true })
	//    {
	//        NewTarget = newTarget;
	//    }

	//    public Entity NewTarget { get; private set; }
	//}


	public class ForceMineralsChangedEventArgs : ReflectiveEventArgs, IQuantifiable
	{
		public ForceMineralsChangedEventArgs(Force theForce, int minerals, int delta)
			: base(AONetwork.SpecialTargetTheGame, "SetForceMinerals", new object[] { theForce.ID, minerals, true })
		{
			Quantity = minerals;
			Delta = delta;
		}

		public int Quantity { get; private set; }
		public int Delta { get; private set; }
	}


	public class EntityDyingEventArgs : ComponentReflectiveEventArgs
	{
		public EntityDyingEventArgs(HitPoints hitPoints)
			: base(hitPoints, "SetDead", new object[] { true, true })
		{
		}
	}

	//public class EntityDyingEventArgs : EntityReflectiveEventArgs
	//{
	//    //public EntityDyingEventArgs(Entity theEntity) : base(theEntity, "SetHitPoints", new object[] { 0.0f, true })
	//    public EntityDyingEventArgs(Entity entity)
	//        : base(entity, "SetDead", new object[] { true, true })
	//    {
	//    }
	//}


	#region Construction Events

	//public class EntityConstructionProgressEventArgs : EntityReflectiveEventArgs, IQuantifiable
	//{
	//    public EntityConstructionProgressEventArgs(ConstructableEntity constructable, float newMineralsLeftToConstruct, int delta)
	//        : base(constructable, "SetMineralsLeftToConstruct", new object[] { newMineralsLeftToConstruct })
	//    {
	//        MineralsLeftToConstruct = newMineralsLeftToConstruct;
	//        Quantity = (int)(constructable.MineralsToConstruct - MineralsLeftToConstruct);
	//        Delta = delta;
	//    }

	//    protected double MineralsLeftToConstruct { get; private set; }
	//    public int Quantity { get; private set; }
	//    public int Delta { get; private set; }
	//}


	//public class EntityRequestConstructionCancelEventArgs : EntityReflectiveEventArgs
	//{
	//    public EntityRequestConstructionCancelEventArgs(ConstructableEntity constructable)
	//        : base(constructable, "CancelConstruction", new object[] { })
	//    {
	//    }
	//}

	#endregion


	#region Upgrade Events

	//public class EntityUpgradeStartedEventArgs : EntityReflectiveEventArgs
	//{
	//    public EntityUpgradeStartedEventArgs(ConstructableEntity constructable, Upgrade theUpgrade)
	//        : base(constructable, "StartUpgrade", new object[] { theUpgrade.Name, true })
	//    {
	//        Upgrade = theUpgrade;
	//    }

	//    public Upgrade Upgrade { get; private set; }
	//}


	//public class EntityUpgradeProgressEventArgs : EntityReflectiveEventArgs
	//{
	//    public EntityUpgradeProgressEventArgs(ConstructableEntity constructable, float newMineralsLeftToUpgrade)
	//        : base(constructable, "SetMineralsLeftToUpgrade", new object[] { newMineralsLeftToUpgrade })
	//    {
	//        MineralsLeftToUpgrade = newMineralsLeftToUpgrade;
	//    }

	//    protected double MineralsLeftToUpgrade { get; private set; }
	//}


	//public class EntityUpgradeCancelledEventArgs : EntityReflectiveEventArgs
	//{
	//    public EntityUpgradeCancelledEventArgs(ConstructableEntity constructable)
	//        : base(constructable, "CancelUpgrade", new object[] { true })
	//    {
	//    }
	//}


	//public class EntityRequestUpgradeEventArgs : EntityReflectiveEventArgs
	//{
	//    public EntityRequestUpgradeEventArgs(ConstructableEntity constructable, Upgrade theUpgrade)
	//        : base(constructable, "StartUpgrade", new object[] { theUpgrade.Name })
	//    {
	//        Upgrade = theUpgrade;
	//    }

	//    public Upgrade Upgrade { get; private set; }
	//}


	//public class EntityRequestUpgradeCancelEventArgs : EntityReflectiveEventArgs
	//{
	//    public EntityRequestUpgradeCancelEventArgs(ConstructableEntity constructable)
	//        : base(constructable, "CancelUpgrade", new object[] { })
	//    {
	//    }
	//}

	#endregion

}
