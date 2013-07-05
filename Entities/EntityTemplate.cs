﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
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


		public EntityTemplate(String json)
		{
			jsonTemplate = JObject.Parse(json);
		}


		public String Name
		{
			get
			{
				// All entities require a name, fail fast without one
				return jsonTemplate["Name"].ToString();
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
			template.ExtendWith(new JObject{ {"Components", jsonValues } });

			JObject componentsJson = template.jsonTemplate["Components"] as JObject;
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
							Sprite sprite = sprites[jsonTemplate["Components"][componentName]["SpriteName"].ToString().ToLower()];
							animator.SpriteAnimator = new SpriteAnimator(sprite);
							if (template.jsonTemplate["Components"][componentName]["CurrentOrientation"] != null)
							{
								float angleStep = 360.0f / sprite.OrientationLookup.Count;
								float spriteOrientation = float.Parse((String)template.jsonTemplate["Components"][componentName]["CurrentOrientation"]);
								template.jsonTemplate["Components"][componentName]["CurrentOrientation"] = (angleStep * (int)((spriteOrientation / angleStep) + 0.5f)) % 360;
							}
						}
						Populate(component, (JObject)template.jsonTemplate["Components"][componentName]);
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


		private void Populate(Component component, JObject jsonObject)
		{
			String json = jsonObject.ToString();
			if (json.Contains("\"**\""))
			{
				Console.WriteLine("There are required values that have not been filled in while populating {0}", GetType());
				Debugger.Break();
			}
			JsonConvert.PopulateObject(json, component);
		}


		public void ExtendWith(JObject donor)
		{
			Extend(jsonTemplate, donor);
		}


		private static void Extend(JObject receiver, JObject donor)
		{
			foreach (var property in donor)
			{
				JObject receiverValue = receiver[property.Key] as JObject;
				JObject donorValue = property.Value as JObject;
				if (receiverValue != null && donorValue != null)
				{
					Extend(receiverValue, donorValue);
				}
				else
				{
					receiver[property.Key] = property.Value;
				}
			}
		}
	}
}
