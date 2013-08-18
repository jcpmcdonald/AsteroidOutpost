using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AsteroidOutpost.Entities
{
	internal class ConstructionButton
	{
		public String Image { get; set; }
		public String Name { get; private set; }
		public int Order { get; set; }
		public Dictionary<String, String> ImportantValues;


		public ConstructionButton(String name)
		{
			this.Name = name;
		}


		public void Initialize(EntityTemplate entityTemplate)
		{
			Image = GetValueFor(Image, entityTemplate) ?? Image;

			String[] keys = new String[ImportantValues.Count];
			ImportantValues.Keys.CopyTo(keys, 0);
			foreach (var key in keys)
			{
				ImportantValues[key] = GetValueFor(ImportantValues[key], entityTemplate) ?? ImportantValues[key];
			}
		}


		/// <summary>
		/// Looks up the value for a given field inside the template.
		/// </summary>
		/// <param name="fullField">The full field name, eg "EntityName.Name"</param>
		/// <param name="template">The template that contains the desired information</param>
		/// <returns>Returns the value of the field, or null if not found</returns>
		private String GetValueFor(String fullField, EntityTemplate template)
		{
			String[] fields = fullField.Split(new char[]{ '.' });
			JToken curr = template.jsonTemplate;

			foreach (var field in fields)
			{
				if (curr[field] != null)
				{
					curr = curr[field];
				}
				else
				{
					return null;
				}
			}

			return curr.ToString();
		}


		public String GetButtonJSON()
		{
			JObject jObject = new JObject{
				{ "$Image", Image },
				{ "$Name", Name }
			};

			jObject.Extend(JObject.FromObject(ImportantValues));

			return jObject.ToString(Formatting.None);
		}
	}
}
