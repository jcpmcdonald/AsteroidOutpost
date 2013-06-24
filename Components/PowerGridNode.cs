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
		protected World world;

		public PowerGridNode(World world, int entityID, bool conductsPower)
			: base(entityID)
		{
			this.world = world;
			ProducesPower = false;
			this.ConductsPower = conductsPower;
		}


		/// <summary>
		/// True if this node is active and ready to either conduct or produce power (not used for consumers)
		/// </summary>
		[XmlIgnore]
		[JsonIgnore]
		public bool PowerStateActive
		{
			get
			{
				Constructible constructable = world.GetNullableComponent<Constructible>(EntityID);
				if(constructable != null)
				{
					return !constructable.IsBeingPlaced && !constructable.IsConstructing;
				}
				else
				{
					// I suppose I'm active?
					return true;
				}
			}
		}


		/// <summary>
		/// True if this conducts power
		/// </summary>
		public bool ConductsPower { get; set; }


		/// <summary>
		/// True if this produces power  (note that power producers should also conduct power)
		/// </summary>
		public bool ProducesPower { get; set; }


		/// <summary>
		/// Returns an offset from the center showing where the power link should be displayed
		/// </summary>
		/// <returns>Returns an offset from the center, showing where the power link should be displayed</returns>
		public Vector2 PowerLinkPointRelative { get; set; }


		/// <summary>
		/// Returns the absolute location showing where the power link should be displayed
		/// </summary>
		/// <returns>Returns the absolute location showing where the power link should be displayed</returns>
		[XmlIgnore]
		[JsonIgnore]
		public Vector2 PowerLinkPointAbsolute
		{
			get
			{
				Position entityPosition = world.GetComponent<Position>(EntityID);
				return entityPosition.Center + PowerLinkPointRelative;
			}
		}
	}
}
