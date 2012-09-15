using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteroidOutpost.Screens;

namespace AsteroidOutpost.Components
{
	class EntityName : Component
	{
		public string Name { get; set; }
		
		public EntityName(World world, int entityID, String name) : base(world, entityID)
		{
			Name = name;
		}
	}
}
