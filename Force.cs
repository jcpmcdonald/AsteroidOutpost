using System;
using System.IO;
using AsteroidOutpost.Entities;
using AsteroidOutpost.Entities.Eventing;
using AsteroidOutpost.Networking;
using AsteroidOutpost.Screens;

namespace AsteroidOutpost
{
	// May the
	public class Force : ISerializable
	{
		// be with you

		protected World world;

		private int id = -1;
		private int minerals;
		private Team team;

		[EventReplication(EventReplication.ServerToClients)]
		public event Action<ForceMineralsChangedEventArgs> MineralsChangedEvent;


		public Force(World world, int theID, int initialMinerals, Team theTeam)
		{
			this.world = world;
			id = theID;
			minerals = Math.Max(initialMinerals, 0);
			team = theTeam;
		}


		public Force(BinaryReader br)
		{
			id = br.ReadInt32();
			minerals = br.ReadInt32();
			team = (Team)br.ReadInt32();
		}

		public void Serialize(BinaryWriter bw)
		{
			bw.Write(id);
			bw.Write(minerals);
			bw.Write((int)team);
		}


		/// <summary>
		/// After deserializing, this should be called to link this object to other objects
		/// </summary>
		/// <param name="world"></param>
		public void PostDeserializeLink(World world)
		{
			this.world = world;
		}


		public void SetMinerals(int value)
		{
			SetMinerals(value, world.IsServer);
		}

		public void SetMinerals(int value, bool authoritative)
		{
			//if(!authoritative)
			//{
			//	return;
			//}


			int delta = value - minerals;
			minerals = value;
			if (minerals < 0) { minerals = 0; }

			if (MineralsChangedEvent != null && delta != 0)
			{
				MineralsChangedEvent(new ForceMineralsChangedEventArgs(this, minerals, delta));
			}
		}

		public int GetMinerals()
		{
			return minerals;
		}

		
		public Team Team
		{
			get { return team; }
		}


		/// <summary>
		/// Gets the Force's ID
		/// </summary>
		public int ID
		{
			get { return id; }
		}
	}

	
	public enum Team
	{
		Team1 = 0,
		Team2,
		Team3,
		Team4,
		Team5,
		Team6,
		Team7,
		Team8,
		Team9,
		Team10,
		AI,
		Neutral
	}
	
}
