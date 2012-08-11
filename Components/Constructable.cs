using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using AsteroidOutpost.Entities.Eventing;
using AsteroidOutpost.Networking;
using AsteroidOutpost.Screens;

namespace AsteroidOutpost.Components
{
	internal class Constructable : Component
	{
		// TODO: 2012-08-10 Do I still want to have entity levels?
		protected int level = 1;

		private float mineralsLeftToConstruct;
		protected bool isConstructing = true;


		// Events
		//[EventReplication(EventReplication.ServerToClients)]
		//public event Action<EntityConstructionProgressEventArgs> ConstructionProgressChangedEvent;

		//[EventReplication(EventReplication.ClientToServer)]
		//public event Action<EntityRequestConstructionCancelEventArgs> RequestConstructionCancelEvent;


		//// Local event only
		//public static event Action<EntityEventArgs> AnyConstructionCompletedEvent;
		//public event Action<EntityEventArgs> ConstructionCompletedEvent;

		public Constructable(World world)
			: base(world)
		{
		}


		protected Constructable(BinaryReader br)
			: base(br)
		{
			level = br.ReadInt32();

			mineralsLeftToConstruct = br.ReadSingle();
			isConstructing = br.ReadBoolean();
		}


		/// <summary>
		/// Serialize this Component
		/// </summary>
		/// <param name="bw">The BinaryWriter to stream to</param>
		public override void Serialize(BinaryWriter bw)
		{
			// Always serialize the base first because we can't pick the deserialization order
			base.Serialize(bw);

			bw.Write(level);

			bw.Write(mineralsLeftToConstruct);
			bw.Write(isConstructing);
		}


		//public override void PostDeserializeLink(World world)
		//{
		//    base.PostDeserializeLink(world);

		//    // Hook me into the grid
		//    world.PowerGrid[owningForce.ID].ConnectToPowerGrid(this);
		//}


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
				//mineralsToUpgrade = 0;
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


		public void CancelConstruction()
		{
			CancelConstruction(world.IsServer);
		}

		public void CancelConstruction(bool authoritative)
		{
			if (!isConstructing)
			{
				// Note: This may be alright in a laggy network, but for now:
				Console.WriteLine("You can't cancel constructing if you aren't constructing");
				Debugger.Break();
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
				if(world.PowerGrid[owningForce.ID].HasPower(this, powerToUse))
				{
					// Check to see if the mineralsLeftToConstruct would pass an integer boundary
					delta = (int)Math.Ceiling(mineralsLeftToConstruct) - (int)Math.Ceiling(mineralsLeftToConstruct - mineralsToUse);
					if (delta != 0)
					{
						// If the force doesn't have enough minerals, we will halt the construction here until it does 
						if (owningForce.GetMinerals() >= delta)
						{
							// Consume the resources
							world.PowerGrid[owningForce.ID].GetPower(this, powerToUse);
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
						world.PowerGrid[owningForce.ID].GetPower(this, powerToUse);
					}
				}
			}
			return true;
		}



	}
}
