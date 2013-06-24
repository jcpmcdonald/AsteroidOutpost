using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using AsteroidOutpost.Components;
using AsteroidOutpost.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using XNASpriteLib;

namespace AsteroidOutpost.Entities
{
	internal static class EntityFactory
	{
		private static World world;
		private static Dictionary<String, Sprite> sprites = new Dictionary<String, Sprite>();
		private static object loadSync = new object();


		//private static EntityTemplate asteroidTemplate;
		private static Dictionary<String, EntityTemplate> templates = new Dictionary<string, EntityTemplate>();


		public static void LoadContent(GraphicsDevice graphicsDevice)
		{
			// Note: The world doesn't exist yet, so don't use it here

			// Set up the sprites
			foreach(var spriteFileName in Directory.EnumerateFiles(@"..\sprites\", "*.sprx"))
			{
				Sprite sprite = new Sprite(File.OpenRead(spriteFileName), graphicsDevice);
				sprites.Add(sprite.Name.ToLower(), sprite);
			}


			// Set up the entity templates
			String entityJson = File.ReadAllText(@"..\entities\Entity.json");
			EntityTemplate baseEntity = new EntityTemplate(entityJson); // JsonConvert.DeserializeObject<EntityTemplate>(entityJson);

			foreach(var templateFileName in Directory.EnumerateFiles(@"..\entities\", "*.json"))
			{
				FileInfo templateFile = new FileInfo(templateFileName);
				String fileName = templateFile.Name.Substring(0, templateFile.Name.Length - templateFile.Extension.Length).ToLower();
				if(fileName != "entity")
				{
					EntityTemplate template = (EntityTemplate)baseEntity.Clone();
					String json = templateFile.OpenText().ReadToEnd();
					template.ExtendWith(JObject.Parse(json));
					templates.Add(fileName, template);
				}
			}

			lock (LoadSync)
			{
				LoadProgress = 100;
			}
		}


		public static int LoadProgress { get; private set; }

		public static object LoadSync
		{
			get
			{
				return loadSync;
			}
			private set
			{
				loadSync = value;
			}
		}


		public static void Init(World theWorld)
		{
			world = theWorld;
		}


		public static int Create(String entityName, Force owningForce, JObject jsonValues)
		{
			int entityID = world.GetNextEntityID();
			world.SetOwningForce(entityID, owningForce);

			List<Component> components = templates[entityName.ToLower()].Instantiate(entityID, jsonValues, sprites);
			foreach (var component in components)
			{
				world.AddComponent(component);
			}

			return entityID;
		}

	}
}
