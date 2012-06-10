using AsteroidOutpost.Networking;
using C3.XNA;

namespace AsteroidOutpost.Screens
{
	public partial class ServerHostScreen : AOMenuScreen
	{
		private World world;

		public ServerHostScreen(World world, ScreenManager theScreenManager, LayeredStarField starField)
			: base(theScreenManager, starField)
		{
			this.world = world;
		}


		void btnStartHost_Click(object sender, C3.XNA.Events.MouseButtonEventArgs e)
		{
			// Start the server information server   (In base 4, I'm FINE!)
			world.Network.ServerName = txtServerName.Text;
			world.IsServer = true;

			ScreenMan.SwitchScreens("Lobby");
		}

		void btnBack_Click(object sender, C3.XNA.Events.MouseButtonEventArgs e)
		{
			ScreenMan.SwitchScreens("Server Browser");
		}

	}
}
