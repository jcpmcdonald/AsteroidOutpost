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
		private Dictionary<String, ParticleEffect> particleEffects = new Dictionary<String, ParticleEffect>();

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
					emitter.ParticleTexture = content.Load<Texture2D>("ParticleTextures\\" + emitter.ParticleTextureAssetPath);
					emitter.Initialise();
				}
				particleEffects.Add(effect.Name, effect);
			}

			renderer.LoadContent(content);
		}

		public IEnumerable<ParticleEffect> ParticleEffects
		{
			get
			{
				return particleEffects.Values;
			}
		}

		public SpriteBatchRenderer Renderer
		{
			get
			{
				return renderer;
			}
		}


		public void Trigger(String name, Vector2 location)
		{
			Vector3 v3 = new Vector3(location.X, location.Y, 0f);
			particleEffects[name].Trigger(ref v3);
		}
	}
}
