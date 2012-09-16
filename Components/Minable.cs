using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AsteroidOutpost.Screens;
using Microsoft.Xna.Framework;

namespace AsteroidOutpost.Components
{
	class Minable : Component
	{
		private int minerals;

		public Minable(World world, int entityID, int minerals)
			: base(world, entityID)
		{
			this.minerals = minerals;
		}


		public int Minerals
		{
			get
			{
				return minerals;
			}
			set
			{
				minerals = Math.Max(0, value);
			}
		}
	}
}
