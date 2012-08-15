﻿using System;
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

		public PowerGridNode(World world, int entityID)
			: base(world, entityID)
		{
		}


		protected PowerGridNode(BinaryReader br)
			: base(br)
		{
		}


		/// <summary>
		/// True if this node is active and ready to either conduct or produce power (not used for consumers)
		/// </summary>
		public bool PowerStateActive { get; set; }


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
		public Vector2 PowerLinkPointAbsolute { get; set; }
	}
}