using AsteroidOutpost.Networking;
using C3.XNA;

namespace AsteroidOutpost.Screens
{
	public partial class ServerHostScreen : AOMenuScreen
	{
		private AsteroidOutpostScreen theGame;

		public ServerHostScreen(AsteroidOutpostScreen theGame, ScreenManager theScreenManager, LayeredStarField starField)
			: base(theScreenManager, starField)
		{
			this.theGame = theGame;
		}


		void btnStartHost_Click(object sender, C3.XNA.Events.MouseButtonEventArgs e)
		{
			// Start the server information server   (In base 4, I'm FINE!)
			theGame.Network.ServerName = txtServerName.Text;
			theGame.IsServer = true;

			ScreenMan.SwitchScreens("Lobby");
		}

		void btnBack_Click(object sender, C3.XNA.Events.MouseButtonEventArgs e)
		{
			ScreenMan.SwitchScreens("Server Browser");
		}

	}
}
