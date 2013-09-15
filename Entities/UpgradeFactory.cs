using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using AsteroidOutpost.Components;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AsteroidOutpost.Entities
{
	class UpgradeFactory
	{
		public Dictionary<String, UpgradeTemplate> Upgrades = new Dictionary<String, UpgradeTemplate>();

		public void LoadUpgradeTemplates()
		{
			foreach(var templateFileName in Directory.EnumerateFiles(@"..\data\upgrades\", "*.json"))
			{
				String json = File.ReadAllText(templateFileName);

				UpgradeTemplate template = new UpgradeTemplate();
				JsonConvert.PopulateObject(json, template);
				Upgrades.Add(template.Name.ToLowerInvariant(), template);
			}
		}


		public void ApplyOnStartPayload(World world, int entityID, String upgradeName)
		{
			UpgradeTemplate upgrade = Upgrades[upgradeName.ToLowerInvariant()];
			ApplyPayload(world, entityID, upgrade.OnStartPayload);

			Upgrading upgrading = world.GetComponent<Upgrading>(entityID);
			upgrading.UpgradeName = upgradeName;
		}

		public void ApplyOnCompletePayload(World world, int entityID)
		{
			Upgrading upgrading = world.GetComponent<Upgrading>(entityID);
			UpgradeTemplate upgrade = Upgrades[upgrading.UpgradeName.ToLowerInvariant()];
			ApplyPayload(world, entityID, upgrade.OnCompletePayload);
		}


		private void ApplyPayload(World world, int entityID, Dictionary<String, JObject> payload)
		{
			//UpgradeTemplate upgrade = Upgrades[upgradeName.ToLowerInvariant()];

			foreach (var componentName in payload.Keys)
			{
				Type componentType = Type.GetType("AsteroidOutpost.Components." + componentName, false, true);
				if (componentType != null)
				{
					Component component = world.GetComponent(entityID, componentType);
					if(component != null)
					{
						// Update the existing component
						JsonConvert.PopulateObject(payload[componentName].ToString(), component);
					}
					else
					{
						// Create, and add a new component
						component = Activator.CreateInstance(componentType, entityID) as Component;
						if (component != null)
						{
							JsonConvert.PopulateObject(payload[componentName].ToString(), component);
						}
						world.AddComponent(component);
					}
				}
				else
				{
					Console.WriteLine("Unrecognized component type '{0}' while Applying Payload '{1}'", componentName, GetType());
					Debugger.Break();
				}
			}
		}
	}
}
