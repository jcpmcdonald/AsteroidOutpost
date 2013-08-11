using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectMercury;
using ProjectMercury.Renderers;

namespace AsteroidOutpost.Systems
{
	public class ParticleEngine : DrawableGameComponent
	{
		private World world;
		private ParticleEffectManager particleEffectManager;


		public ParticleEngine(AOGame game, World world)
			: base(game)
		{
			this.world = world;
			particleEffectManager = game.ParticleEffectManager;
		}


		public override void Update(GameTime gameTime)
		{
			float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
			foreach (var effect in particleEffectManager.ParticleEffects)
			{
				effect.Update(delta);
			}
			base.Update(gameTime);
		}


		public override void Draw(GameTime gameTime)
		{

			foreach (var effect in particleEffectManager.ParticleEffects)
			{
				Console.WriteLine(effect.ActiveParticlesCount);

				Matrix dummyMatrix = Matrix.Identity;
				Vector3 dummyVector3 = Vector3.Zero;
				particleEffectManager.Renderer.Transformation = world.WorldToScreenTransform;
				particleEffectManager.Renderer.RenderEffect(effect, ref dummyMatrix, ref dummyMatrix, ref dummyMatrix, ref dummyVector3);
			}

			base.Draw(gameTime);
		}
	}
}
