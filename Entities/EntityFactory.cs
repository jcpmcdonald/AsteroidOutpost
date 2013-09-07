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
	internal class EntityFactory
	{
		private Dictionary<String, Sprite> sprites = new Dictionary<String, Sprite>();
		private object loadSync = new object();


		//private static EntityTemplate asteroidTemplate;
		internal Dictionary<String, EntityTemplate> templates = new Dictionary<string, EntityTemplate>();
		


		public void LoadContent(GraphicsDevice graphicsDevice)
		{
			// Note: The world doesn't exist yet, so don't use it here

			// Set up the sprites
			String curdirBackup = Directory.GetCurrentDirectory();
			Directory.SetCurrentDirectory(@"..\data\sprites\");
			foreach(var spriteFileName in Directory.EnumerateFiles(".", "*.sprx"))
			{
				Sprite sprite = new Sprite(File.OpenRead(spriteFileName), graphicsDevice);
				sprites.Add(sprite.Name.ToLower(), sprite);
			}
			Directory.SetCurrentDirectory(curdirBackup);

			LoadEntityTemplates();

			lock (LoadSync)
			{
				LoadProgress = 100;
			}
		}

		public void LoadEntityTemplates()
		{
			String entityJson = File.ReadAllText(@"..\data\entities\Entity.json");
			JObject entityJObject = JObject.Parse(entityJson);
			EntityTemplate baseEntity = new EntityTemplate((JObject)entityJObject["Components"]); // JsonConvert.DeserializeObject<EntityTemplate>(entityJson);

			foreach(var templateFileName in Directory.EnumerateFiles(@"..\data\entities\", "*.json"))
			{
				FileInfo templateFile = new FileInfo(templateFileName);
				String fileName = templateFile.Name.Substring(0, templateFile.Name.Length - templateFile.Extension.Length).ToLower();
				if(fileName != "entity")
				{
					EntityTemplate template = (EntityTemplate)baseEntity.Clone();
					String json = templateFile.OpenText().ReadToEnd();
					JObject jObject = JObject.Parse(json);
					template.ExtendWith((JObject)jObject["Components"]);
					if(template.Name != "**")
					{
						templates.Add(template.Name.ToLower(), template);
					}
					else
					{
						Console.WriteLine("An entity name must be provided in the entity JSON");
						Debugger.Break();
					}
				}
			}
		}


		public void Refresh(World world)
		{
			Console.WriteLine("REFRESHING Entity Templates");
			templates.Clear();
			LoadEntityTemplates();

			Dictionary<String, JObject> cleanedTemplates = new Dictionary<string, JObject>(templates.Count);
			foreach (var entityTemplate in templates)
			{
				cleanedTemplates.Add(entityTemplate.Key, entityTemplate.Value.RemoveVariables());
			}

			foreach (var entityName in world.GetComponents<EntityName>())
			{
				if(cleanedTemplates.ContainsKey(entityName.Name.ToLower()))
				{
					JObject cleanTemplate = cleanedTemplates[entityName.Name.ToLower()];
					EntityTemplate.Update(world, entityName.EntityID, cleanTemplate);
				}
				else
				{
					Console.WriteLine("Did not update entity because it has a non-standard name: " + entityName.Name);
				}
			}
		}


		


		public int LoadProgress { get; private set; }

		public object LoadSync
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


		public int Create(World world, String entityName, Force owningForce, JObject jsonValues)
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
