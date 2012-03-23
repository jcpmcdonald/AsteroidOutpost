using System;
using System.IO;
using AsteroidOutpost.Entities.Eventing;
using AsteroidOutpost.Interfaces;
using AsteroidOutpost.Networking;
using AsteroidOutpost.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AsteroidOutpost.Components
{
	public class Component : ISerializable, ICanKillSelf, IIdentifiable
	{
		protected AsteroidOutpostScreen theGame;

		// This ID will uniquely identify this object in the game
		protected int id = -1;
		private bool deleteMe;


		// Events
		[EventReplication(EventReplication.ServerToClients)]
		public event Action<ComponentDyingEventArgs> DyingEvent;


		public Component(AsteroidOutpostScreen theGame, IComponentList componentList)
		{
			this.theGame = theGame;
		}

		protected Component(BinaryReader br)
		{
			id = br.ReadInt32();
		}


		/// <summary>
		/// Serializes this object
		/// </summary>
		/// <param name="bw">The binary writer to serialize to</param>
		public virtual void Serialize(BinaryWriter bw)
		{
			bw.Write(id);
		}


		/// <summary>
		/// After deserializing, this should be called to link this object to other objects
		/// </summary>
		/// <param name="theGame"></param>
		public virtual void PostDeserializeLink(AsteroidOutpostScreen theGame)
		{
			this.theGame = theGame;
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
				DyingEvent(new ComponentDyingEventArgs(this));
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
