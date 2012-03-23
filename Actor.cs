﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using AsteroidOutpost.Networking;
using AsteroidOutpost.Screens;

namespace AsteroidOutpost
{
	public class Actor : ISerializable
	{

		protected AsteroidOutpostScreen theGame;

		private int id;
		private readonly ActorRole role;
		private List<Force> forces = new List<Force>(1);

		// Deserialized, Pre-linked IDs
		private List<int> forceIDs = new List<int>(1);


		public Actor(AsteroidOutpostScreen theGame, int theID, ActorRole theRole, Force primaryForce)
		{
			this.theGame = theGame;
			id = theID;
			role = theRole;
			forces.Add(primaryForce);
		}

		public Actor(BinaryReader br)
		{
			id = br.ReadInt32();
			role = (ActorRole)br.ReadInt32();

			int forceCount = br.ReadInt32();
			for (int i = 0; i < forceCount; i++)
			{
				forceIDs.Add(br.ReadInt32());
			}
		}


		/// <summary>
		/// Serializes this object
		/// </summary>
		/// <param name="bw">The binary writer to serialize to</param>
		public void Serialize(BinaryWriter bw)
		{
			bw.Write(id);
			bw.Write((int)role);
			bw.Write(forces.Count);
			foreach(Force force in forces)
			{
				bw.Write(force.ID);
			}
		}


		/// <summary>
		/// After deserializing, this should be called to link this object to other objects
		/// </summary>
		/// <param name="theGame"></param>
		public void PostDeserializeLink(AsteroidOutpostScreen theGame)
		{
			this.theGame = theGame;

			foreach (int forceID in forceIDs)
			{
				forces.Add(theGame.GetForce(forceID));
			}
			forceIDs.Clear();
		}


		public ActorRole Role
		{
			get { return role; }
		}

		public Force PrimaryForce
		{
			get { return forces[0]; }
		}

		public List<Force> Forces
		{
			get { return forces; }
			set
			{
				if(value.Count == 0)
				{
					Console.WriteLine("Actors must control at least 1 Force at all times");
					Debugger.Break();
				}
				forces = value;
			}
		}

		public virtual void Update(TimeSpan deltaTime)
		{
		}


		/// <summary>
		/// Gets the Actor's ID
		/// </summary>
		public int ID
		{
			get { return id; }
		}
	}


	public enum ActorRole
	{
		Idle,
		Local,
		Remote,
		AI			// Note: Do I need a local AI and a remote AI?
	}
}
