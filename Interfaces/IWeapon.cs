using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsteroidOutpost.Interfaces
{
	public interface IWeapon
	{
		int Range { get; set; }
		float Damage { get; set; }
	}
}
