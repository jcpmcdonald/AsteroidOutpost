using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsteroidOutpost.Entities
{
	public interface IUpdatable
	{
		void Update(TimeSpan deltaTime);
	}
}
