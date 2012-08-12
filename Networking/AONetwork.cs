using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Xml.Linq;
using AsteroidOutpost.Entities;
using AsteroidOutpost.Screens;
using Microsoft.Xna.Framework;

namespace AsteroidOutpost.Networking
{

	enum GameState
	{
		NONE,
		LOBBY,
		IN_GAME
	}

	internal class AONetwork : Network, IReflectionTarget
	{
		private World world;

		public const int SpecialTargetNetworkingClass = -1;
		public const int SpecialTargetTheGame = -2;

		private GameState gameState = GameState.NONE;

		private readonly Queue<AOIncomingMessage> incomingMessageQueue = new Queue<AOIncomingMessage>(64);
		private readonly Queue<AOOutgoingMessage> outgoingMessageQueue = new Queue<AOOutgoingMessage>(64);
		private List<TcpClient> newClients = new List<TcpClient>();


		// TODO: Store this list of matchmakers in a file, and get them here
		String[] matchmakers = new String[] { "http://cell.stat-life.com/matchmaker/" };
		private Thread serverBeacon;
		private bool serverBeaconActive;

		private Thread serverInfo;
		private bool serverInfoActive;
		private String serverName = "My Server!";
		private int serverPlayerCount;
		private int serverPlayerCapacity = 2;

		
#if DEBUG
		private int packetsCounter = 0;
		private DateTime lastPacketCountPrint = DateTime.Now;
#endif


		[EventReplication(EventReplication.ServerToClients)]
		public event ClientJoinedGameHandler ClientJoinedGame;


		public AONetwork(World world)
		{
			this.world = world;
		}


		public override void Dispose()
		{
			StopServerBeacon();
			StopServerInfoServer();
			base.Dispose();
		}


		/// <summary>
		/// Gets the ID for this network target
		/// </summary>
		public int EntityID
		{
			get
			{
				return SpecialTargetNetworkingClass;
			}
		}

		
		/// <summary>
		/// Gets or Sets the name of the server
		/// </summary>
		public String ServerName
		{
			get
			{
				return serverName;
			}
			set
			{
				if (value == null || value.Trim() == "")
				{
					serverName = "My Server";
				}
				else
				{
					serverName = value;
				}
			}
		}


		/// <summary>
		/// Starts listening on the given port (typically servers only)
		/// </summary>
		/// <param name="port">The port to listen on</param>
		public override void StartListening(int port)
		{
			gameState = GameState.LOBBY;
			serverPlayerCount = 1;

			ListenToEvents(this);

			// We'll join our own server
			OnClientJoinedGame("Player" + serverPlayerCount);

			base.StartListening(port);
		}



		/// <summary>
		/// Called when a client connects
		/// </summary>
		/// <param name="client">The new client</param>
		protected override void OnClientConnected(TcpClient client)
		{
			if (gameState == GameState.LOBBY)
			{
				lock (newClients)
				{
					newClients.Add(client);
				}
			}
			else
			{
				// TODO: reject the client, we aren't in the lobby anymore
			}

			base.OnClientConnected(client);
		}



		/// <summary>
		/// Called when a client joins the game (or lobby)
		/// </summary>
		/// <param name="name">The player's name</param>
		public virtual void OnClientJoinedGame(String name)
		{
			if(ClientJoinedGame != null)
			{
				ClientJoinedGame(this, new ClientJoinedGameHandlerArgs(name));
			}
		}


		/// <summary>
		/// Connect to a server at the given address on the given port
		/// </summary>
		/// <param name="address">The server's address</param>
		/// <param name="port">The port to connect on</param>
		/// <returns>True if we managed to connect, False otherwise</returns>
		public bool Connect(IPAddress address, int port)
		{
			try
			{
				base.connect(address, port);
				gameState = GameState.LOBBY;
			}
			catch (Exception ex)
			{
				Console.WriteLine("Failed to connect: " + ex.Message);
				return false;
			}

			return true;
		}



