using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using AsteroidOutpost.Scenarios;

namespace AsteroidOutpost
{
	public class Profile
	{
		private Dictionary<Type, Scenario> availableScenarios = new Dictionary<Type, Scenario>();

		public Profile()
		{
			availableScenarios.Add(typeof(TutorialScenario), new TutorialScenario());
			availableScenarios.Add(typeof(RandomScenario), new RandomScenario());
		}

		public void ScenarioCompleted(Scenario completedScenario)
		{
			if(completedScenario is TutorialScenario)
			{
				UnlockScenario(new MinerealCollectionScenario());
			}
			else if(completedScenario is MinerealCollectionScenario)
			{
				UnlockScenario(new RandomScenario());
			}
		}

		private void UnlockScenario(Scenario scenario)
		{
			if(!availableScenarios.ContainsKey(scenario.GetType()))
			{
				availableScenarios.Add(scenario.GetType(), scenario);
			}
		}

		public IEnumerable<String> GetAvailableScenarios()
		{
			return availableScenarios.Select(x => x.Value.Name);
		}
	}
}
