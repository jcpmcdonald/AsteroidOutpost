using System;
using System.IO;
using AsteroidOutpost.Entities.Eventing;
using AsteroidOutpost.Eventing;
using AsteroidOutpost.Interfaces;
using AsteroidOutpost.Networking;
using AsteroidOutpost.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AsteroidOutpost.Components
{
	public class Component : ICanKillSelf, IIdentifiable
	{
		protected World world;

		// This ID will uniquely identify this object in the game
		private bool deleteMe;


		// Events
		[EventReplication(EventReplication.ServerToClients)]
		public event Action<ComponentDyingEventArgs> DyingEvent;


		public Component(World world, int entityID)
		{
			GUID = Guid.NewGuid();
			this.EntityID = entityID;
			this.world = world;
		}

		//protected Component(BinaryReader br)
		//{
		//    entityID = br.ReadInt32();
		//}


		/// <summary>
		/// Serializes this object
		/// </summary>
		/// <param name="bw">The binary writer to serialize to</param>
		//public virtual void Serialize(BinaryWriter bw)
		//{
		//    bw.Write(entityID);
		//}


		/// <summary>
		/// After deserializing, this should be called to link this object to other objects
		/// </summary>
		/// <param name="world"></param>
		//public virtual void PostDeserializeLink(World world)
		//{
		//    this.world = world;
		//}


		/// <summary>
		/// Gets the Entity's ID
		/// </summary>
		public int EntityID { get; set; }

		public Guid GUID { get; set; }


		public void KillSelf(EventArgs e)
		{
			// This should only be a local event, so I am authoritative
			SetDead(true, true);
		}


		/// <summary>
		/// Gets whether this Entity should be deleted after this update cycle
		/// </summary>
		public bool IsDead()
		{
			return deleteMe;
		}

		/// <summary>
		/// Sets whether this Entity should be deleted after this update cycle
		/// </summary>
		public void SetDead(bool delMe)
		{
			SetDead(delMe, world.IsServer);
		}


		public void SetDead(bool delMe, bool authoritative)
		{
			if (!authoritative)
			{
				return;
			}

			deleteMe = delMe;

			// Tell everyone that's interested in my death
			if (DyingEvent != null)
			{
				DyingEvent(new ComponentDyingEventArgs(this));
			}
		}
	}
}
