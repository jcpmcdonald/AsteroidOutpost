using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using ProjectMercury;
using ProjectMercury.Renderers;

namespace AsteroidOutpost
{
	public class ParticleEffectManager
	{
		private SpriteBatchRenderer renderer;
		private List<ParticleEffect> particleEffects = new List<ParticleEffect>();

		public ParticleEffectManager()
		{
		}


		public void LoadParticles(IGraphicsDeviceService graphics, ContentManager content)
		{
			renderer = new SpriteBatchRenderer{ GraphicsDeviceService = graphics };;

			String particleEffectsDirName = "ParticleEffects";
			DirectoryInfo particleEffectsDir = new DirectoryInfo(Environment.CurrentDirectory + "\\" + content.RootDirectory + "\\" + particleEffectsDirName);
			if (!particleEffectsDir.Exists)
			{
				Console.WriteLine("Directory '" + particleEffectsDirName + "' was not found. Directory is required for particles");
				Debugger.Break();
				throw new DirectoryNotFoundException("Directory '" + particleEffectsDirName + "' was not found. Directory is required for particles");
			}

			FileInfo[] files = particleEffectsDir.GetFiles("*.*");
			foreach (FileInfo file in files)
			{
				ParticleEffect effect = content.Load<ParticleEffect>(particleEffectsDirName + "\\" + Path.GetFileNameWithoutExtension(file.Name));

				foreach(var emitter in effect.Emitters)
				{
					emitter.ParticleTexture = content.Load<Texture2D>(emitter.ParticleTextureAssetPath);
					emitter.Initialise();
				}
				particleEffects.Add(effect);
			}

			renderer.LoadContent(content);
		}


		public void Add(ParticleEffect effect)
		{
			particleEffects.Add(effect);
		}

		public IEnumerable<ParticleEffect> ParticleEffects
		{
			get
			{
				return particleEffects;
			}
		}

		public SpriteBatchRenderer Renderer
		{
			get
			{
				return renderer;
			}
		}


		public void Trigger(Vector3 location)
		{
			particleEffects[0].Trigger(ref location);
		}
	}
}
