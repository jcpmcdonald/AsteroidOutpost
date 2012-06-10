using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using AsteroidOutpost.Networking;
using C3.XNA;
using C3.XNA.Controls;

namespace AsteroidOutpost.Screens
{
	partial class ServerBrowserScreen : AOMenuScreen, IDisposable
	{
		private World world;

		private Thread refreshThread;
		private Thread serverReplyListenerThread;
		private bool listenForServerReplies;
		private List<IListRow> newServers = new List<IListRow>();
		private Dictionary<string, Stopwatch> serverUpdateRequestsSent = new Dictionary<string, Stopwatch>();
		private UdpClient udpClient;



		public ServerBrowserScreen(World world, ScreenManager theScreenManager, LayeredStarField starField)
			: base(theScreenManager, starField)
		{
			this.world = world;
		}



		void btnMainMenu_Click(object sender, C3.XNA.Events.MouseButtonEventArgs e)
		{
			ScreenMan.SwitchScreens("Main Menu");
		}


		void btnRefresh_Click(object sender, C3.XNA.Events.MouseButtonEventArgs e)
		{
			lstServers.ClearListItems();
			listBox_SelectionChanged(this, EventArgs.Empty);

			refreshThread = new Thread(refreshServerList);
			refreshThread.Start();

			// Disable the button until the refresh is complete
			btnRefresh.Enabled = false;
		}


		void listBox_SelectionChanged(object sender, EventArgs e)
		{
			// Only allow them to click "Connect" if there is a server selected
			if(lstServers.GetSelectedRow() == null)
			{
				btnConnect.Enabled = false;
			}
			else
			{
				btnConnect.Enabled = true;
			}
		}


		void btnConnect_Clicked(object sender, EventArgs e)
		{
			IListRow selectedServer = lstServers.GetSelectedRow();
			if (selectedServer == null)
			{
				return;
			}

			IPEndPoint remoteEndpoint = selectedServer.Tag as IPEndPoint;
			if(remoteEndpoint != null)
			{
				bool success = world.Network.Connect(remoteEndpoint.Address, remoteEndpoint.Port);
				if(success)
				{
					SimpleListRow simpleListRow = selectedServer as SimpleListRow;
					if(simpleListRow != null)
					{
						world.Network.ServerName = simpleListRow.Cells[0];
					}
					ScreenMan.SwitchScreens("Lobby");
				}
			}
		}


		void btnHost_Clicked(object sender, EventArgs e)
		{
			ScreenMan.SwitchScreens("Server Host");
		}


		/// <summary>
		/// Updates this screen
		/// </summary>
		/// <param name="deltaTime">The amount of time that has passed since the last update</param>
		/// <param name="theMouse">The current state of the mouse</param>
		/// <param name="theKeyboard">The current state of the keyboard</param>
		public override void Update(TimeSpan deltaTime, EnhancedMouseState theMouse, EnhancedKeyboardState theKeyboard)
		{
			if (newServers.Count > 0)
			{
				lock(newServers)
				{
					foreach(IListRow newRow in newServers)
					{
						lstServers.AddListItem(newRow);
					}
					newServers.Clear();
				}
			}

			base.Update(deltaTime, theMouse, theKeyboard);
		}


		/// <summary>
		/// Called when this screen is first being transitioned toward
		/// </summary>
		/// <param name="deltaTime">The amount of time that has passed since the last update</param>
		/// <param name="theMouse">The current state of the mouse</param>
		/// <param name="theKeyboard">The current state of the keyboard</param>
		protected override void StartTransitionToward(TimeSpan deltaTime, EnhancedMouseState theMouse, EnhancedKeyboardState theKeyboard)
		{
			world.IsServer = false;
			base.StartTransitionToward(deltaTime, theMouse, theKeyboard);
		}


		/// <summary>
		/// Called when this screen is first being transitioned toward
		/// </summary>
		/// <param name="deltaTime">The amount of time that has passed since the last update</param>
		/// <param name="theMouse">The current state of the mouse</param>
		/// <param name="theKeyboard">The current state of the keyboard</param>
		protected override void StartTransitionAway(TimeSpan deltaTime, EnhancedMouseState theMouse, EnhancedKeyboardState theKeyboard)
		{
			if (serverReplyListenerThread != null)
			{
				listenForServerReplies = false;
				serverReplyListenerThread.Abort();
				serverReplyListenerThread = null;
			}
			base.StartTransitionAway(deltaTime, theMouse, theKeyboard);
		}


