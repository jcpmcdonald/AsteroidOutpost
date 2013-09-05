using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteroidOutpost.Entities;

namespace AsteroidOutpost
{
	internal class ContextMenuPage
	{
		List<ContextButton> contextButtons = new List<ContextButton>(12);

		public void AddButton(ContextButton button)
		{
			contextButtons.Add(button);
		}


		public String GetJSON()
		{
			String json = "[";
			foreach (var button in contextButtons.OrderBy(b => b.Slot))
			{
				json += button.GetJSON() + ",";
			}
			json += "]";
			return json;
		}
	}
}
