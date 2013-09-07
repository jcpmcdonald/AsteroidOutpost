using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteroidOutpost.Entities;

namespace AsteroidOutpost
{
	internal class ContextMenuPage
	{
		public String Name { get; set; }
		public List<ContextButton> ContextButtons = new List<ContextButton>(12);


		public void AddButton(ContextButton button)
		{
			ContextButtons.Add(button);
		}


		public String GetJSON()
		{
			String json = "[";
			foreach (var button in ContextButtons.OrderBy(b => b.Slot))
			{
				json += button.GetJSON() + ",";
			}
			json += "]";
			return json;
		}
	}
}
