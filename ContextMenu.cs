using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using AsteroidOutpost.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AsteroidOutpost
{
	internal class ContextMenu
	{
		private readonly World world;
		Dictionary<String, ContextMenuPage> contextPages = new Dictionary<String, ContextMenuPage>();
		private ContextMenuPage currentPage;

		public ContextMenu(World world, Dictionary<String, EntityTemplate> entityTemplates)
		{
			this.world = world; 

			foreach(var contextFileName in Directory.EnumerateFiles(@"..\data\context\", "*.json"))
			{
				String json = File.ReadAllText(contextFileName);
				JObject jObject = JObject.Parse(json);

				ContextMenuPage page = new ContextMenuPage();
				JsonConvert.PopulateObject(jObject.ToString(), page);
				contextPages.Add(page.Name.ToLowerInvariant(), page);

				foreach (var contextButton in page.ContextButtons)
				{
					contextButton.Initialize(entityTemplates);
				}
			}

			currentPage = contextPages["main"];

			SetContextButtons();
		}


		protected void SetContextButtons()
		{
			String json = JObject.FromObject(currentPage).ToString(Formatting.None);
			world.ExecuteAwesomiumJS(String.Format(CultureInfo.InvariantCulture, "SetContextPage('{0}');", json));
		}

	}
}
