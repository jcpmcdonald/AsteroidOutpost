using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using AsteroidOutpost.Entities.Eventing;
using AsteroidOutpost.Networking;
using AsteroidOutpost.Screens;
using C3.XNA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AsteroidOutpost.Entities
{
	public class Component : ISerializable, ICanKillSelf, IReflectionTarget
	{
		protected AsteroidOutpostScreen theGame;

		// This ID will uniquely identify this object in the game
		protected int id = -1;
		private bool deleteMe;

		protected Force owningForce;
		private int postDeserializeOwningForceID;		// For serialization linking, don't use this


		// Events
		[EventReplication(EventReplication.ServerToClients)]
		public event Action<EntityDyingEventArgs> DyingEvent;


		public Component(AsteroidOutpostScreen theGame, IComponentList componentList, Force owningForce)
		{
			this.theGame = theGame;
			this.owningForce = owningForce;
		}

		protected Component(BinaryReader br)
		{
			id = br.ReadInt32();
			postDeserializeOwningForceID = br.ReadInt32();
		}


		/// <summary>
		/// Serializes this object
		/// </summary>
		/// <param name="bw">The binary writer to serialize to</param>
		public virtual void Serialize(BinaryWriter bw)
		{
			bw.Write(id);

			if (owningForce != null)
			{
				bw.Write(owningForce.ID);
			}
			else
			{
				Debugger.Break();
				bw.Write(-1);
			}
		}


		/// <summary>
		/// After deserializing, this should be called to link this object to other objects
		/// </summary>
		/// <param name="theGame"></param>
		public virtual void PostDeserializeLink(AsteroidOutpostScreen theGame)
		{
			this.theGame = theGame;

			if (postDeserializeOwningForceID >= 0)
			{
				// Find the force with this ID
				owningForce = theGame.GetForce(postDeserializeOwningForceID);
			}
			else
			{
				owningForce = null;
			}
		}


		/// <summary>
		/// Gets the Entity's ID
		/// </summary>
		public int ID
		{
			get { return id; }
			set
			{
				id = value;
			}
		}


		public void KillSelf(EventArgs e)
		{
			// This should only be a local entity, so I am authoritative
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
			SetDead(delMe, theGame.IsServer);
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
				DyingEvent(new EntityDyingEventArgs(this));
			}
		}


		/// <summary>
		/// Draws this component
		/// </summary>
		/// <param name="spriteBatch">The sprite batch to draw to</param>
		/// <param name="scaleModifier">The scale modifier to use</param>
		/// <param name="tint">The tint to use</param>
		public virtual void Draw(SpriteBatch spriteBatch, float scaleModifier, Color tint)
		{
		}

		/// <summary>
		/// Updates this component
		/// </summary>
		/// <param name="deltaTime">The amount of time that has passed since the last frame</param>
		public virtual void Update(TimeSpan deltaTime)
		{
		}
	}
}
