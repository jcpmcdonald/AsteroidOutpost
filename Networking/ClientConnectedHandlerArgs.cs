using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace AsteroidOutpost.Networking
{
	public class ClientConnectedHandlerArgs
	{
		public ClientConnectedHandlerArgs(TcpClient connectedClient)
		{
			ConnectedClient = connectedClient;
		}


		public TcpClient ConnectedClient { get; private set; }
	}
}
