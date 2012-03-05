﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using AsteroidOutpost.Screens;
using Microsoft.Xna.Framework;

namespace AsteroidOutpost.Entities
{
	class PositionOffset : Position
	{
		// Store the parent position, and use the base's "center" as an offset from the parent. Confused? Me too, but it's pretty slick when you really think hard
		private Position parent;
		private int postDeserializeParentID;		// For serialization linking, don't use this


		public PositionOffset(AsteroidOutpostScreen theGame, IComponentList componentList, Force owningForce, Position parentPosition, Vector2 offset)
			: base(theGame, componentList, owningForce, offset)
		{
			parent = parentPosition;
		}

		public PositionOffset(AsteroidOutpostScreen theGame, IComponentList componentList, Force owningForce, Position parentPosition, Vector2 offset, Vector2 velocity)
			: base(theGame, componentList, owningForce, offset, velocity)
		{
			parent = parentPosition;
		}


		/// <summary>
		/// Initializes this Entity from a BinaryReader
		/// </summary>
		/// <param name="br">The BinaryReader to Deserialize from</param>
		public PositionOffset(BinaryReader br)
			: base(br)
		{
			postDeserializeParentID = br.ReadInt32();
		}


		/// <summary>
		/// Serializes this object
		/// </summary>
		/// <param name="bw">The binary writer to serialize to</param>
		public override void Serialize(BinaryWriter bw)
		{
			// Always serialize the base first because we can't pick the deserialization order
			base.Serialize(bw);

			bw.Write(parent.ID);
		}


		/// <summary>
		/// After deserializing, this should be called to link this object to other objects
		/// </summary>
		/// <param name="theGame"></param>
		public override void PostDeserializeLink(AsteroidOutpostScreen theGame)
		{
			base.PostDeserializeLink(theGame);

			parent = theGame.GetComponent(postDeserializeParentID) as Position;

			if (parent == null)
			{
				Debugger.Break();
			}
		}


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
