using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework;

namespace AsteroidOutpost.Networking
{
	abstract class AOOutgoingMessage
	{
		
		/// <summary>
		/// Serialize this message
		/// </summary>
		/// <param name="stream">The stream to use</param>
		public void Serialize(Stream stream)
		{
			Serialize(new BinaryWriter(stream));
		}


		/// <summary>
		/// Serialize this message
		/// </summary>
		/// <param name="bw">The BinaryWriter to use</param>
		public abstract void Serialize(BinaryWriter bw);
	}
}