		/*
		// TODO: Limit the visibility of this function because it's really only used internally. The problems with doing that are:
		//		1) This will get called from an Incoming Message
		//		2) Reflection needs to be able to look this up
		/// <summary>
		/// Receives all of the entities in binary-form from the server
		/// </summary>
		/// <param name="entityCount">The number of entities to expect</param>
		/// <param name="entityBytes">The bytes that represent all the entities in the game </param>
		public void ReceiveAllEntityStates(int entityCount, byte[] entityBytes)
		{
			gameState = GameState.IN_GAME;

			BinaryReader br = new BinaryReader(new MemoryStream(entityBytes));


			// Sync the entire game state
			for (int i = 0; i < entityCount; i++)
			{
				String assemName = br.ReadString();

				// Use reflection to make a new entity of... whatever type was sent to us
				Type t = Type.GetType(assemName);
				ConstructorInfo entityConstructor = t.GetConstructor(new Type[]{typeof(BinaryReader)});

				Object obj = entityConstructor.Invoke(new object[] { br });
				if(obj is ISerializable)
				{
					((ISerializable)obj).PostDeserializeLink(world);
				}

				if(obj is Entity)
				{
					world.Add((Entity)obj, true);
				}
				else
				{
					throw new Exception("Unhandled object type was received in a whole game sync");
				}
				
			}
		}
		*/


		/// <summary>
		/// Send the current game state to the client, and create a new actor and force for them
		/// </summary>
		/// <param name="client">The client to send the game state to</param>
		private void SendGameStateTo(TcpClient client)
		{
			BinaryWriter clientWriter = beginSend(client);
			SendGameStateTo(clientWriter);
			endSend();
		}

		private void SendGameStateTo(BinaryWriter clientWriter)
		{
			if (!world.IsServer)
			{
				Console.WriteLine("Client is trying to SEND the game state");
				Debugger.Break();
				return;
			}

			//BinaryWriter clientWriter = beginSend(client);
			try
			{
				MemoryStream memStream = new MemoryStream(4096);
				BinaryWriter bw = new BinaryWriter(memStream);

				world.Serialize(bw, false);

				// TODO: Don't hard-code the team here, and make sure there's exactly 1 result. Better yet, associate the force with the client
				Force clientForce = world.GetForcesOnTeam(Team.Team2)[0];
				Controller clientController = new Controller(world, ControllerRole.Remote, clientForce);
				Controller aiController = world.Controllers.First(a => a.Role == ControllerRole.AI);
				bw.Write(2);
				clientController.Serialize(bw);
				aiController.Serialize(bw);

				// Add a footer so that we can verify the integrity of this block
				bw.Write(World.StreamIdent);
				bw.Flush();


				new AOReflectiveOutgoingMessage(SpecialTargetTheGame, "Deserialize", new object[] { memStream.GetBuffer() }).Serialize(clientWriter);

				// Don't queue the messages because queued messages are sent to everyone
				//new AOReflectiveOutgoingMessage(SpecialTargetTheGame, "OnClientJoinedGame", new object[] { "Player" + iPlayer }).Serialize(newClient.GetStream());
			}
			catch (Exception ex)
			{
				Console.WriteLine("Failed to send the game state: " + ex.Message);
				Debugger.Break();
			}
			//endSend();
		}


		/// <summary>
		/// Queues and outgoing message. This message will get sent on all connections when the outgoing queue is processed
		/// </summary>
		/// <param name="message">The message to send</param>
		public void EnqueueMessage(AOOutgoingMessage message)
		{
			if(message == null)
			{
				throw new ArgumentNullException("message");
			}
			outgoingMessageQueue.Enqueue(message);
		}



