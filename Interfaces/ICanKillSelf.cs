using System;

namespace AsteroidOutpost.Interfaces
{
	interface ICanKillSelf : IKillable
	{
		void KillSelf(EventArgs e);
	}
}
