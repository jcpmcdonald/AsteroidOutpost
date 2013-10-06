using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using AsteroidOutpost.Entities.Eventing;
using AsteroidOutpost.Screens;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace AsteroidOutpost.Components
{
	public class PowerGridNode : Component
	{
		public PowerGridNode(int entityID)
			: base(entityID)
		{
		}


		/// <summary>
		/// True if this conducts power
		/// </summary>
		public bool ConductsPower { get; set; }


		/// <summary>
		/// Returns an offset from the center showing where the power link should be displayed
		/// </summary>
		/// <returns>Returns an offset from the center, showing where the power link should be displayed</returns>
		public Vector2 PowerLinkPointRelative { get; set; }


		[JsonIgnore][XmlIgnore]
		public bool PowerStarved { get; set; }


		/// <summary>
		/// Returns the absolute location showing where the power link should be displayed
		/// </summary>
		/// <returns>Returns the absolute location showing where the power link should be displayed</returns>
		public Vector2 PowerLinkPointAbsolute(World world)
		{
			Position entityPosition = world.GetComponent<Position>(EntityID);
			return entityPosition.Center + PowerLinkPointRelative;
		}

	}
}
