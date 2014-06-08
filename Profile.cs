using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using AsteroidOutpost.Scenarios;
using Newtonsoft.Json;

namespace AsteroidOutpost
{
	public class Profile
	{
		public List<ScenarioSelector> progress;


		public Profile()
		{
			progress = new List<ScenarioSelector>{
				new ScenarioSelector(){
					scenario = new TutorialScenario(),
					complete = false,
					image = "MarsSmall.png",
					top = 100,
					left = 100,
					width = 70,
					height = 70,
				},
				new ScenarioSelector(){
					scenario = new MinerealCollectionScenario(),
					complete = false,
					image = "IcePlanetSmall.png",
					top = 250,
					left = 300,
					width = 150,
					height = 150,
				},
				new ScenarioSelector(){
					scenario = new SuperStructureProtectScenario(),
					complete = false,
					image = "EarthSmall.png",
					top = 120,
					left = 470,
					width = 90,
					height = 90,
				},
				new ScenarioSelector(){
					scenario = new SuperStructureProtectScenario(),
					complete = false,
					image = "EarthSmall.png",
					top = 200,
					left = 670,
					width = 90,
					height = 90,
				},
				new ScenarioSelector(){
					scenario = new SuperStructureProtectScenario(),
					complete = false,
					image = "EarthSmall.png",
					top = 420,
					left = 570,
					width = 90,
					height = 90,
				},
			};
		}


		public void ScenarioCompleted(Scenario completedScenario)
		{
			
		}

		//private Dictionary<Type, Scenario> availableScenarios = new Dictionary<Type, Scenario>();

		//public Profile()
		//{
		//	availableScenarios.Add(typeof(TutorialScenario), new TutorialScenario());
		//	availableScenarios.Add(typeof(RandomScenario), new RandomScenario());
		//	availableScenarios.Add(typeof(SuperStructureProtectScenario), new SuperStructureProtectScenario());
		//}

		//public void ScenarioCompleted(Scenario completedScenario)
		//{
		//	if(completedScenario is TutorialScenario)
		//	{
		//		UnlockScenario(new MinerealCollectionScenario());
		//	}
		//	else if(completedScenario is MinerealCollectionScenario)
		//	{
		//		UnlockScenario(new RandomScenario());
		//	}
		//}

		//private void UnlockScenario(Scenario scenario)
		//{
		//	if(!availableScenarios.ContainsKey(scenario.GetType()))
		//	{
		//		availableScenarios.Add(scenario.GetType(), scenario);
		//	}
		//}

		//public IEnumerable<String> GetAvailableScenarios()
		//{
		//	return availableScenarios.Select(x => x.Value.Name);
		//}
	}

	public class ScenarioSelector
	{
		[JsonIgnore][XmlIgnore]
		public Scenario scenario;

		public String scenarioName { get { return scenario.Name; } }
		public String description { get { return "TODO"; } }
		public bool complete;

		public String image;
		public int top;
		public int left;
		public int width;
		public int height;
	}
}
