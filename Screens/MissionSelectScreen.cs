using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteroidOutpost.Scenarios;
using C3.XNA;
using C3.XNA.Events;

namespace AsteroidOutpost.Screens
{
	partial class MissionSelectScreen : AOMenuScreen
	{
		private World world;

		public MissionSelectScreen(World world, ScreenManager theScreenManager, LayeredStarField starField)
			: base(theScreenManager, starField)
		{
			this.world = world;
		}



		void btnEndless_Click(object sender, MouseButtonEventArgs e)
		{
			ScreenMan.SwitchScreens("Game");
			world.StartServer(new RandomScenario(world, 1));
		}

		void btnTutorial_Click(object sender, MouseButtonEventArgs e)
		{
			ScreenMan.SwitchScreens("Game");
			world.StartServer(new TutorialScenario(world, 1));
		}

		void btnBack_Click(object sender, MouseButtonEventArgs e)
		{
			ScreenMan.SwitchScreens("Main Menu");
		}
	}
}
