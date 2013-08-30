using System;
using AsteroidOutpost.Eventing;
using AsteroidOutpost.Networking;

namespace AsteroidOutpost.Components
{
	public class Perishable : Component
	{
		[EventReplication(EventReplication.ServerToClients)]
		public event Action<EntityPerishingEventArgs> Perishing;


		public Perishable(int entityID)
			: base(entityID)
		{
		}


		public String ParticleEffectOnPerish { get; set; }


		public void OnPerish(EntityPerishingEventArgs e)
		{
			if(Perishing != null)
			{
				Perishing(e);
			}
		}
	}
}
