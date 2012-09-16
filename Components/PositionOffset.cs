using System.Diagnostics;
using System.IO;
using AsteroidOutpost.Entities;
using AsteroidOutpost.Interfaces;
using AsteroidOutpost.Screens;
using Microsoft.Xna.Framework;

namespace AsteroidOutpost.Components
{
	class PositionOffset : Position
	{
		// Store the parent position, and use the base's "center" as an offset from the parent. Confused? Me too, but it's pretty slick when you really think hard
		private Position parent;
		private int postDeserializeParentID;		// For serialization linking, don't use this


		public PositionOffset(World world, int entityID, Position parentPosition, Vector2 offset)
			: base(world, entityID, offset)
		{
			parent = parentPosition;
		}

		public PositionOffset(World world, int entityID, Position parentPosition, Vector2 offset, Vector2 velocity)
			: base(world, entityID, offset, velocity)
		{
			parent = parentPosition;
		}


		///// <summary>
		///// Initializes this Entity from a BinaryReader
		///// </summary>
		///// <param name="br">The BinaryReader to Deserialize from</param>
		//public PositionOffset(BinaryReader br)
		//    : base(br)
		//{
		//    postDeserializeParentID = br.ReadInt32();
		//}


		///// <summary>
		///// Serializes this object
		///// </summary>
		///// <param name="bw">The binary writer to serialize to</param>
		//public override void Serialize(BinaryWriter bw)
		//{
		//    // Always serialize the base first because we can't pick the deserialization order
		//    base.Serialize(bw);

		//    bw.Write(parent.EntityID);
		//}


		/// <summary>
		/// After deserializing, this should be called to link this object to other objects
		/// </summary>
		/// <param name="world"></param>
		//public override void PostDeserializeLink(World world)
		//{
		//    base.PostDeserializeLink(world);

		//    parent = world.GetComponents(postDeserializeParentID) as Position;

		//    if (parent == null)
		//    {
		//        Debugger.Break();
		//    }
		//}


		/// <summary>
		/// Gets the center coordinates of this component
		/// </summary>
		public override Vector2 Center
		{
			get
			{
				return parent.Center + base.Center;
			}
			set
			{
				base.Center = value - parent.Center;
			}
		}
	}
}
