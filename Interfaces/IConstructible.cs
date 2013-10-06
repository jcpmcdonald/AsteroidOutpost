using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsteroidOutpost.Interfaces
{
	interface IConstructible
	{
		int Cost { get; set; }
		float MineralsConstructed { get; set; }
		int Priority { get; set; }
	}
}
