using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AsteroidOutpost.Screens;

namespace AsteroidOutpost.Networking
{
	interface ISerializable
	{
		/// <summary>
		/// Serializes this object
		/// </summary>
		/// <param name="bw">The binary writer to serialize to</param>
		void Serialize(BinaryWriter bw);


		/// <summary>
		/// After deserializing, this should be called to link this object to other objects
		/// </summary>
		/// <param name="theGame"></param>
		void PostDeserializeLink(AsteroidOutpostScreen theGame);
	}
}
