using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using AsteroidOutpost.Components;
using Newtonsoft.Json;
using XNASpriteLib;

namespace AsteroidOutpost.Entities
{
	class EntityTemplate : ICloneable
	{
		public String SpriteName;
		public Dictionary<String, Dictionary<String, Object>> componentDefaults = new Dictionary<String, Dictionary<String, Object>>();
		//public Dictionary<String, String> componentDefaults = new Dictionary<String, String>();
		//public List<object[]> componentDefaults = new List<object[]>();
		//public Dictionary<String, List<String[]>> componentDefaults = new Dictionary<String, List<String[]>>();
		//public List<String[]> componentDefaults = new List<String[]>();

		//public EntityTemplate(){}
		//public EntityTemplate(EntityTemplate template)
		//{
		//    SpriteName = template.SpriteName;
		//    componentDefaults = template.componentDefaults.
		//}

		//public EntityTemplate Extend(EntityTemplate extension)
		//{
		//    EntityTemplate rv = new EntityTemplate(this);
		//}

		public object Clone()
		{
			EntityTemplate clone = new EntityTemplate();
			clone.SpriteName = SpriteName;
			clone.componentDefaults = new Dictionary<string, Dictionary<string, object>>(componentDefaults);
			return clone;
		}


		public List<Component> Instantiate(int entityID, Dictionary<String, object> values, Dictionary<String, Sprite> sprites)
		{
			return Instantiate(entityID, JsonConvert.SerializeObject(values), sprites);
		}

		public List<Component> Instantiate(int entityID, String valuesJson, Dictionary<String, Sprite> sprites)
		{
			List<Component> components = new List<Component>();

			// Duplicate the current template so that we can apply the object's values (this could be optimized out if it's a problem)
			EntityTemplate template = (EntityTemplate)Clone();
			//JsonConvert.PopulateObject("{\"componentDefaults\":" + valuesJson + "}", template);
			template.ExtendRecursivelyWith("{\"componentDefaults\":" + valuesJson + "}");

			foreach(var componentName in componentDefaults.Keys)
			{
				Type componentType = Type.GetType("AsteroidOutpost.Components." + componentName);
				if(componentType != null)
				{
					Component component = Activator.CreateInstance(componentType, entityID) as Component;
					if(component != null)
					{
						// Great, let's fill it up!
						Animator animator = component as Animator;
						if(animator != null)
						{
							animator.SpriteAnimator = new SpriteAnimator(sprites[SpriteName]);
							if(template.componentDefaults[componentName].ContainsKey("CurrentOrientation"))
							{
								float angleStep = 360.0f / sprites[SpriteName].OrientationLookup.Count;
								float spriteOrientation = float.Parse((String)template.componentDefaults[componentName]["CurrentOrientation"]);
								template.componentDefaults[componentName]["CurrentOrientation"] = (angleStep * (int)((spriteOrientation / angleStep) + 0.5f)) % 360;
							}
						}
						Populate(component, template.componentDefaults[componentName]);
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


		private void Populate(Component component, Dictionary<String, Object> componentValues)
		{
			// hahah, so wasteful, but so easy
			String json = JsonConvert.SerializeObject(componentValues);
			if(json.Contains("\"**\""))
			{
				Console.WriteLine("There are required values that have not been filled in while populating {0}", GetType());
				Debugger.Break();
			}
			JsonConvert.PopulateObject(json, component);
		}


		public void ExtendRecursivelyWith(String json)
		{
			EntityTemplate tmp = JsonConvert.DeserializeObject<EntityTemplate>(json);


			if(tmp.SpriteName != null)
			{
				SpriteName = tmp.SpriteName;
			}


			// Go through the componentDefaults of the temporary object, and merge/add values from there to here
			foreach(var componentName in tmp.componentDefaults.Keys)
			{
				if(componentDefaults.ContainsKey(componentName))
				{
					// Dig deeper
					foreach (var varName in tmp.componentDefaults[componentName].Keys)
					{
						if(componentDefaults[componentName].ContainsKey(varName))
						{
							// Dig deeper?
							componentDefaults[componentName][varName] = tmp.componentDefaults[componentName][varName];
						}
						else
						{
							componentDefaults[componentName].Add(varName, tmp.componentDefaults[componentName][varName]);
						}
					}
				}
				else
				{
					// Add
					componentDefaults.Add(componentName, tmp.componentDefaults[componentName]);
				}
			}
		}
	}
}
