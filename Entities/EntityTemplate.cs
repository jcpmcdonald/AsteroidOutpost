using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using AsteroidOutpost.Components;
using AsteroidOutpost.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using XNASpriteLib;

namespace AsteroidOutpost.Entities
{
	internal class EntityTemplate : ICloneable
	{
		public JObject jsonTemplate;

		private EntityTemplate()
		{
		}


		//public EntityTemplate(String json)
		//{
		//    jsonTemplate = JObject.Parse(json);
		//}

		public EntityTemplate(JObject jObject)
		{
			jsonTemplate = jObject;
		}


		public String Name
		{
			get
			{
				return jsonTemplate["EntityName"]["Name"].ToString();
			}
		}


		public object Clone()
		{
			EntityTemplate clone = new EntityTemplate();
			clone.jsonTemplate = (JObject)jsonTemplate.DeepClone();
			return clone;
		}


		public List<Component> Instantiate(int entityID, JObject jsonValues, Dictionary<String, Sprite> sprites)
		{
			List<Component> components = new List<Component>();

			// Duplicate the current template so that we can apply the object's values (this could be optimized out if it's a problem)
			EntityTemplate template = (EntityTemplate)Clone();
			template.ExtendWith(jsonValues);

			JObject componentsJson = template.jsonTemplate;
			foreach (var componentJson in componentsJson)
			{
				String componentName = componentJson.Key;
				Type componentType = Type.GetType("AsteroidOutpost.Components." + componentName, false, true);
				if (componentType != null)
				{
					Component component = Activator.CreateInstance(componentType, entityID) as Component;
					if (component != null)
					{
						// Great, let's fill it up!
						if (componentType == typeof(Animator))
						{
							FillAnimator((Animator)component, componentJson, sprites);
						}
						if(componentType == typeof(AnimatorSet))
						{
							FillAnimatorSet((AnimatorSet)component, componentJson, sprites);
							//if(componentJson.Value["SpriteName2"] != null)
							//{
							//    Animator animator2 = new Animator(entityID);
							//    Sprite sprite2 = sprites[componentJson.Value["SpriteName2"].ToString().ToLower()];
							//    animator2.SpriteAnimator = new SpriteAnimator(sprite2);
							//    Populate(animator2, (JObject)componentJson.Value);
							//    animator2.Layer = 1;
							//    components.Add(animator2);
							//}
						}
						Populate(component, (JObject)componentJson.Value);
						components.Add(component);
					}
					else
					{
						Console.WriteLine("Component type was not a component: '{0}' while Instantiating '{1}'", componentName, GetType());
						Debugger.Break();
					}
				}
				else
				{
					Console.WriteLine("Unrecognized component type '{0}' while Instantiating '{1}'", componentName, GetType());
					Debugger.Break();
				}
			}

			return components;
		}


		private void FillAnimator(Animator animator, KeyValuePair<String, JToken> componentJson, Dictionary<String, Sprite> sprites)
		{
			Sprite sprite = sprites[componentJson.Value["SpriteName"].ToString().ToLower()];
			animator.SpriteAnimator = new SpriteAnimator(sprite);

			if (componentJson.Value["CurrentOrientation"] == null)
			{
				componentJson.Value["CurrentOrientation"] = (float)GlobalRandom.Next(0, 359);
			}
		}


		private void FillAnimatorSet(AnimatorSet animatorSet, KeyValuePair<String, JToken> componentJson, Dictionary<String, Sprite> sprites)
		{
			List<String> spriteNames = new List<String>();
			List<int> spriteLayers = new List<int>();
			JsonConvert.PopulateObject(componentJson.Value["SpriteNames"].ToString(), spriteNames);
			JsonConvert.PopulateObject(componentJson.Value["SpriteLayers"].ToString(), spriteLayers);

			animatorSet.SpriteAnimators = new List<KeyValuePair<SpriteAnimator, int>>();
			for (int i = 0; i < spriteNames.Count; i++)
			{
				SpriteAnimator sprite = new SpriteAnimator(sprites[spriteNames[i].ToLower()]);
				animatorSet.SpriteAnimators.Add(new KeyValuePair<SpriteAnimator, int>(sprite, spriteLayers[i]));
			}

			if (componentJson.Value["CurrentOrientation"] == null)
			{
				componentJson.Value["CurrentOrientation"] = (float)GlobalRandom.Next(0, 359);
			}
		}


		/// <summary>
		/// Used to clean out the variables from a template
		/// </summary>
		/// <returns></returns>
		public JObject RemoveVariables()
		{
			return RemoveVariables(jsonTemplate);
		}


		private JObject RemoveVariables(JObject template)
		{
			JObject cleanTemplate = new JObject();
			foreach (var property in template)
			{
				JObject value = property.Value as JObject;
				if(value != null)
				{
					cleanTemplate[property.Key] = RemoveVariables(value);
				}
				else
				{
					if(property.Value.ToString() != "**")
					{
						cleanTemplate[property.Key] = property.Value;
					}
				}
			}
			return cleanTemplate;
		}


		public static void Update(World world, int entityID, JObject componentsJson)
		{
			foreach (var componentJson in componentsJson)
			{
				String componentName = componentJson.Key;
				Type componentType = Type.GetType("AsteroidOutpost.Components." + componentName, false, true);
				if (componentType != null)
				{
					Component component = world.GetComponent(entityID, componentType);
					Populate(component, (JObject)componentsJson[componentName]);
				}
				else
				{
					Console.WriteLine("Unrecognized component type '{0}' while Updating an object", componentName);
					Debugger.Break();
				}
			}
		}


		private static void Populate(Component component, JObject jsonObject)
		{
			String json = jsonObject.ToString();
			if (json.Contains("\"**\""))
			{
				Console.WriteLine("There are required values that have not been filled in while populating");
				Debugger.Break();
			}

			try
			{
				JsonConvert.PopulateObject(json, component);
			}
			catch (FormatException fe)
			{
				//Debugger.Break();
				Console.WriteLine("Error parsing JSON: " + fe.StackTrace);
			}
		}

		public void ExtendWith(JObject donor)
		{
			jsonTemplate.Extend(donor);
		}
	}
}
