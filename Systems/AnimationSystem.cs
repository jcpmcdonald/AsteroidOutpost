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
				foreach (var animator in world.GetComponents<Animator>(entity))
				{
					animator.SpriteAnimator.Draw(spriteBatch,
					                             world.WorldToScreen(world.GetComponent<Position>(entity).Center),
					                             MathHelper.ToRadians(animator.OrientationDiff),
					                             world.Scale(animator.Scale),
					                             animator.Tint);
				}
			}

			spriteBatch.End();
			base.Draw(gameTime);
		}
	}
}
