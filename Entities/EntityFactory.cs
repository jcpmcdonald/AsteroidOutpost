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
		private static Dictionary<String, ConstructionButton> constructionButtons = new Dictionary<string, ConstructionButton>();


		public static void LoadContent(GraphicsDevice graphicsDevice)
		{
			// Note: The world doesn't exist yet, so don't use it here

			// Set up the sprites
			foreach(var spriteFileName in Directory.EnumerateFiles(@"..\sprites\", "*.sprx"))
			{
				Sprite sprite = new Sprite(File.OpenRead(spriteFileName), graphicsDevice);
				sprites.Add(sprite.Name.ToLower(), sprite);
			}

			LoadEntityTemplates();

			lock (LoadSync)
			{
				LoadProgress = 100;
			}
		}

		public static void LoadEntityTemplates()
		{
			String entityJson = File.ReadAllText(@"..\entities\Entity.json");
			JObject entityJObject = JObject.Parse(entityJson);
			EntityTemplate baseEntity = new EntityTemplate((JObject)entityJObject["Components"]); // JsonConvert.DeserializeObject<EntityTemplate>(entityJson);

			foreach(var templateFileName in Directory.EnumerateFiles(@"..\entities\", "*.json"))
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


					if(jObject["ConstructionButton"] != null)
					{
						ConstructionButton button = new ConstructionButton();
						JsonConvert.PopulateObject(jObject["ConstructionButton"].ToString(), button);
						button.Initialize(template);
						constructionButtons.Add(button.Name, button);
					}
				}
			}
		}


		public static void Refresh(World world)
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


		public static String GetConstructionButtonJSON()
		{
			String json = "[";
			foreach (var button in constructionButtons.Values.OrderBy(b => b.Order))
			{
				json += button.GetButtonJSON() + ",";
			}
			json += "]";
			return json;
		}
	}
}