		/// <summary>
		/// Process all of the Outgoing messages that have accumulated since the last time this was called
		/// </summary>
		public void ProcessOutgoingQueue()
		{
			if (newClients.Count > 0)
			{
				lock (newClients)
				{
					if (gameState == GameState.IN_GAME)
					{
						// Really though... this shouldn't happen, should it? Only when someone connects after the game has started
						foreach (TcpClient newClient in newClients)
						{
							SendGameStateTo(newClient); // These clients are no longer new, get rid of them
						}
					}

					foreach (TcpClient newClient in newClients)
					{
						// Send the client a list of all the players in the game so far
						for (int iPlayer = 1; iPlayer <= serverPlayerCount; iPlayer++)
						{
							// Don't queue the messages because queued messages are sent to everyone
							new AOReflectiveOutgoingMessage(SpecialTargetNetworkingClass, "OnClientJoinedGame", new object[]{"Player" + iPlayer}).Serialize(newClient.GetStream());
						}

						// TODO: How on earth do I get their name!?!
						serverPlayerCount++;
						OnClientJoinedGame("Player" + serverPlayerCount);
					}

					newClients.Clear();
				}
			}


#if DEBUG
			packetsCounter += outgoingMessageQueue.Count;
			if (DateTime.Now.Subtract(lastPacketCountPrint).TotalSeconds >= 0.1)
			{
				lastPacketCountPrint = DateTime.Now;
				File.AppendAllText("PacketCounter.txt", DateTime.Now.ToLongTimeString() + "\t" + packetsCounter + "\r\n");
				packetsCounter = 0;
			}
#endif
			

			if (outgoingMessageQueue.Count > 0)
			{
				Dictionary<TcpClient, BinaryWriter> sendStreams = beginSend();

				// No locking required on the message queue, everything alters this queue from the same thread
				AOOutgoingMessage message;
				while (outgoingMessageQueue.Count > 0)
				{
					message = outgoingMessageQueue.Dequeue();
					foreach (BinaryWriter bw in sendStreams.Values)
					{
						try
						{
							message.Serialize(bw);
						}
						catch (Exception ex)
						{
							Console.WriteLine("Failed to write to client!");
							Console.WriteLine(ex.Message);
							//throw;
						}
					}
				}

				endSend();
			}
		}


		/// <summary>
		/// Process all of the Incoming messages that have accumulated since the last time this was called
		/// </summary>
		public void ProcessIncomingQueue(TimeSpan deltaTime)
		{
			// Technically I could lock just the parts where I call Dequeue(), but that's
			// annoying and could cause more overhead than it's worth
			lock (incomingMessageQueue)
			{
				AOIncomingMessage message;
				while (incomingMessageQueue.Count > 0)
				{
					message = incomingMessageQueue.Dequeue();
					message.Execute(world, this, deltaTime);
				}
			}
		}


		/// <summary>
		/// Handles the receiving of a message by creating an Incoming message and adding it to the queue.
		/// This is executed in a secondary thread
		/// </summary>
		/// <param name="stream">The stream that the message is coming in on</param>
		protected override void handleClientMessage(Stream stream)
		{
			// Plain streams are boring
			BinaryReader br = new BinaryReader(stream);

			// Grab some important information from the stream
			int targetObjectId = br.ReadInt32();


			// Make the incoming message
			AOIncomingMessage incomingMessage = new AOReflectiveIncomingMessage(targetObjectId, br);

			// Stash it for later. We are running in a secondary thread right now and shouldn't interrupt
			lock(incomingMessageQueue)
			{
				incomingMessageQueue.Enqueue(incomingMessage);
			}
		}


		// TODO: Find a more appropriate home for this. Is a static method in the Controller appropriate?
		/// <summary>
		/// Creates a controller and adds it to the game
		/// </summary>
		/// <param name="role">The controller's role</param>
		/// <param name="forceID">The controller's primary force's ID</param>
		public void CreateController(int role, int forceID)
		{
			world.AddController(new Controller(world, (ControllerRole)role, world.GetForceByID(forceID)));
		}


		#region Event Hooking

