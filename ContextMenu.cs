using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using AsteroidOutpost.Entities;
using AsteroidOutpost.Systems;
using Awesomium.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AsteroidOutpost
{
	internal class ContextMenu
	{
		private readonly World world;

		public Dictionary<String, ContextMenuPage> ContextPages = new Dictionary<String, ContextMenuPage>();
		private ContextMenuPage currentPage;

		public ContextMenu(AOGame game, World world, AOHUD hud, Dictionary<String, EntityTemplate> entityTemplates, Dictionary<String, UpgradeTemplate> upgradeTemplates)
		{
			this.world = world; 

			JSObject jsContextMenu = game.Awesomium.WebView.CreateGlobalJavascriptObject("contextMenu");

			foreach(var contextFileName in Directory.EnumerateFiles(@"..\data\context\", "*.json"))
			{
				String json = File.ReadAllText(contextFileName);

				ContextMenuPage page = new ContextMenuPage();
				JsonConvert.PopulateObject(json, page);
				ContextPages.Add(page.Name.ToLowerInvariant(), page);

				page.Initialize(hud, jsContextMenu, entityTemplates, upgradeTemplates);
			}

			SetPage("main");
		}


		public void SetPage(String pageName)
		{
			pageName = pageName.ToLowerInvariant();
			if(ContextPages.ContainsKey(pageName))
			{
				if(currentPage != null)
				{
					currentPage.PageChanged -= CurrentPageOnPageChanged;
				}
				currentPage = ContextPages[pageName];
				ApplyContextPage(world);
				currentPage.PageChanged += CurrentPageOnPageChanged;
			}
			else
			{
				Console.WriteLine("The page name '{0}' does not exist, the page will not be flipped", pageName);
				Debugger.Break();
			}
		}


		private void CurrentPageOnPageChanged(ContextMenuPage contextMenuPage)
		{
			ApplyContextPage(world);
		}


		protected void ApplyContextPage(World world)
		{
			currentPage.Apply(world);
		}


		public void HandleHotKeys(EnhancedKeyboardState keyboard)
		{
			currentPage.HandleHotKeys(keyboard);
		}

	}
}
