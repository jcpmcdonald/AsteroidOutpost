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
		
		public EntityName(int entityID) : base(entityID){}
		public EntityName(int entityID, String name) : base(entityID)
		{
			Name = name;
		}
	}
}