		/// <summary>
		/// Listen to all appropriate events for the given object
		/// </summary>
		/// <param name="obj">The object to listen to</param>
		public void ListenToEvents(Object obj)
		{
			// This is our entity, reflectively register to a bunch of events
			EventInfo[] allEvents = obj.GetType().GetEvents();
			foreach (EventInfo eventInfo in allEvents)
			{
				bool registerThisEvent = false;

				object[] eventAttributes = eventInfo.GetCustomAttributes(typeof(EventReplicationAttribute), false);
				if (eventAttributes.Length == 1)
				{
					EventReplicationAttribute eventReplicationAttribute = (EventReplicationAttribute)eventAttributes[0];

					if (eventReplicationAttribute.ReplicationTarget == EventReplication.ServerToClients && world.IsServer)
					{
						registerThisEvent = true;
					}
					else if (eventReplicationAttribute.ReplicationTarget == EventReplication.ClientToServer && !world.IsServer)
					{
						registerThisEvent = true;
					}
				}

				if (registerThisEvent)
				{
					// Sign me up!

					Type[] delegateParameterTypes = getDelegateParameterTypes(eventInfo.EventHandlerType);

					// See if the network handles this type of event
					MethodInfo networkEventHandler = GetType().GetMethod("EventHandler", delegateParameterTypes);
					if (networkEventHandler != null)
					{
						Delegate networkEventDelegate = Delegate.CreateDelegate(eventInfo.EventHandlerType, this, networkEventHandler);
						eventInfo.AddEventHandler(obj, networkEventDelegate);
					}
					else
					{
						Console.WriteLine("The network can't handle an event! Is this bad? Delegate parameter types: ");
						if (delegateParameterTypes != null)
						{
							foreach (Type t in delegateParameterTypes)
							{
								Console.WriteLine("\t" + t);
							}
						}
						else
						{
							Console.WriteLine("\tNo delegate types");
						}
					}
				}
			}
		}


		/// <summary>
		/// Gets a list of parameters for the Delegate Type.
		/// </summary>
		/// <param name="d">The Delegate Type</param>
		/// <returns>A list of parameter types</returns>
		private static Type[] getDelegateParameterTypes(Type d)
		{
			// This method was stolen from: http://msdn.microsoft.com/en-us/library/ms228976.aspx

			if (d.BaseType != typeof(MulticastDelegate))
				throw new ApplicationException("Not a delegate.");

			MethodInfo invoke = d.GetMethod("Invoke");
			if (invoke == null)
				throw new ApplicationException("Not a delegate.");

			ParameterInfo[] parameters = invoke.GetParameters();
			Type[] typeParameters = new Type[parameters.Length];
			for (int i = 0; i < parameters.Length; i++)
			{
				typeParameters[i] = parameters[i].ParameterType;
			}
			return typeParameters;
		}


		/// <summary>
		/// Tie this event handler to all of the locally-owned entities to send events across the network
		/// </summary>
		/// <param name="e">The entity event arguments</param>
		public void EventHandler(ReflectiveEventArgs e)
		{
			// No need to lock the outgoing queue, it will always be added to, then pulled from the Game thread
			// Add an outgoing message to call a method on any remote computers
			// Couldn't be easier, :D
			outgoingMessageQueue.Enqueue(new AOReflectiveOutgoingMessage(e.TargetID,
			                                                             e.RemoteMethodName,
			                                                             e.RemoteMethodParameters));
		}


		/// <summary>
		/// Tie this event handler to all of the locally-owned entities to send events across the network
		/// </summary>
		/// <param name="sender">The sender of the event</param>
		/// <param name="e">The entity event arguments</param>
		public void EventHandler(Object sender, ReflectiveEventArgs e)
		{
			// Note: The sender is completely ignored
			EventHandler(e);
		}

		#endregion


