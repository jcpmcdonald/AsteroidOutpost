using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteroidOutpost.Eventing;
using AsteroidOutpost.Interfaces;
using Microsoft.Xna.Framework;

namespace AsteroidOutpost.Components
{
	class Upgrading : Component, IConstructible
	{
		private float mineralsUpgraded;

		public event Action<UpgradeCompleteEventArgs> UpgradeComplete;

		public Upgrading(int entityID)
			: base(entityID)
		{
		}


		public int Cost { get; set; }
		public float MineralsConstructed
		{
			get
			{
				return mineralsUpgraded;
			}
			set
			{
				mineralsUpgraded = MathHelper.Clamp(value, 0, Cost);
			}
		}

		public string UpgradeName { get; set; }


		public void OnUpgradeComplete(UpgradeCompleteEventArgs e)
		{
			if(UpgradeComplete != null)
			{
				UpgradeComplete(e);
			}
		}
	}
}
