using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsteroidOutpost.Components
{
	public class Selectable : Component
	{

		public String ContextMenu { get; set; }

		public Selectable(int entityID)
			: base(entityID)
		{
		}
	}
}