		/// <summary>
		/// This will return a List of servers that have posted themselves on a matchmaking service. The list contains Tuples whose first part is a String representing the server's IP address, and the second part is an int port number
		/// </summary>
		/// <returns>A list of servers that contains Tuples whose first part is a String representing the server's IP address, and the second part is an int port number</returns>
		public List<Tuple<String, int>> GetServerIPs()
		{
			List<Tuple<String, int>> serverIPs = new List<Tuple<String, int>>();

			foreach (String matchmaker in matchmakers)
			{
				try
				{
					// TODO: Send a game and version filter to the server
					// Grab a list of recent servers from the matchmaker
					HttpWebRequest getServersRequest = (HttpWebRequest)WebRequest.Create(matchmaker + "getServers.php");
					getServersRequest.UserAgent = "game";
					WebResponse response = getServersRequest.GetResponse();
					XElement xmlDoc = XElement.Load(response.GetResponseStream());

					// Parse out the server information for each server
					// This can also be done in LINQ, but I'd rather just do this:
					foreach (XElement server in xmlDoc.Descendants("server"))
					{
						XElement ipElement = server.Element("ipaddress");
						XElement portElement = server.Element("port");
						if (ipElement != null && portElement != null)
						{
							serverIPs.Add(new Tuple<String, int>(ipElement.Value, int.Parse(portElement.Value)));
						}
					}

				}
				catch (WebException webException)
				{
					Console.WriteLine(webException.Message);
					throw;
				}
				catch (UriFormatException uriFormatException)
				{
					Console.WriteLine(uriFormatException.Message);
					throw;
				}
			}

			return serverIPs;
		}



		#region Server Beacon

		/// <summary>
		/// Starts the server beacon. This will start a new thread that will broadcast this computer's location to the matchmakers
		/// </summary>
		public void StartServerBeacon()
		{
			if (serverBeacon != null)
			{
				StopServerBeacon();
			}

			serverBeaconActive = true;

			serverBeacon = new Thread(serverBeaconThread);
			serverBeacon.Start();
		}


		/// <summary>
		/// Stop the server beacon
		/// </summary>
		public void StopServerBeacon()
		{
			if(serverBeacon != null)
			{
				serverBeaconActive = false;

				// Do we really care about the thread ending gracefully? Nope. Kill it now!
				serverBeacon.Abort();
				serverBeacon = null;
			}
		}


		/// <summary>
		/// Loops until serverBeaconActive becomes false, broadcasting this computer's location to the matchmaking servers at regular intervals
		/// </summary>
		private void serverBeaconThread()
		{
			TimeSpan beaconTimeSpan = new TimeSpan(0, 0, 1 /*minute(s)*/, 0);
			Version version = Assembly.GetExecutingAssembly().GetName().Version;
			String assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
			try
			{
				while (serverBeaconActive)
				{
					// Piiinnngggg
					serverBeaconBroadcast(assemblyName, version);

					Thread.Sleep(beaconTimeSpan);
				}
			}
			catch (ThreadAbortException threadAbortException)
			{
				Console.WriteLine("Server Beacon Thread Aborted! " + threadAbortException.Message);
			}
		}


		/// <summary>
		/// Actually sends the matchmaking servers a post request
		/// </summary>
		private void serverBeaconBroadcast(String assemblyName, Version version)
		{
			foreach (String matchmaker in matchmakers)
			{
				//try
				{
					HttpWebRequest getServersRequest = (HttpWebRequest)WebRequest.Create(String.Format("{0}postServer.php?game={1}&version={2}.{3}.{4}.{5}",
																									   matchmaker,
																									   assemblyName,
																									   version.Major,
																									   version.Minor,
																									   version.Build,
																									   version.Revision));
					getServersRequest.UserAgent = "game";
					WebResponse response = getServersRequest.GetResponse();
					XElement xmlDoc = XElement.Load(response.GetResponseStream());

					if(xmlDoc.Name.LocalName.ToLower() == "success")
					{
						// Good news! I'm not going to worry about the particulars
					}
					else
					{
						Console.WriteLine("Server beacon replied: {0}({1})", xmlDoc.Name.LocalName, xmlDoc.Attribute("code"));
						Debugger.Break();
					}
				}
				//catch (Exception ex)
				//{
				//	Console.WriteLine(ex.Message);
				//	throw;
				//}
			}
		}