		private void refreshServerList()
		{
			if(udpClient == null)
			{
				// Initialize the UDP client if it's not already
				IPEndPoint localIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
				udpClient = new UdpClient(localIpEndPoint);
			}

			if(serverReplyListenerThread == null)
			{
				// Start the reply listener if it's not already running
				listenForServerReplies = true;
				serverReplyListenerThread = new Thread(serverReplyListener);
				serverReplyListenerThread.Start();
			}

			List<Tuple<string, int>> serverIPs = world.Network.GetServerIPs();

			// Locked the shared variable that is used to communicate the new servers to the main thread
			lock (serverUpdateRequestsSent)
			{
				foreach (Tuple<string, int> serverIP in serverIPs)
				{
					IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Parse(serverIP.Item1), 18189);
					Stopwatch pingTimer = new Stopwatch();

					MemoryStream memStream = new MemoryStream();
					BinaryWriter bw = new BinaryWriter(memStream);
					bw.Write(World.StreamIdent);
					bw.Write(World.Version);
					bw.Write((byte)StreamType.RequestServerInfo);
					bw.Flush();


					udpClient.Send(memStream.GetBuffer(), (int)memStream.Length, remoteIpEndPoint);
					pingTimer.Start();

					String key = remoteIpEndPoint.Address + ":" + remoteIpEndPoint.Port;
					if (serverUpdateRequestsSent.ContainsKey(key))
					{
						serverUpdateRequestsSent.Remove(key);
					}
					serverUpdateRequestsSent.Add(key, pingTimer);
				}
			}

			btnRefresh.Enabled = true;
		}


		private void serverReplyListener()
		{
			// This is required as a reference parameter to receive UDP packets. The parameters here are irrelevant 
			IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

			while (listenForServerReplies)
			{
				try
				{
					while (udpClient.Available == 0 && listenForServerReplies)
					{
						Thread.Sleep(0);
					}

					if (listenForServerReplies)
					{
						// Use the alphabet to avoid the Dining Philosophers Problem: http://en.wikipedia.org/wiki/Dining_philosophers_problem   One of my personal favourites :)
						lock (newServers)
						{
							lock (serverUpdateRequestsSent)
							{
								byte[] bytes = udpClient.Receive(ref remoteIpEndPoint);

								// You may be thinking "What if the server was inserted with a domain name?", but I've covered that, this is using my match-making service, and I use IPs only
								String key = remoteIpEndPoint.Address + ":" + remoteIpEndPoint.Port;
								if (!serverUpdateRequestsSent.ContainsKey(key))
								{
									// Wtf? Someone sent us a reply... but we didn't ask for it? Garbage!
									Console.WriteLine("Someone sent us a reply but we didn't ask for it, IP = " + remoteIpEndPoint.Address);
									continue;
								}

								Stopwatch pingTimer = serverUpdateRequestsSent[key];
								pingTimer.Stop();
								serverUpdateRequestsSent.Remove(key);

								BinaryReader br = new BinaryReader(new MemoryStream(bytes));

								UInt32 handshake = br.ReadUInt32();
								if(handshake != World.StreamIdent)
								{
									Debugger.Break();
								}

								UInt32 version = br.ReadUInt32();
								StreamType streamType = (StreamType)br.ReadByte();

								if (streamType == StreamType.ServerInfo)
								{
									String serverName = br.ReadString();
									int currentPlayers = br.ReadInt32();
									int maxPlayers = br.ReadInt32();

									SimpleListRow newServer = new SimpleListRow(new String[]{serverName, "", currentPlayers + "/" + maxPlayers, pingTimer.ElapsedMilliseconds + "ms"});
									// TODO: Attach something to the server list row so we can update it if I use "Refresh" vs "Update"
									newServer.Tag = remoteIpEndPoint;
									newServers.Add(newServer);
								}
							}
						}
					}

				}
				catch (ThreadAbortException)
				{
				}
			}
		}


		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <filterpriority>2</filterpriority>
		public void Dispose()
		{
			if (serverReplyListenerThread != null)
			{
				listenForServerReplies = false;
				serverReplyListenerThread.Abort();
				serverReplyListenerThread = null;
			}
		}
	}
}
