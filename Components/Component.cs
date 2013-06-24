using System;
using System.IO;
using System.Xml.Serialization;
using AsteroidOutpost.Entities.Eventing;
using AsteroidOutpost.Eventing;
using AsteroidOutpost.Interfaces;
using AsteroidOutpost.Networking;
using AsteroidOutpost.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace AsteroidOutpost.Components
{
	public class Component : IIdentifiable
	{
		private String componentClassName;


		public Component(int entityID)
		{
			GUID = Guid.NewGuid();
			this.EntityID = entityID;

			// Reflectively look up the class name of this component
			componentClassName = GetType().ToString();
			componentClassName = componentClassName.Substring(componentClassName.LastIndexOf('.') + 1);
		}


		/// <summary>
		/// Gets the Entity's ID
		/// </summary>
		[XmlIgnore]
		[JsonIgnore]
		public int EntityID { get; set; }
		

		/// <summary>
		/// This ID will uniquely identify this object in the game
		/// </summary>
		public Guid GUID { get; set; }


		public String GetComponentClassName()
		{
			return componentClassName;
		}
	}
}
