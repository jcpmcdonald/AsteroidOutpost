using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using AsteroidOutpost.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AsteroidOutpost
{
	internal class ContextButton
	{
		public String Name { get; set; }
		public String Image { get; set; }
		public int Slot { get; set; }
		public Dictionary<String, String> ImportantValues;
		private bool Enabled { get; set; }


		public ContextButton(String name)
		{
			this.Name = name;
			Enabled = true;
		}


		public void Initialize(Dictionary<String, EntityTemplate> entityTemplates)
		{
			Name = Evaluate(Name, entityTemplates) ?? Name;
			Image = Evaluate(Image, entityTemplates) ?? Image;

			String[] keys = new String[ImportantValues.Count];
			ImportantValues.Keys.CopyTo(keys, 0);
			foreach (var key in keys)
			{
				ImportantValues[key] = Evaluate(ImportantValues[key], entityTemplates) ?? ImportantValues[key];
			}
		}


		/// <summary>
		/// Looks up the value for a given field inside the template.
		/// </summary>
		/// <param name="fullField">The full field name, eg "%Constructible.Cost% minerals"</param>
		/// <param name="template">The template that contains the desired information</param>
		/// <returns>Returns the new field</returns>
		private String Evaluate(String fullField, Dictionary<String, EntityTemplate> entityTemplates)
		{
			int percentStart = fullField.IndexOf("%", StringComparison.InvariantCulture);
			while(percentStart >= 0)
			{
				int percentEnd = fullField.IndexOf("%", percentStart + 1, StringComparison.InvariantCulture);
				if(percentEnd >= 0)
				{
					String percentField = fullField.Substring(percentStart, percentEnd - percentStart + 1);
					fullField = fullField.Replace(percentField, GetFieldValue(percentField.Replace("%", ""), entityTemplates));

					percentStart = fullField.IndexOf("%", StringComparison.InvariantCulture);
				}
				else
				{
					percentStart = -1;
				}
			}
			return fullField;
		}


		private String GetFieldValue(String field, Dictionary<String, EntityTemplate> entityTemplates)
		{
			String[] fieldParts = field.Split(new char[]{ '.' });

			if(entityTemplates.ContainsKey(fieldParts[0].ToLowerInvariant()))
			{
				JToken curr = entityTemplates[fieldParts[0].ToLowerInvariant()].jsonTemplate;

				for (int i = 1; i < fieldParts.Length; i++)
				{
					var fieldPart = fieldParts[i];
					if (curr[fieldPart] != null)
					{
						curr = curr[fieldPart];
					}
					else
					{
						Console.WriteLine("Could not find a value for a percent-delimited field. Not good");
						Debugger.Break();
						return null;
					}
				}

				return curr.ToString();
			}
			else
			{
				Console.WriteLine("Could not find the entity name in the percent-delimited field");
				Debugger.Break();
				return field;
			}
		}


		public String GetJSON()
		{
			JObject jObject = new JObject{
				{ "$Image", Image },
				{ "$Name", Name },
				{ "$Enabled", Enabled.ToString().ToLowerInvariant() }
			};

			jObject.Extend(JObject.FromObject(ImportantValues));

			return jObject.ToString(Formatting.None);
		}
	}
}
