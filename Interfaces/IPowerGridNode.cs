using System;
using AsteroidOutpost.Entities.Eventing;
using Microsoft.Xna.Framework;

namespace AsteroidOutpost.Interfaces
{
	internal interface IPowerGridNode
	{
		event Action<EntityDyingEventArgs> DyingEvent;


		/// <summary>
		/// True if this node is active and ready to either conduct or produce power (not used for consumers)
		/// </summary>
		bool PowerStateActive { get; }


		/// <summary>
		/// True if this conducts power
		/// </summary>
		bool ConductsPower { get; }


		/// <summary>
		/// True if this produces power  (note that power producers should also conduct power)
		/// </summary>
		bool ProducesPower { get; }


		/// <summary>
		/// Returns an offset from the center showing where the power link should be displayed
		/// </summary>
		/// <returns>Returns an offset from the center, showing where the power link should be displayed</returns>
		Vector2 PowerLinkPointRelative { get; }


		/// <summary>
		/// Returns the absolute location showing where the power link should be displayed
		/// </summary>
		/// <returns>Returns the absolute location showing where the power link should be displayed</returns>
		Vector2 PowerLinkPointAbsolute { get; }
	}
}
