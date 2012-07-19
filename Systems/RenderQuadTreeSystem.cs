using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteroidOutpost.Entities;
using AsteroidOutpost.Screens;
using C3.XNA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AsteroidOutpost.Systems
{
	class RenderQuadTreeSystem : DrawableGameComponent
	{
		private World world;
		private SpriteBatch spriteBatch;

		private Color[] colorPalette = {
		                               	Color.White,
		                               	Color.Red,
		                               	Color.Green,
		                               	Color.Blue,
		                               	Color.Gray,
		                               	Color.DarkRed,
		                               	Color.DarkGreen,
		                               	Color.DarkBlue
		                               };

		protected internal bool DrawQuadTree { get; set; }

		public RenderQuadTreeSystem(AOGame game, World world)
			: base(game)
		{
			this.world = world;
			spriteBatch = new SpriteBatch(game.GraphicsDevice);
		}

		public override void Draw(GameTime gameTime)
		{
			spriteBatch.Begin();
			if(DrawQuadTree && world.QuadTree != null)
			{
				DrawQuad(spriteBatch, world.QuadTree.RootQuad, 0);
			}
			spriteBatch.End();
			base.Draw(gameTime);
		}

		private void DrawQuad(SpriteBatch spriteBatch, QuadTreeNode<Entity> quad, int depth)
		{
			if (quad != null)
			{

				Rectangle rect = quad.QuadRect;

				Color drawColor = colorPalette[depth % colorPalette.Length];

				Vector2 screenTopLeft = world.WorldToScreen(rect.X, rect.Y);
				Vector2 screenBottomLeft = world.WorldToScreen(rect.Right, rect.Bottom);

				rect = new Rectangle((int)screenTopLeft.X,
				                     (int)screenTopLeft.Y,
				                     (int)(screenBottomLeft.X - screenTopLeft.X),
				                     (int)(screenBottomLeft.Y - screenTopLeft.Y));
				spriteBatch.DrawRectangle(rect, drawColor, 1);

				DrawQuad(spriteBatch, quad.TopLeftChild, depth + 1);
				DrawQuad(spriteBatch, quad.TopRightChild, depth + 1);
				DrawQuad(spriteBatch, quad.BottomLeftChild, depth + 1);
				DrawQuad(spriteBatch, quad.BottomRightChild, depth + 1);
			}
		}
	}
}
