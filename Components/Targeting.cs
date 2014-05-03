using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace AsteroidOutpost.Components
{
	public class Targeting : Component
	{
		public Targeting(int entityID)
			: base(entityID)
		{
		}

		public String TargetingType { get; set; }
		public int? Target { get; set; }

		[XmlIgnore]
		[JsonIgnore]
		public Vector2 TargetVector { get; set; }


		[XmlIgnore]
		[JsonIgnore]
		public new int EntityID
		{
			get
			{
				// Do not confuse EntityID with Target!
				Debugger.Break();
				return base.EntityID;
			}
		}
	}
}
