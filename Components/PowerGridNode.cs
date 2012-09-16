using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AsteroidOutpost.Entities.Eventing;
using AsteroidOutpost.Screens;
using Microsoft.Xna.Framework;

namespace AsteroidOutpost.Components
{
	public class PowerGridNode : Component
	{
		private bool producesPower = false;
		protected bool conductsPower;


		public PowerGridNode(World world, int entityID, bool conductsPower)
			: base(world, entityID)
		{
			this.conductsPower = conductsPower;
		}


		/// <summary>
		/// True if this node is active and ready to either conduct or produce power (not used for consumers)
		/// </summary>
		public bool PowerStateActive
		{
			get
			{
				Constructable constructable = world.GetNullableComponent<Constructable>(EntityID);
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
		public bool ConductsPower
		{
			get
			{
				return conductsPower;
			}
			set
			{
				conductsPower = value;
			}
		}


		/// <summary>
		/// True if this produces power  (note that power producers should also conduct power)
		/// </summary>
		public bool ProducesPower
		{
			get
			{
				return producesPower;
			}
			set
			{
				producesPower = value;
			}
		}


		/// <summary>
		/// Returns an offset from the center showing where the power link should be displayed
		/// </summary>
		/// <returns>Returns an offset from the center, showing where the power link should be displayed</returns>
		public Vector2 PowerLinkPointRelative { get; set; }


		/// <summary>
		/// Returns the absolute location showing where the power link should be displayed
		/// </summary>
		/// <returns>Returns the absolute location showing where the power link should be displayed</returns>
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
