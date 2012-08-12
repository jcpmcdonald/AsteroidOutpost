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
	public class AnimationSystem : DrawableGameComponent
	{
		private SpriteBatch spriteBatch;
		private readonly World world;

		public AnimationSystem(AOGame game, World world)
			: base(game)
		{
			spriteBatch = new SpriteBatch(game.GraphicsDevice);
			this.world = world;
		}

		public override void Update(GameTime gameTime)
		{
			List<Animator> animators = world.GetComponents<Animator>();
			foreach (Animator animator in animators)
			{
				animator.SpriteAnimator.Update(gameTime);
			}
			base.Update(gameTime);
		}


		public override void Draw(GameTime gameTime)
		{
			spriteBatch.Begin();

			// Draw all the visible entities
			List<int> visibleEntities = world.QuadTree.GetObjects(world.HUD.FocusScreen).Select(x => x.EntityID).ToList();
			foreach (int entity in visibleEntities)
			{
				List<Animator> animators = world.GetComponents<Animator>(entity);
				foreach (var animator in animators)
				{
					animator.SpriteAnimator.Draw(spriteBatch, world.WorldToScreen(world.GetComponent<Position>(entity).Center), 0f, 1f / world.ScaleFactor, animator.Tint);
				}
			}

			spriteBatch.End();
			base.Draw(gameTime);
		}
	}
}