		#endregion


		#region Server Information Server

		/// <summary>
		/// Starts the server's information... server...
		/// </summary>
		public void StartServerInfoServer()
		{
			if(serverInfo != null)
			{
				StopServerInfoServer();
			}

			serverInfoActive = true;
			serverInfo = new Thread(serverInfoThread);
			serverInfo.Start();
		}


		/// <summary>
		/// Stops the server's information server
		/// </summary>
		public void StopServerInfoServer()
		{
			if (serverInfo != null)
			{
				serverInfoActive = false;
				serverInfo = null;
			}
		}


		private void serverInfoThread()
		{
			IPEndPoint localIpEndPoint = new IPEndPoint(IPAddress.Any, 18189);
			IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
			UdpClient udpServer = new UdpClient(localIpEndPoint);

			try
			{

				while(serverInfoActive)
				{
					if(udpServer.Available > 0)
					{
						// Woot!  A client
						byte[] bytes = udpServer.Receive(ref remoteIpEndPoint);

						BinaryReader br = new BinaryReader(new MemoryStream(bytes));
						UInt32 handshake = br.ReadUInt32();
						if (handshake != World.StreamIdent)
						{
							// TODO: See if this actually kills the whole datagram and allows the server to continue normally
							br.Close();
						}

						UInt32 version = br.ReadUInt32();

						StreamType streamType = (StreamType)br.ReadByte();
						if (streamType != StreamType.RequestServerInfo)
						{
							Debugger.Break();
						}

						MemoryStream memStream = new MemoryStream();
						BinaryWriter bw = new BinaryWriter(memStream);

						bw.Write(World.StreamIdent);
						bw.Write(World.Version);
						bw.Write((byte)StreamType.ServerInfo);

						bw.Write(serverName);
						bw.Write(serverPlayerCount);
						bw.Write(serverPlayerCapacity);
						udpServer.Send(memStream.GetBuffer(), (int)memStream.Length, remoteIpEndPoint);
						break;

					}
					else
					{
						Thread.Sleep(0);
					}
				}

			}
			finally
			{
				udpServer.Close();
			}
		}

		#endregion

		public void StartGame()
		{
			Dictionary<TcpClient, BinaryWriter>.ValueCollection clientWriters = beginSend().Values;
			int count = 0;
			foreach (BinaryWriter clientWriter in clientWriters)
			{
				count++;

				SendGameStateTo(clientWriter);

				int clientStartingID = (Int32.MaxValue / 4) * count;
				new AOReflectiveOutgoingMessage(SpecialTargetTheGame, "StartClient", new object[] { clientStartingID }).Serialize(clientWriter);

				//Force clientForce = new Force(world, world.GetNextForceID(), 1000, Team.Team2);
				//world.AddForce(clientForce);
				//Controller clientController = new Controller(world, world.GetNextControllerID(), ControllerRole.Remote, clientForce);
				//world.AddController(clientController);

				// Flush the outgoing queue
				ProcessOutgoingQueue();

				//world.CreatePowerGrid(clientForce);
				

				/*
				// TODO: Allow targeted messages in the queue and change this to a queue message
				// Tell this client about their own controller
				new AOReflectiveOutgoingMessage(SpecialTargetNetworkingClass,
				                                "CreateController",
				                                new object[]{(int)ControllerRole.Local, clientForce.ID}).Serialize(clientWriter);
				*/

				// Set this guy's focus to his start location
				//new AOReflectiveOutgoingMessage(world.ID, "SetFocus", new object[] { startingStation.Position.Center }).Serialize(clientWriter);
			}
			endSend();
		}
	}
}
