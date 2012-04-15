using System;
using System.Diagnostics;
using AsteroidOutpost.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AsteroidOutpost.Entities.Weapons
{
	abstract class Weapon
	{
		protected AsteroidOutpostScreen theGame;

		private int optimumRange;
		private int maxRange;
		protected Entity owner;
		protected Entity target;


		protected Weapon(AsteroidOutpostScreen theGame, Entity theOwner)
		{
			if(theGame == null)
			{
				Debugger.Break();
			}
			this.theGame = theGame;
			owner = theOwner;
		}

		public int OptimumRange
		{
			get { return optimumRange; }
			protected set { optimumRange = value; }
		}

		public int MaxRange
		{
			get { return maxRange; }
			protected set{ maxRange = value; }
		}


		public abstract void Shoot(Entity target);


		public abstract void Draw(SpriteBatch spriteBatch, Color tint);


		public abstract void Update(TimeSpan deltaTime);
	}
}
