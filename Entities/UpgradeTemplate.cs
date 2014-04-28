using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace AsteroidOutpost.Entities
{
	public class UpgradeTemplate
	{
		public String Name { get; set; }
		public Dictionary<String, JObject> OnStartPayload { get; set; }
		public Dictionary<String, JObject> OnCompletePayload { get; set; }
	}
}
