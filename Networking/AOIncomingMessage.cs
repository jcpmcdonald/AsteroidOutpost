using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using AsteroidOutpost.Entities;
using AsteroidOutpost.Screens;
using Microsoft.Xna.Framework;

namespace AsteroidOutpost.Networking
{
	abstract class AOIncomingMessage
	{

		/// <summary>
		/// Deserialize additional information from a BinaryReader
		/// Note: This is done inside the Thread that read the data
		/// </summary>
		/// <param name="br">The binary reader to read additional information from</param>
		protected abstract void deserialize(BinaryReader binaryReader);

		/// <summary>
		/// Execute this message. This should only be done from the Game Thread to avoid threading issues
		/// </summary>
		public abstract void Execute(AsteroidOutpostScreen theGame, AONetwork network, TimeSpan deltaTime);

	}
}
