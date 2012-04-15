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
	public abstract class ConstructableEntity : Entity, IPowerGridNode
	{
		private const float powerUsageRate = 12.0f;
		private const float mineralUsageRate = 30.0f;
		
		protected int level = 1;
		
		
		// TODO: Think of a better name for "nearbyPower" because it can also contain consumers
		/// <summary>
		/// Store a list of all nearby power conductors, and if we're a power conductor, also store consumers
		/// </summary>
		//protected List<PowerConductor> nearbyPower;
		
		private float mineralsLeftToConstruct;
		protected bool isConstructing = true;

		private int mineralsToUpgrade;
		private float mineralsLeftToUpgrade;
		protected bool isUpgrading;
		
		protected List<Upgrade> allUpgrades = new List<Upgrade>();
		protected List<Upgrade> constructedUpgrades = new List<Upgrade>();
		protected Upgrade currentUpgrade;


		// Events
		[EventReplication(EventReplication.ServerToClients)]
		public event Action<EntityConstructionProgressEventArgs> ConstructionProgressChangedEvent;

		[EventReplication(EventReplication.ClientToServer)]
		public event Action<EntityRequestUpgradeEventArgs> RequestUpgradeEvent;

		[EventReplication(EventReplication.ServerToClients)]
		public event Action<EntityUpgradeStartedEventArgs> UpgradeStartedEvent;

		[EventReplication(EventReplication.ServerToClients)]
		public event Action<EntityUpgradeProgressEventArgs> UpgradeProgressChangedEvent;

		[EventReplication(EventReplication.ClientToServer)]
		public event Action<EntityRequestUpgradeCancelEventArgs> RequestUpgradeCancelEvent;

		[EventReplication(EventReplication.ServerToClients)]
		public event Action<EntityUpgradeCancelledEventArgs> UpgradeCancelledEvent;

		[EventReplication(EventReplication.ClientToServer)]
		public event Action<EntityRequestConstructionCancelEventArgs> RequestConstructionCancelEvent;


		// Local event only
		public static event Action<EntityEventArgs> AnyConstructionCompletedEvent;
		public static event Action<EntityUpgradeEventArgs> AnyUpgradeCompletedEvent;

		public event Action<EntityEventArgs> ConstructionCompletedEvent;
		public event Action<EntityUpgradeEventArgs> UpgradeCompletedEvent;


		protected ConstructableEntity(AsteroidOutpostScreen theGame, IComponentList componentList, Force theowningForce, Vector2 theCenter, int theRadius, int totalHitPoints)
			: base(theGame, componentList, theowningForce, theCenter, theRadius, totalHitPoints)
		{
			// TODO: Lookup and fix "virtual member call in constructor"
			mineralsLeftToConstruct = MineralsToConstruct;

			InitializeUpgrades();
		}


		protected ConstructableEntity(BinaryReader br)
			: base(br)
		{
			level = br.ReadInt32();

			mineralsLeftToConstruct = br.ReadSingle();
			isConstructing = br.ReadBoolean();

			mineralsToUpgrade = br.ReadInt32();
			mineralsLeftToUpgrade = br.ReadSingle();
			isUpgrading = br.ReadBoolean();

			// Deserialize the upgrades
			InitializeUpgrades();

			int constructedUpgradesCount = br.ReadInt32();
			for (int i = 0; i < constructedUpgradesCount; i++)
			{
				constructedUpgrades.Add(GetUpgradeByName(br.ReadString()));
			}

			if (isUpgrading)
			{
				currentUpgrade = GetUpgradeByName(br.ReadString());
			}
		}


		/// <summary>
		/// Serialize this Entity
		/// </summary>
		/// <param name="bw">The BinaryWriter to stream to</param>
		public override void Serialize(BinaryWriter bw)
		{
			// Always serialize the base first because we can't pick the deserialization order
			base.Serialize(bw);

			bw.Write(level);

			bw.Write(mineralsLeftToConstruct);
			bw.Write(isConstructing);

			bw.Write(mineralsToUpgrade);
			bw.Write(mineralsLeftToUpgrade);
			bw.Write(isUpgrading);

			// Serialize the upgrades
			bw.Write(constructedUpgrades.Count);
			foreach(Upgrade constructedUpgrade in constructedUpgrades)
			{
				bw.Write(constructedUpgrade.Name);
			}

			if (isUpgrading)
			{
				bw.Write(currentUpgrade.Name);
			}
		}


		public override void PostDeserializeLink(AsteroidOutpostScreen theGame)
		{
			base.PostDeserializeLink(theGame);

			// Hook me into the grid
			theGame.PowerGrid(owningForce).ConnectToPowerGrid(this);
		}


		/// <summary>
		/// How many minerals does this constructable take to build?
		/// </summary>
		public abstract int MineralsToConstruct
		{
			get;
		}
		
		
		/// <summary>
		/// How many minerals does this constructable take to build?
		/// </summary>
		public int MineralsLeftToConstruct
		{
			get
			{
				return (int)mineralsLeftToConstruct;
			}
		}

		public void SetMineralsLeftToConstruct(float value)
		{
			int delta = (int)Math.Ceiling(mineralsLeftToConstruct) - (int)Math.Max(Math.Ceiling(value), 0);
			mineralsLeftToConstruct = Math.Max(value, 0);
			
			// Tell all my friends
			if (ConstructionProgressChangedEvent != null)
			{
				ConstructionProgressChangedEvent(new EntityConstructionProgressEventArgs(this, mineralsLeftToConstruct, delta));
			}


			if (mineralsLeftToConstruct <= 0)
			{
				mineralsToUpgrade = 0;
				mineralsLeftToConstruct = 0;

				// This construction is complete
				IsConstructing = false;

				if (AnyConstructionCompletedEvent != null)
				{
					AnyConstructionCompletedEvent(new EntityEventArgs(this));
				}
				if (ConstructionCompletedEvent != null)
				{
					ConstructionCompletedEvent(new EntityEventArgs(this));
				}
			}
		}


		/// <summary>
		/// How many minerals does this constructable take to upgrade?
		/// </summary>
		public int MineralsToUpgrade
		{
			get
			{
				return mineralsToUpgrade;
			}
		}


		/// <summary>
		/// How many minerals are left to upgrade this constructable?
		/// </summary>
		public int MineralsLeftToUpgrade
		{
			get
			{
				return (int)mineralsLeftToUpgrade;
			}
		}

		public void SetMineralsLeftToUpgrade(float value)
		{
			bool changed = false;
			if(mineralsLeftToUpgrade != value)
			{
				changed = true;
			}

			mineralsLeftToUpgrade = value;

			if(changed)
			{
				if (UpgradeProgressChangedEvent != null)
				{
					UpgradeProgressChangedEvent(new EntityUpgradeProgressEventArgs(this, mineralsLeftToUpgrade));
				}


				if (mineralsLeftToUpgrade <= 0.0)
				{
					mineralsLeftToUpgrade = 0.0f;
					isUpgrading = false;

					constructedUpgrades.Add(currentUpgrade);
					currentUpgrade.Complete();

					if (AnyUpgradeCompletedEvent != null)
					{
						AnyUpgradeCompletedEvent(new EntityUpgradeEventArgs(this, currentUpgrade));
					}
					if (UpgradeCompletedEvent != null)
					{
						UpgradeCompletedEvent(new EntityUpgradeEventArgs(this, currentUpgrade));
					}
					currentUpgrade = null;
				}
			}
		}


		// TODO: Find a better way to do this
		/// <summary>
		/// Entities should initialize their possible upgrades here
		/// </summary>
		protected abstract void InitializeUpgrades();
		
		
		public virtual int Level
		{
			get{ return level; }
			set
			{
				level = value;
			}
		}
		
		
		/// <summary>
		/// Is this under construction?
		/// </summary>
		public virtual bool IsConstructing
		{
			get { return isConstructing; }
			set { isConstructing = value; }
		}
		
		
		/// <summary>
		/// Is this upgrading?
		/// </summary>
		public virtual bool IsUpgrading
		{
			get { return isUpgrading; }
			private set { isUpgrading = value; }
		}
		
		
		
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


		private Upgrade GetUpgradeByName(String upgradeName)
		{
			foreach(Upgrade u in allUpgrades)
			{
				if(u.Name == upgradeName)
				{
					return u;
				}
			}
			return null;
		}


		public void StartUpgrade(String upgradeName)
		{
			StartUpgrade(GetUpgradeByName(upgradeName), theGame.IsServer);
		}

		public void StartUpgrade(String upgradeName, bool authoritative)
		{
			StartUpgrade(GetUpgradeByName(upgradeName), authoritative);
		}

		public void StartUpgrade(Upgrade u)
		{
			StartUpgrade(u, theGame.IsServer);
		}

		public void StartUpgrade(Upgrade u, bool authoritative)
		{
			if (u == null)
			{
				throw new ArgumentNullException("u");
			}

			
			currentUpgrade = u;
			mineralsToUpgrade = u.MineralCost;
			mineralsLeftToUpgrade = u.MineralCost;
			isUpgrading = true;

			if(UpgradeStartedEvent != null)
			{
				UpgradeStartedEvent(new EntityUpgradeStartedEventArgs(this, u));
			}

			if (!authoritative)
			{
				// Request an upgrade
				if (RequestUpgradeEvent == null)
				{
					// Nobody is listening to our cries
					Debugger.Break();
				}
				RequestUpgradeEvent(new EntityRequestUpgradeEventArgs(this, u));
			}
		}



		public void CancelUpgrade()
		{
			CancelUpgrade(theGame.IsServer);
		}

		public void CancelUpgrade(bool authoritative)
		{
			if (currentUpgrade == null || !isUpgrading)
			{
				// Note: This may be alright in a laggy network, but for now
				// You can't cancel an upgrade if you aren't upgrading
				Debugger.Break();
			}

			
			// Give the user back half of their spent minerals
			int mineralsToGiveBack = (int)(((mineralsToUpgrade - mineralsLeftToUpgrade) * 0.5) + 0.5);
			owningForce.SetMinerals(owningForce.GetMinerals() + mineralsToGiveBack);

			mineralsToUpgrade = 0;
			mineralsLeftToUpgrade = 0.0f;
			isUpgrading = false;

			if (UpgradeCancelledEvent != null)
			{
				UpgradeCancelledEvent(new EntityUpgradeCancelledEventArgs(this));
			}


			if (!authoritative)
			{
				// Request cancel
				if (RequestUpgradeCancelEvent == null)
				{
					// Nobody is listening to our cries
					Debugger.Break();
				}
				RequestUpgradeCancelEvent(new EntityRequestUpgradeCancelEventArgs(this));
			}
		}


		public void CancelConstruction()
		{
			CancelConstruction(theGame.IsServer);
		}

		public void CancelConstruction(bool authoritative)
		{
			if (!isConstructing)
			{
				// Note: This may be alright in a laggy network, but for now:
				throw new ArgumentException("You can't cancel constructing if you aren't constructing");
			}


			int mineralsToGiveBack = (int)(((MineralsToConstruct - mineralsLeftToConstruct) * 0.5) + 0.5);
			owningForce.SetMinerals(owningForce.GetMinerals() + mineralsToGiveBack);

			if (authoritative)
			{
				SetDead(true);
			}
			else
			{
				// Request a cancel
				if(RequestConstructionCancelEvent == null)
				{
					// Nobody is listening to our cries
					Debugger.Break();
				}
				RequestConstructionCancelEvent(new EntityRequestConstructionCancelEventArgs(this));
			}
		}
		
		
		/// <summary>
		/// Update this constructing building
		/// </summary>
		/// <param name="deltaTime"></param>
		/// <returns></returns>
		protected bool UpdateConstructing(TimeSpan deltaTime)
		{
			if(IsConstructing)
			{
				float powerToUse = powerUsageRate * (float)deltaTime.TotalSeconds;
				float mineralsToUse = mineralUsageRate * (float)deltaTime.TotalSeconds;
				int delta;

				// Check that we have enough power in the grid
				if(theGame.PowerGrid(owningForce).HasPower(this, powerToUse))
				{
					// Check to see if the mineralsLeftToConstruct would pass an integer boundary
					delta = (int)Math.Ceiling(mineralsLeftToConstruct) - (int)Math.Ceiling(mineralsLeftToConstruct - mineralsToUse);
					if (delta != 0)
					{
						// If the force doesn't have enough minerals, we will halt the construction here until it does 
						if (owningForce.GetMinerals() >= delta)
						{
							// Consume the resources
							theGame.PowerGrid(owningForce).GetPower(this, powerToUse);
							SetMineralsLeftToConstruct(mineralsLeftToConstruct - mineralsToUse);

							// Set the force's minerals
							owningForce.SetMinerals(owningForce.GetMinerals() - delta);
						}
						else
						{
							// Construction Halts, no progress, no consumption
						}
					}
					else
					{
						// We have not passed an integer boundary, so just keep track of the change locally
						// We'll get around to subtracting this from the force's minerals when we pass an integer boundary
						mineralsLeftToConstruct -= mineralsToUse;

						// We should consume our little tidbit of power though:
						theGame.PowerGrid(owningForce).GetPower(this, powerToUse);
					}
				}
			}
			return true;
		}
		
		
		
		/// <summary>
		/// Update this upgrading building
		/// </summary>
		/// <param name="deltaTime"></param>
		/// <returns></returns>
		protected virtual bool UpdateUpgrading(TimeSpan deltaTime)
		{
			if(isUpgrading && currentUpgrade != null)
			{
				float powerToUse = powerUsageRate * (float)deltaTime.TotalSeconds;
				float mineralsToUse = mineralUsageRate * (float)deltaTime.TotalSeconds;

				// BUG: There is a disconnect between the check for minerals (below) and the actual consumption of minerals. Could cause weird behaviour
				if (owningForce.GetMinerals() > mineralsToUse && theGame.PowerGrid(owningForce).GetPower(this, powerToUse))
				{
					// Use some minerals toward my upgrade
					int temp = (int)mineralsLeftToConstruct;
					SetMineralsLeftToUpgrade(mineralsLeftToUpgrade - mineralsToUse);

					// If the minerals left to construct has increased by a whole number, subtract it from the force's minerals
					if (temp > (int)mineralsLeftToConstruct)
					{
						owningForce.SetMinerals(owningForce.GetMinerals() - (temp - (int)mineralsLeftToConstruct));
					}
				}
			}
			return true;
		}


		/// <summary>
		/// Draw this entity to the screen
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="scaleModifier"></param>
		/// <param name="tint"></param>
		public override void Draw(SpriteBatch spriteBatch, float scaleModifier, Color tint)
		{
			if(IsConstructing)
			{
				DrawConstructing(spriteBatch, scaleModifier, tint);
			}
			else if(isUpgrading)
			{
				DrawUpgrading(spriteBatch, scaleModifier, tint);
			}
			else
			{
				base.Draw(spriteBatch, scaleModifier, tint);
				
				//DrawPowerConnections(spriteBatch);
			}
		}


		/// <summary>
		/// Draw this constructing entity to the screen
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="scaleModifier"></param>
		/// <param name="tint"></param>
		protected void DrawConstructing(SpriteBatch spriteBatch, float scaleModifier, Color tint)
		{
			float percentComplete = (float)(MineralsToConstruct - mineralsLeftToConstruct) / MineralsToConstruct;
			byte rgb = (byte)((percentComplete * 100.0) + 150.0);
			tint = ColorPalette.ApplyTint(new Color(rgb, rgb, rgb), tint);

			base.Draw(spriteBatch, scaleModifier, tint);
			
			// Draw a progress bar
			//spriteBatch.FillRectangle(theGame.Scale(new Vector2(-Radius.Value, Radius.Value - 6)) + theGame.WorldToScreen(Position.Center), theGame.Scale(new Vector2(Radius.Width, 6)), Color.Gray);
			//spriteBatch.FillRectangle(theGame.Scale(new Vector2(-Radius.Value, Radius.Value - 6)) + theGame.WorldToScreen(Position.Center), theGame.Scale(new Vector2(Radius.Width * percentComplete, 6)), Color.RoyalBlue);
		}


		/// <summary>
		/// Draw this upgrading entity to the screen
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="scaleModifier"></param>
		/// <param name="tint"></param>
		protected void DrawUpgrading(SpriteBatch spriteBatch, float scaleModifier, Color tint)
		{
			float percentComplete = (float)((currentUpgrade.MineralCost - mineralsLeftToUpgrade) / currentUpgrade.MineralCost);

			base.Draw(spriteBatch, scaleModifier, tint);
			
			// Draw a progress bar
			//Vector2 selfCenterOnScreen = new Vector2(Center.X - theGame.Hud.FocusScreen.X, Center.Y - theGame.Hud.FocusScreen.Y);
			spriteBatch.FillRectangle(theGame.Scale(new Vector2(-Radius.Value, Radius.Value - 6)) + theGame.WorldToScreen(Position.Center), theGame.Scale(new Vector2(Radius.Width, 6)), Color.Gray);
			spriteBatch.FillRectangle(theGame.Scale(new Vector2(-Radius.Value, Radius.Value - 6)) + theGame.WorldToScreen(Position.Center), theGame.Scale(new Vector2(Radius.Width * percentComplete, 6)), Color.RoyalBlue);
			
			//DrawPowerConnections(spriteBatch);
		}
		
		
		/// <summary>
		/// Is this a valid place to build?
		/// </summary>
		/// <returns>True if it's legal to build here, false otherwise</returns>
		public virtual bool IsValidToBuildHere()
		{
			bool valid = true;

			// This will grab all objects who's bounding square intersects with us
			List<Entity> nearbyEntities = theGame.EntitiesInArea(Rect);
			foreach (Entity entity in nearbyEntities)
			{
				// Now determine if they are really intersecting
				if(entity.Solid && Radius.IsIntersecting(entity.Radius))
				{
					valid = false;
					break;
				}
			}
			return valid;
		}


		public virtual void GetRangeRings(ref List<Tuple<int, Color, String>> rangeRingDefinition)
		{
			rangeRingDefinition.Add(Tuple.Create(PowerGrid.PowerConductingDistance, new Color(200, 200, 200), "Power Range"));
		}


		/// <summary>
		/// True if this node is active and ready to either conduct or produce power (not used for consumers)
		/// </summary>
		public bool PowerStateActive
		{
			get
			{
				return !isConstructing;
			}
		}

		/// <summary>
		/// True if this conducts power
		/// </summary>
		public bool ConductsPower { get; protected set; }


		/// <summary>
		/// True if this produces power  (note that power producers should also conduct power)
		/// </summary>
		public bool ProducesPower { get; protected set; }
		
		
		/// <summary>
		/// Returns an offset from the center, showing where the power link should be displayed
		/// </summary>
		/// <returns>Returns an offset from the center, showing where the power link should be displayed</returns>
		public Vector2 PowerLinkPointRelative { get; protected set; }


		/// <summary>
		/// Returns the absolute location showing where the power link should be displayed
		/// </summary>
		/// <returns>Returns the absolute location showing where the power link should be displayed</returns>
		public Vector2 PowerLinkPointAbsolute
		{
			get
			{
				return Position.Center + PowerLinkPointRelative;
			}
		}


		public void StartConstruction()
		{
			// Setting these are redundant, but whatever
			IsConstructing = true;
			IsUpgrading = false;

			// Hook me into the grid
			theGame.PowerGrid(owningForce).ConnectToPowerGrid(this);
		}
	}
}