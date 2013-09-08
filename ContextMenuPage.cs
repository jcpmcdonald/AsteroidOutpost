using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using AsteroidOutpost.Entities;
using AsteroidOutpost.Systems;
using Awesomium.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AsteroidOutpost
{
	internal class ContextMenuPage
	{
		public event Action<ContextMenuPage> PageChanged;

		private string name;

		// TODO: I'd really like this variable to be private, but then the serialization doesn't include this
		public List<ContextButton> ContextButtons = new List<ContextButton>(12);

		[JsonIgnore]
		public Dictionary<String, ContextButton> ContextButtonDictionary = new Dictionary<String, ContextButton>(12);


		public String Name
		{
			get { return name; }
			set
			{
				name = value;
				OnPageChanged();
			}
		}


		private void OnPageChanged(ContextButton button)
		{
			OnPageChanged();
		}

		private void OnPageChanged()
		{
			if(PageChanged != null)
			{
				PageChanged(this);
			}
		}


		public void AddButton(ContextButton button)
		{
			ContextButtons.Add(button);
			ContextButtonDictionary.Add(button.Name.ToLowerInvariant(), button);
			button.ButtonChanged += OnPageChanged;
			OnPageChanged();
		}

		public void Apply(World world)
		{
			String json = JObject.FromObject(this).ToString(Formatting.None);
			//Console.WriteLine(json);
			world.ExecuteAwesomiumJS(String.Format(CultureInfo.InvariantCulture, "SetContextPage('{0}');", json.Replace("'", "\\'")));
		}


		public void Initialize(AOHUD hud, JSObject jsContextMenu, Dictionary<string, EntityTemplate> entityTemplates)
		{
			foreach (var button in ContextButtons)
			{
				button.Initialize(name, hud, jsContextMenu, entityTemplates);
				ContextButtonDictionary.Add(button.Name.ToLowerInvariant(), button);
				button.ButtonChanged += OnPageChanged;
			}
		}
	}
}
