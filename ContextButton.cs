using System;
using System.Collections.Generic;
using System.Diagnostics;
using AsteroidOutpost.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AsteroidOutpost
{
	internal class ContextButton
	{
		public String Image { get; set; }
		public String Name { get; private set; }
		public int Slot { get; set; }
		public Dictionary<String, String> ImportantValues;


		public ContextButton(String name)
		{
			this.Name = name;
		}


		public void Initialize(EntityTemplate entityTemplate)
		{
			Image = Evaluate(Image, entityTemplate) ?? Image;

			String[] keys = new String[ImportantValues.Count];
			ImportantValues.Keys.CopyTo(keys, 0);
			foreach (var key in keys)
			{
				ImportantValues[key] = Evaluate(ImportantValues[key], entityTemplate) ?? ImportantValues[key];
			}
		}


		/// <summary>
		/// Looks up the value for a given field inside the template.
		/// </summary>
		/// <param name="fullField">The full field name, eg "%Constructible.Cost% minerals"</param>
		/// <param name="template">The template that contains the desired information</param>
		/// <returns>Returns the new field</returns>
		private String Evaluate(String fullField, EntityTemplate template)
		{
			int percentStart = fullField.IndexOf("%", StringComparison.InvariantCulture);
			while(percentStart >= 0)
			{
				int percentEnd = fullField.IndexOf("%", percentStart + 1, StringComparison.InvariantCulture);
				if(percentEnd >= 0)
				{
					String percentField = fullField.Substring(percentStart, percentEnd - percentStart + 1);
					fullField = fullField.Replace(percentField, GetFieldValue(percentField.Replace("%", ""), template));

					percentStart = fullField.IndexOf("%", StringComparison.InvariantCulture);
				}
				else
				{
					percentStart = -1;
				}
			}
			return fullField;
		}

		private String GetFieldValue(String field, EntityTemplate template)
		{
			String[] fieldParts = field.Split(new char[]{ '.' });
			JToken curr = template.jsonTemplate;

			foreach (var fieldPart in fieldParts)
			{
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


		public String GetJSON()
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
