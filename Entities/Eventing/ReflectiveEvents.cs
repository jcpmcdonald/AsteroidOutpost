


using System;
using AsteroidOutpost.Entities.Structures;
using AsteroidOutpost.Networking;
using Microsoft.Xna.Framework;

namespace AsteroidOutpost.Entities.Eventing
{
	public class EntityReflectiveEventArgs : ReflectiveEventArgs
	{
		public EntityReflectiveEventArgs(Component component, String theRemoteMethodName, object[] theRemoteMethodParameters)
			: base(component, theRemoteMethodName, theRemoteMethodParameters)
		{
			Component = component;
		}

		public Component Component { get; private set; }
	}


	public class EntityMovedEventArgs : EntityReflectiveEventArgs
	{
		public EntityMovedEventArgs(Component component, Vector2 newPosition, Vector2 delta)
			: base(component, "SetCenter", new object[] { newPosition })
		{
			NewPosition = newPosition;
			Delta = delta;
		}

		public Vector2 NewPosition { get; private set; }
		public Vector2 Delta { get; private set; }
	}


	public class EntityHitPointsChangedEventArgs : EntityReflectiveEventArgs, IQuantifiable
	{
		public EntityHitPointsChangedEventArgs(Component component, float theNewHitPoints, int delta)
			: base(component, "Set", new object[] { theNewHitPoints })
		{
			NewHitPoints = theNewHitPoints;
			Delta = delta;
		}

		public float NewHitPoints { get; private set; }
		public int Quantity { get { return (int)NewHitPoints; } }
		public int Delta { get; private set; }
	}


	public class EntityPowerLevelChangedEventArgs : EntityReflectiveEventArgs, IQuantifiable
	{
		public EntityPowerLevelChangedEventArgs(Component component, float thePowerLevel, int delta)
			: base(component, "SetCurrentPower", new object[] { thePowerLevel })
		{
			PowerLevel = thePowerLevel;
			Delta = delta;
		}

		public double PowerLevel { get; private set; }
		public int Quantity { get { return (int)PowerLevel; } }
		public int Delta { get; private set; }
	}


	public class EntityMineralValueChangedEventArgs : EntityReflectiveEventArgs
	{
		public EntityMineralValueChangedEventArgs(Component component, int mineralValue)
			: base(component, "SetMinerals", new object[] { mineralValue, true })
		{
			MineralValue = mineralValue;
		}

		public int MineralValue { get; private set; }
	}


	public class EntityTargetChangedEventArgs : EntityReflectiveEventArgs
	{
		public EntityTargetChangedEventArgs(Component component, Entity newTarget)
			: base(component, "SetTarget", new object[] { newTarget == null ? -1 : newTarget.ID, true })
		{
			NewTarget = newTarget;
		}

		public Entity NewTarget { get; private set; }
	}


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


	#region Construction Events

	public class EntityConstructionProgressEventArgs : EntityReflectiveEventArgs, IQuantifiable
	{
		public EntityConstructionProgressEventArgs(ConstructableEntity constructable, float newMineralsLeftToConstruct, int delta)
			: base(constructable, "SetMineralsLeftToConstruct", new object[] { newMineralsLeftToConstruct })
		{
			MineralsLeftToConstruct = newMineralsLeftToConstruct;
			Quantity = (int)(constructable.MineralsToConstruct - MineralsLeftToConstruct);
			Delta = delta;
		}

		protected double MineralsLeftToConstruct { get; private set; }
		public int Quantity { get; private set; }
		public int Delta { get; private set; }
	}


	public class EntityRequestConstructionCancelEventArgs : EntityReflectiveEventArgs
	{
		public EntityRequestConstructionCancelEventArgs(Component component)
			: base(component, "CancelConstruction", new object[] { })
		{
		}
	}

	#endregion


	#region Upgrade Events

	public class EntityUpgradeStartedEventArgs : EntityReflectiveEventArgs
	{
		public EntityUpgradeStartedEventArgs(Component component, Upgrade theUpgrade)
			: base(component, "StartUpgrade", new object[] { theUpgrade.Name, true })
		{
			Upgrade = theUpgrade;
		}

		public Upgrade Upgrade { get; private set; }
	}


	public class EntityUpgradeProgressEventArgs : EntityReflectiveEventArgs
	{
		public EntityUpgradeProgressEventArgs(Component component, float newMineralsLeftToUpgrade)
			: base(component, "SetMineralsLeftToUpgrade", new object[] { newMineralsLeftToUpgrade })
		{
			MineralsLeftToUpgrade = newMineralsLeftToUpgrade;
		}

		protected double MineralsLeftToUpgrade { get; private set; }
	}


	public class EntityUpgradeCancelledEventArgs : EntityReflectiveEventArgs
	{
		public EntityUpgradeCancelledEventArgs(Component component)
			: base(component, "CancelUpgrade", new object[] { true })
		{
		}
	}


	public class EntityRequestUpgradeEventArgs : EntityReflectiveEventArgs
	{
		public EntityRequestUpgradeEventArgs(Component component, Upgrade theUpgrade)
			: base(component, "StartUpgrade", new object[] { theUpgrade.Name })
		{
			Upgrade = theUpgrade;
		}

		public Upgrade Upgrade { get; private set; }
	}


	public class EntityRequestUpgradeCancelEventArgs : EntityReflectiveEventArgs
	{
		public EntityRequestUpgradeCancelEventArgs(Component component)
			: base(component, "CancelUpgrade", new object[] { })
		{
		}
	}

	#endregion

}
