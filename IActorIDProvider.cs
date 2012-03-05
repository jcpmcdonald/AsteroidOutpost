using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsteroidOutpost
{
	interface IActorIDProvider
	{
		int GetNextActorID();
	}
}
