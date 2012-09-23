using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteroidOutpost.Components;
using AsteroidOutpost.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AsteroidOutpost.Systems
{
	class HitPointSystem : DrawableGameComponent
	{
		private readonly World world;
		//private SpriteBatch spriteBatch;

		public HitPointSystem(Game game, World world)
			: base(game)
		{
			this.world = world;
			//spriteBatch = new SpriteBatch(game.GraphicsDevice);
		}

		public override void Update(GameTime gameTime)
		{
			// TODO: This code will need to be changed to allow a client to delete entities
			bool authoratative = world.IsServer;
			foreach (var hitPoints in world.GetComponents<HitPoints>())
			{
				if (authoratative && hitPoints.Armour <= 0.0f)
				{
					// We have just died
					//SetDead(true);
					world.DeleteComponents(hitPoints.EntityID);
				}
			}
		}
	}
}
