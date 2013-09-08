using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using AsteroidOutpost.Entities.Eventing;
using AsteroidOutpost.Eventing;
using AsteroidOutpost.Networking;
using AsteroidOutpost.Screens;
using Microsoft.Xna.Framework;

namespace AsteroidOutpost.Components
{
	internal class Constructible : Component
	{
		private float mineralsConstructed;

		public event Action<ConstructionCompleteEventArgs> ConstructionComplete;

		public Constructible(int entityID) : base(entityID)
		{
			IsBeingPlaced = true;
			mineralsConstructed = 0;
		}


		
		public bool IsBeingPlaced { get; set; }
		public bool IsConstructing { get; set; }
		public int Cost { get; set; }
		public float MineralsConstructed
		{
			get
			{
				return mineralsConstructed;
			}
			set
			{
				mineralsConstructed = MathHelper.Clamp(value, 0, Cost);
			}
		}


		public void OnConstructionComplete(ConstructionCompleteEventArgs e)
		{
			if(ConstructionComplete != null)
			{
				ConstructionComplete(e);
			}
		}
	}
}
