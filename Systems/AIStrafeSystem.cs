using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsteroidOutpost.Systems
{
	class AIStrafeSystem : GameComponent
	{
		private World world;

		public AIStrafeSystem(AOGame game, World world)
			: base(game)
		{
			this.world = world;
		}
	}
}
