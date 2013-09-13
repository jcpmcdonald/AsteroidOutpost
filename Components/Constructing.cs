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
	internal class Constructing : Component
	{
		private float mineralsConstructed;

		public event Action<ConstructionCompleteEventArgs> ConstructionComplete;

		public Constructing(int entityID) : base(entityID)
		{
			IsBeingPlaced = true;
			mineralsConstructed = 0;
		}


		
		public bool IsBeingPlaced { get; set; }
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
