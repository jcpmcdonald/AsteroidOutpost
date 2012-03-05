using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteroidOutpost.Screens;

namespace AsteroidOutpost.Scenarios
{
	public abstract class Scenario
	{
		private String name;
		private String author;

		protected AsteroidOutpostScreen theGame;


		protected Scenario(AsteroidOutpostScreen theGame)
		{
			this.theGame = theGame;
		}


		public abstract void Start();
		public void End() { }

		public abstract void Update(TimeSpan deltaTime);

	}
}
