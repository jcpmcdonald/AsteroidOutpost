using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using AsteroidOutpost.Components;
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
							Animator animator = (Animator)component;
							Sprite sprite = sprites[jsonTemplate[componentName]["SpriteName"].ToString().ToLower()];
							animator.SpriteAnimator = new SpriteAnimator(sprite);
							if (template.jsonTemplate[componentName]["CurrentOrientation"] != null)
							{
								float angleStep = 360.0f / sprite.OrientationLookup.Count;
								float spriteOrientation = float.Parse((String)template.jsonTemplate[componentName]["CurrentOrientation"], CultureInfo.InvariantCulture);
								template.jsonTemplate[componentName]["CurrentOrientation"] = (angleStep * (int)((spriteOrientation / angleStep) + 0.5f)) % 360;
							}
						}
						Populate(component, (JObject)template.jsonTemplate[componentName]);
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
				Debugger.Break();
				Console.WriteLine("Error parsing JSON: " + fe.StackTrace);
			}
		}

		public void ExtendWith(JObject donor)
		{
			jsonTemplate.Extend(donor);
		}
	}
}
