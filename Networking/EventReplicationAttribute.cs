using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsteroidOutpost.Networking
{
	[Flags]
	public enum EventReplication
	{
		ServerToClients,
		ClientToServer
	}

	[AttributeUsage(AttributeTargets.Event)]
	public class EventReplicationAttribute : Attribute
	{
		public EventReplication ReplicationTarget { get; set; }
		public EventReplicationAttribute(EventReplication theReplicationTarget)
		{
			ReplicationTarget = theReplicationTarget;
		}
	}
}
