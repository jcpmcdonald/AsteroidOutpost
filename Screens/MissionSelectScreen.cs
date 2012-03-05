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
		private AsteroidOutpostScreen theGame;

		public MissionSelectScreen(AsteroidOutpostScreen theGame, ScreenManager theScreenManager, LayeredStarField starField)
			: base(theScreenManager, starField)
		{
			this.theGame = theGame;
		}



		void btnEndless_Click(object sender, MouseButtonEventArgs e)
		{
			ScreenMan.SwitchScreens("Game");
			theGame.StartServer(new RandomScenario(theGame));
		}

		void btnTutorial_Click(object sender, MouseButtonEventArgs e)
		{
			ScreenMan.SwitchScreens("Game");
			theGame.StartServer(new TutorialScenario(theGame));
		}

		void btnBack_Click(object sender, MouseButtonEventArgs e)
		{
			ScreenMan.SwitchScreens("Main Menu");
		}
	}
}
