using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using AsteroidOutpost.Entities;

namespace AsteroidOutpost
{
	internal class ContextMenu
	{
		private readonly World world;
		Dictionary<String, ContextMenuPage> contextPages = new Dictionary<String, ContextMenuPage>();
		private ContextMenuPage currentPage;

		public ContextMenu(World world)
		{
			this.world = world;
			ContextMenuPage mainPage = new ContextMenuPage();

			var contextButtonPool = EntityFactory.GetContextButtons();
			foreach (var contextButton in contextButtonPool.Values)
			{
				mainPage.AddButton(contextButton);
			}
			
			contextPages.Add("main", mainPage);
			currentPage = mainPage;

			SetContextButtons();
		}


		protected void SetContextButtons()
		{
			world.ExecuteAwesomiumJS(String.Format(CultureInfo.InvariantCulture, "SetContextButtons('{0}');", currentPage.GetJSON()));
		}

	}
}
