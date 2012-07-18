using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteroidOutpost.Components;
using AsteroidOutpost.Screens;
using Microsoft.Xna.Framework;

namespace AsteroidOutpost.Systems
{
	public class AnimationSystem : GameComponent
	{
		private readonly World world;

		public AnimationSystem(AOGame game, World world)
			: base(game)
		{
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
	}
}
