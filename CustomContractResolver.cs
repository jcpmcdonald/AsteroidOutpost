using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace AsteroidOutpost
{
	/// <summary>
	/// json.net serializes ALL properties of a class by default
	/// this class will tell json.net to only serialize properties if they MATCH 
	/// the list of valid columns passed through the querystring to criteria object
	/// </summary>
	public class CustomContractResolver : DefaultContractResolver
	{
		protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
		{
			IList<JsonProperty> filtered = base.CreateProperties(type, memberSerialization);

			filtered = filtered.Where(p => p.PropertyType != typeof(GraphicsDevice) && p.PropertyType != typeof(Game)).ToList();

			return filtered;
		}
	}
}