using System.IO;
using Microsoft.Xna.Framework;

namespace AsteroidOutpost.Networking
{
	/// <summary>
	/// Extends the BinaryWriter and BinaryReader classes to provide read/write functionality for classes of my choosing.
	/// Superfluous, I know, but I wanted to learn how to use Extensions, and this seemed like a good excuse
	/// </summary>
	public static class BinaryStreamExtensions
	{
		
		/// <summary>
		/// Writes a Vector2 to this BinaryWriter
		/// </summary>
		/// <param name="bw">The BinaryWriter to use (this is implied because this is an Extension method)</param>
		/// <param name="vector">The Vector2 to write</param>
		public static void Write(this BinaryWriter bw, Vector2 vector)
		{
			// Yeah, pretty pointless eh?
			bw.Write(vector.X);
			bw.Write(vector.Y);
		}
		

		/// <summary>
		/// Reads a Vector2 from this BinaryReader
		/// </summary>
		/// <param name="br">The BinaryReader to use (this is implied because this is an Extension method)</param>
		/// <returns>A Vector2 from the reader</returns>
		public static Vector2 ReadVector2(this BinaryReader br)
		{
			return new Vector2(br.ReadSingle(), br.ReadSingle());
		}
	}
}
