using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.IO;
using System.Net;
using System.Threading;

namespace AsteroidOutpost.Networking
{

	internal delegate void ClientConnectedHandler(object sender, ClientConnectedHandlerArgs args);


	/// <summary>
	/// A generic, multi-threaded networking class that can act as both a client and a server
	/// </summary>
	abstract class Network : IDisposable
	{
		private const bool DEATH_AND_TAXES = true;		// :(

		private readonly List<TcpClient> clients = new List<TcpClient>();
		private readonly List<Thread> clientListenThreads = new List<Thread>();
		private readonly Dictionary<TcpClient, BinaryWriter> clientOutgoingStreams = new Dictionary<TcpClient, BinaryWriter>();
		private TcpListener tcpListener;
		private Thread listenThread;

		public event ClientConnectedHandler ClientConnected;


		/// <summary>
		/// Terminate all networking threads
		/// </summary>
		public virtual void Dispose()
		{
			if (listenThread != null)
			{
				listenThread.Abort();
			}

			lock (clientListenThreads)
			{
				foreach (Thread thread in clientListenThreads)
				{
					thread.Abort();
				}
			}
		}


		/// <summary>
		/// Connect to the given address on the given port
		/// </summary>
		/// <param name="address">The address you want to connect to</param>
		/// <param name="port">The port you want to connect on</param>
		/// <exception cref="System.Net.Sockets.SocketException"></exception>
		protected virtual void connect(IPAddress address, int port)
		{
			TcpClient newClient = new TcpClient();
			newClient.Connect(address, port);

			addNewClient(newClient);
		}


		/// <summary>
		/// Starts listening on the given port (typically servers only)
		/// </summary>
		/// <param name="port">The port to listen on</param>
		public virtual void StartListening(int port)
		{
			if (tcpListener == null)
			{
				tcpListener = new TcpListener(IPAddress.Any, port);
				listenThread = new Thread(listenForClients);
				listenThread.Start();
			}
		}


		/// <summary>
		/// Listen for connecting clients (in a separate thread)
		/// </summary>
		private void listenForClients()
		{
			try
			{
#if DEBUG
				Console.WriteLine("Starting to listen for clients...");
#endif
				tcpListener.Start();

				while (DEATH_AND_TAXES)
				{
					// Doing it this way allows for abortions, >:D
					if (tcpListener.Pending())
					{
#if DEBUG
						Console.WriteLine("A client connected!");
#endif

						// Blocking, but since we know there's someone waiting, this should be instant
						TcpClient client = tcpListener.AcceptTcpClient();
						addNewClient(client);

						OnClientConnected(client);
					}
					else
					{
						// Let up to allow other threads processor time
						Thread.Sleep(0);
					}
				}
			}
			catch (ThreadAbortException)
			{
				// Nobody likes me
				tcpListener.Stop();
			}
		}


		/// <summary>
		/// Triggered when a client connects
		/// </summary>
		/// <param name="client">The new client</param>
		protected virtual void OnClientConnected(TcpClient client)
		{
			if (ClientConnected != null)
			{
				ClientConnected(this, new ClientConnectedHandlerArgs(client));
			}
		}


		private void addNewClient(TcpClient client)
		{

#if DEBUG
			Console.WriteLine("New connection! Creating thread to listen");
#endif
			// Oh boy oh boy
			// Create a thread to handle communication with the new client

			Thread clientThread = new Thread(handleClientComm);
			clientThread.Start(client);

			// Add pieces of the client to a whole bunch of collections for easy access
			lock (clients)
			{
				clients.Add(client);
			}
			lock (clientListenThreads)
			{
				clientListenThreads.Add(clientThread);
			}
			lock (clientOutgoingStreams)
			{
				clientOutgoingStreams.Add(client, new BinaryWriter(client.GetStream()));
			}
		}


		/// <summary>
		/// This thread handles incoming packets from a client
		/// </summary>
		/// <param name="client">The TcpClient object to handle messages for</param>
		private void handleClientComm(object client)
		{
			TcpClient tcpClient = (TcpClient)client;
			try
			{
				NetworkStream clientStream = tcpClient.GetStream();

				while (DEATH_AND_TAXES)
				{
					//try
					{
						// Doing it this way allows for abortions, >:D
						if (clientStream.DataAvailable)
						{
							handleClientMessage(clientStream);
						}
						else
						{
							// Let up to allow other threads processor time
							Thread.Sleep(0);
						}


						if(!tcpClient.Connected)
						{
#if DEBUG
							Console.WriteLine("Client has disconnected");
#endif
							break;
						}
					}
					/*
					catch(Exception ex)
					{
						// A socket error has occurred, I guess we should terminate?
						Console.WriteLine(ex.Message);
						throw;
						//break;
					}
					*/
				}
			}
			finally
			{
				// There has been a problem, disconnect the client
				tcpClient.Close();
				lock (clients)
				{
					clients.Remove(tcpClient);
				}
				lock (clientListenThreads)
				{
					clientListenThreads.Remove(Thread.CurrentThread);
				}
				lock (clientOutgoingStreams)
				{
					clientOutgoingStreams.Remove(tcpClient);
				}
			}
		}


		/// <summary>
		/// Handles client messages on the given stream
		/// </summary>
		/// <param name="stream">The stream that a message has arrived on</param>
		protected abstract void handleClientMessage(Stream stream);


		/// <summary>
		/// Allows derived classes to access the streams directly to send data. endSend must be called for each beginSend
		/// </summary>
		protected Dictionary<TcpClient, BinaryWriter> beginSend()
		{
			// Lock the dictionary
			Monitor.Enter(clientOutgoingStreams);
			return clientOutgoingStreams;
		}


		/// <summary>
		/// Allows derived classes to access the streams directly to send data. endSend must be called for each beginSend
		/// </summary>
		protected BinaryWriter beginSend(TcpClient client)
		{
			// Lock the dictionary
			Monitor.Enter(clientOutgoingStreams);
			return clientOutgoingStreams[client];
		}


		/// <summary>
		/// Ends sending
		/// </summary>
		protected void endSend()
		{
			// Always remember to flush!
			foreach (BinaryWriter bw in clientOutgoingStreams.Values)
			{
				bw.Flush();
			}

			// Unlock
			Monitor.Exit(clientOutgoingStreams);
		}
	}
}
