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
			if(world.Paused) { return; }

			foreach (Animator animator in world.GetComponents<Animator>())
			{
				animator.SpriteAnimator.Update(gameTime);
			}

			foreach (Spin spinner in world.GetComponents<Spin>())
			{
				foreach(Animator animator in world.GetComponents<Animator>(spinner))
				{
					animator.SetOrientation(animator.Angle + (spinner.RotationSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds), spinner.RotateFrame);
				}
			}
		}


		public override void Draw(GameTime gameTime)
		{
			spriteBatch.Begin();

			// Draw all the visible entities
			foreach (int entity in world.QuadTree.GetObjects(world.HUD.FocusScreen).Select(x => x.EntityID))
			{
				foreach (var animator in world.GetComponents<Animator>(entity).Where(x => x.Layer <= 0))
				{
					animator.SpriteAnimator.Draw(spriteBatch,
					                             world.WorldToScreen(world.GetComponent<Position>(entity).Center + animator.Offset),
					                             MathHelper.ToRadians(animator.FrameAngle),
					                             world.Scale(animator.Scale),
					                             animator.Tint);
				}

				AnimatorSet animatorSet = world.GetNullableComponent<AnimatorSet>(entity);
				if(animatorSet != null)
				{
					var animators = animatorSet.SpriteAnimators.Where(x => x.Value <= 0).Select(x => x.Key);
					foreach (var animator in animators)
					{
						animator.Draw(spriteBatch,
						              world.WorldToScreen(world.GetComponent<Position>(entity).Center + animatorSet.Offset),
						              MathHelper.ToRadians(animatorSet.FrameAngle),
						              world.Scale(animatorSet.Scale),
						              animatorSet.Tint);
					}
				}
			}

			spriteBatch.End();
			base.Draw(gameTime);
		}


		public void DrawUpperLayer(GameTime gameTime)
		{
			spriteBatch.Begin();

			// Draw all the visible entities
			foreach (int entity in world.QuadTree.GetObjects(world.HUD.FocusScreen).Select(x => x.EntityID))
			{
				foreach (var animator in world.GetComponents<Animator>(entity).Where(x => x.Layer > 0))
				{
					animator.SpriteAnimator.Draw(spriteBatch,
					                             world.WorldToScreen(world.GetComponent<Position>(entity).Center + animator.Offset),
					                             MathHelper.ToRadians(animator.FrameAngle),
					                             world.Scale(animator.Scale),
					                             animator.Tint);
				}

				AnimatorSet animatorSet = world.GetNullableComponent<AnimatorSet>(entity);
				if(animatorSet != null)
				{
					var animators = animatorSet.SpriteAnimators.Where(x => x.Value > 0).Select(x => x.Key);
					foreach (var animator in animators)
					{
						animator.Draw(spriteBatch,
						              world.WorldToScreen(world.GetComponent<Position>(entity).Center + animatorSet.Offset),
						              MathHelper.ToRadians(animatorSet.FrameAngle),
						              world.Scale(animatorSet.Scale),
						              animatorSet.Tint);
					}
				}
			}

			spriteBatch.End();
			//base.Draw(gameTime);
		}
	}
}
