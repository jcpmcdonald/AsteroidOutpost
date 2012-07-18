using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteroidOutpost.Entities;
using AsteroidOutpost.Screens;
using AsteroidOutpost.Screens.HeadsUpDisplay;
using C3.XNA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AsteroidOutpost.Systems
{
	class RenderSystem : DrawableGameComponent
	{
		private SpriteBatch spriteBatch;
		private World world;


		public RenderSystem(AOGame game, World world)
			: base(game)
		{
			this.world = world;
			spriteBatch = new SpriteBatch(game.GraphicsDevice);
		}


		public override void Draw(GameTime gameTime)
		{
			spriteBatch.Begin();

			// Draw all the visible entities
			List<Entity> visible = world.QuadTree.GetObjects(world.HUD.FocusScreen);
			foreach (Entity entity in visible)
			{
				entity.Draw(spriteBatch, 1, Color.White);
			}

			spriteBatch.End();
			base.Draw(gameTime);
		}
	}
}
