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
		private float mineralsLeftToConstruct;


		// Events
		//[EventReplication(EventReplication.ServerToClients)]
		//public event Action<EntityConstructionProgressEventArgs> ConstructionProgressChangedEvent;

		//[EventReplication(EventReplication.ClientToServer)]
		//public event Action<EntityRequestConstructionCancelEventArgs> RequestConstructionCancelEvent;


		//// Local event only
		//public static event Action<EntityEventArgs> AnyConstructionCompletedEvent;
		//public event Action<EntityEventArgs> ConstructionCompletedEvent;

		public Constructable(World world, int entityID, int mineralsToConstruct)
			: base(world, entityID)
		{
			IsConstructing = false;
			IsBeingPlaced = true;
			this.MineralsToConstruct = mineralsToConstruct;
			this.mineralsLeftToConstruct = mineralsToConstruct;
		}


		//protected Constructable(BinaryReader br)
		//    : base(br)
		//{
		//    level = br.ReadInt32();

		//    mineralsLeftToConstruct = br.ReadSingle();
		//    isConstructing = br.ReadBoolean();
		//}


		///// <summary>
		///// Serialize this Component
		///// </summary>
		///// <param name="bw">The BinaryWriter to stream to</param>
		//public override void Serialize(BinaryWriter bw)
		//{
		//    // Always serialize the base first because we can't pick the deserialization order
		//    base.Serialize(bw);

		//    bw.Write(level);

		//    bw.Write(mineralsLeftToConstruct);
		//    bw.Write(isConstructing);
		//}


		//public override void PostDeserializeLink(World world)
		//{
		//    base.PostDeserializeLink(world);

		//    // Hook me into the grid
		//    world.PowerGrid[owningForce.ID].ConnectToPowerGrid(this);
		//}


		/// <summary>
		/// How many minerals does this constructable take to build?
		/// </summary>
		public int MineralsToConstruct { get; set; }


		/// <summary>
		/// How many minerals does this constructable take to build?
		/// </summary>
		public float MineralsLeftToConstruct
		{
			get
			{
				return mineralsLeftToConstruct;
			}
			set
			{
				//int delta = (int)Math.Ceiling(mineralsLeftToConstruct) - (int)Math.Max(Math.Ceiling(value), 0);
				mineralsLeftToConstruct = Math.Max(value, 0);

				// Tell all my friends
				//if (ConstructionProgressChangedEvent != null)
				//{
				//    ConstructionProgressChangedEvent(new EntityConstructionProgressEventArgs(this, mineralsLeftToConstruct, delta));
				//}


				if (mineralsLeftToConstruct <= 0)
				{
					//mineralsToUpgrade = 0;
					mineralsLeftToConstruct = 0;

					// This construction is complete
					IsConstructing = false;

					//if (AnyConstructionCompletedEvent != null)
					//{
					//    AnyConstructionCompletedEvent(new EntityEventArgs(this));
					//}
					//if (ConstructionCompletedEvent != null)
					//{
					//    ConstructionCompletedEvent(new EntityEventArgs(this));
					//}
				}
			}
		}


		/// <summary>
		/// Is this being placed?
		/// </summary>
		public virtual bool IsBeingPlaced { get; set; }


		/// <summary>
		/// Is this under construction?
		/// </summary>
		public virtual bool IsConstructing { get; set; }


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



	}
}
