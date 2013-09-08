﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using AsteroidOutpost.Entities;
using AsteroidOutpost.Systems;
using Awesomium.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AsteroidOutpost
{
	internal class ContextButton
	{
		public event Action<ContextButton> ButtonChanged;

		private bool enabled;
		private int slot;
		private string image;
		private string name;
		public Dictionary<String, String> ImportantValues;
		private string callbackJS;
		private string hotkey;
		private Call onClickData;

		[JsonIgnore]
		private AOHUD hud;

		public String Name
		{
			get { return name; }
			set
			{
				name = value;
				OnButtonChanged();
			}
		}

		public String Image
		{
			get { return image; }
			set
			{
				image = value;
				OnButtonChanged();
			}
		}

		public int Slot
		{
			get { return slot; }
			set
			{
				slot = value;
				OnButtonChanged();
			}
		}

		public String Hotkey
		{
			get { return hotkey; }
			set
			{
				hotkey = value;
				OnButtonChanged();
			}
		}

		
		public Call OnClickData
		{
			set
			{
				onClickData = value;
				OnButtonChanged();
			}
		}

		public String CallbackJS
		{
			get { return callbackJS; }
			set
			{
				callbackJS = value;
				OnButtonChanged();
			}
		}

		public bool Enabled
		{
			get { return enabled; }
			set
			{
				enabled = value;
				OnButtonChanged();
			}
		}


		public ContextButton(String name)
		{
			this.Name = name;
			Enabled = true;
		}


		private void OnButtonChanged()
		{
			if(ButtonChanged != null)
			{
				ButtonChanged(this);
			}
		}


		private void OnClick(Object sender, JavascriptMethodEventArgs args)
		{
			if(onClickData != null)
			{
				onClickData.Invoke(hud);
			}
			else
			{
				Console.WriteLine("There is no click data!");
				Debugger.Break();
			}
		}


		public void Initialize(String pageName, AOHUD hud, JSObject jsContextMenu, Dictionary<String, EntityTemplate> entityTemplates)
		{
			this.hud = hud;

			name = Evaluate(name, entityTemplates) ?? name;
			image = Evaluate(image, entityTemplates) ?? image;
			hotkey = Evaluate(hotkey, entityTemplates) ?? hotkey;

			if(onClickData != null)
			{
				for (int index = 0; index < onClickData.Parameters.Count; index++)
				{
					String s = onClickData.Parameters[index] as String;
					if (s != null)
					{
						onClickData.Parameters[index] = Evaluate(s, entityTemplates) ?? s;
					}
				}
			}


			// Set up a JS callback to catch when this button is clicked
			String clickMethodName = String.Format(CultureInfo.InvariantCulture, "{0}{1}Click", pageName, name).Replace(" ", "");
			callbackJS = String.Format(CultureInfo.InvariantCulture, "{1}.{0}()", clickMethodName, jsContextMenu.GlobalObjectName);
			jsContextMenu.Bind(clickMethodName, false, OnClick);

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
			if(fullField == null){ return null; }

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
	}


	internal class Call
	{
		public String Method;
		public List<object> Parameters;


		public void Invoke(AOHUD hud)
		{
			String[] methodParts = Method.Split(new char[]{ '.' }, 2);

			if (methodParts[0] == "hud" || methodParts.Length == 1)
			{
				String methodName = (methodParts.Length == 1) ? methodParts[0] : methodParts[1];

				hud.GetType().InvokeMember(methodName,
				                           BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static,
				                           null,
				                           hud,
				                           Parameters.ToArray());

			}
			else
			{
				Console.WriteLine("I don't know how to call the method requested");
				Debugger.Break();
			}
		}
	}
}
