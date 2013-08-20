using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using AsteroidOutpost.Entities.Eventing;
using AsteroidOutpost.Networking;
using AsteroidOutpost.Screens;
using Microsoft.Xna.Framework;

namespace AsteroidOutpost.Components
{
	internal class Constructible : Component
	{
		private float mineralsConstructed;

		public Constructible(int entityID) : base(entityID)
		{
			IsBeingPlaced = true;
			mineralsConstructed = 0;
		}
		public Constructible(int entityID, int mineralsToConstruct)
			: base(entityID)
		{
			IsConstructing = false;
			IsBeingPlaced = true;
			MineralsToConstruct = mineralsToConstruct;
			mineralsConstructed = 0;
		}


		
		public bool IsBeingPlaced { get; set; }
		public bool IsConstructing { get; set; }
		public int MineralsToConstruct { get; set; }
		public float MineralsConstructed
		{
			get
			{
				return mineralsConstructed;
			}
			set
			{
				mineralsConstructed = MathHelper.Clamp(value, 0, MineralsToConstruct);
			}
		}
	}
}
