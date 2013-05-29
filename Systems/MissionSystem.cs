using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteroidOutpost.Scenarios;
using AsteroidOutpost.Screens;
using AwesomiumXNA;
using Microsoft.Xna.Framework;

namespace AsteroidOutpost.Systems
{
	public class MissionSystem : GameComponent
	{
		private World world;
		private Scenario scenario;

		public MissionSystem(AOGame game, World world, Scenario scenario) : base(game)
		{
			this.world = world;
			this.scenario = scenario;
		}

		public override void Update(GameTime gameTime)
		{
			foreach (var mission in scenario.Missions.Where(m => m.Dirty))
			{
				world.ExecuteAwesomiumJS(String.Format("window.scopeOf('MissionController').AddMission('{0}', '{1}', '{2}');", mission.Key, mission.Description, mission.Done));
				//awesomium.WebView.ExecuteJavascript(String.Format("AddMission('{0}', '{1}', '{2}');", mission.Key, mission.Description, mission.Done));
				mission.Dirty = false;
			}

			base.Update(gameTime);
		}
	}
}
