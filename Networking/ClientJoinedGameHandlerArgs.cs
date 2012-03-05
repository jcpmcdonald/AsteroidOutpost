using System;

namespace AsteroidOutpost.Networking
{
	public delegate void ClientJoinedGameHandler(object sender, ClientJoinedGameHandlerArgs args);

	public class ClientJoinedGameHandlerArgs : ReflectiveEventArgs
	{
		public ClientJoinedGameHandlerArgs(String playerName)
			: base(AONetwork.SpecialTargetNetworkingClass, "OnClientJoinedGame", new object[] { playerName })
		{
			PlayerName = playerName;
		}


		public String PlayerName { get; private set; }
	}
}