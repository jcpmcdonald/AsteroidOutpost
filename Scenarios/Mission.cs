using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsteroidOutpost.Scenarios
{
	public class Mission
	{
		private String key;
		private String description;
		private bool done;

		private bool isNew = true;
		private bool dirty = true;

		public Mission(string key, string description, bool done)
		{
			this.key = key;
			this.description = description;
			this.done = done;
			dirty = true;
		}


		public string Key
		{
			get
			{
				return key;
			}
			set
			{
				key = value;
				dirty = true;
			}
		}

		public string Description
		{
			get
			{
				return description;
			}
			set
			{
				if(value != description)
				{
					description = value;
					dirty = true;
				}
			}
		}

		public bool Done
		{
			get
			{
				return done;
			}
			set
			{
				done = value;
				dirty = true;
			}
		}

		public bool New
		{
			get
			{
				return isNew;
			}
			set
			{
				isNew = value;
			}
		}

		public bool Dirty
		{
			get
			{
				return dirty;
			}
			set
			{
				dirty = value;
			}
		}
	}
}
