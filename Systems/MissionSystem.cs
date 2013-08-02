using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteroidOutpost.Scenarios;
using AsteroidOutpost.Screens;
using AwesomiumXNA;
using Microsoft.Xna.Framework;
using System.Globalization;

namespace AsteroidOutpost.Systems
{
	public class MissionSystem : GameComponent
	{
		private World world;
		private Scenario scenario;
		private TimeSpan updateFrequency = TimeSpan.FromSeconds(0.25);
		private TimeSpan updateAccumulator = TimeSpan.Zero;

		public MissionSystem(AOGame game, World world, Scenario scenario) : base(game)
		{
			this.world = world;
			this.scenario = scenario;
		}

		public override void Update(GameTime gameTime)
		{
			updateAccumulator += gameTime.ElapsedGameTime;		// Accumulate regardless of paused state
			if (world.Paused) { return; }

			if(updateAccumulator > updateFrequency)
			{
				foreach (var mission in scenario.Missions.Where(m => m.Dirty))
				{
				world.ExecuteAwesomiumJS(String.Format(CultureInfo.InvariantCulture, "window.scopeOf('MissionController').AddMission('{0}', '{1}', {2});", mission.Key, mission.Description, mission.Done.ToString().ToLower()));
				//awesomium.WebView.ExecuteJavascript(String.Format(CultureInfo.InvariantCulture, "AddMission('{0}', '{1}', '{2}');", mission.Key, mission.Description, mission.Done));
					mission.Dirty = false;
				}

				updateAccumulator = TimeSpan.Zero;
			}

			base.Update(gameTime);
		}
	}
}
