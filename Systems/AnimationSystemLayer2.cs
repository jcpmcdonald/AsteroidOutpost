using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AsteroidOutpost.Systems
{
	public class AnimationSystemLayer2 : DrawableGameComponent
	{
		private AnimationSystem animationSystem;

		public AnimationSystemLayer2(AOGame game, World world, AnimationSystem animationSystem)
			: base(game)
		{
			this.animationSystem = animationSystem;
		}

		public override void Draw(GameTime gameTime)
		{
			animationSystem.DrawUpperLayer(gameTime);
		}
	}
}
