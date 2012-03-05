using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsteroidOutpost
{
	interface IPowerProducer
	{
		float AvailablePower { get; }

		bool GetPower(float amount);
	}
}
