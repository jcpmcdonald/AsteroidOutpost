using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using AsteroidOutpost.Interfaces;
using AsteroidOutpost.Networking;
using AsteroidOutpost.Screens;
using Microsoft.Xna.Framework;

namespace AsteroidOutpost.Entities
{
	public class Radius : Component, ISerializable
	{
		private int value;
		private Position position;
		private int postDeserializePositionID;		// For serialization linking, don't use this


		public Radius(AsteroidOutpostScreen theGame, IComponentList componentList, Position position, int radius)
			: base(theGame, componentList)
		{
			Value = radius;
			this.position = position;
		}

		public Radius(BinaryReader br)
			: base(br)
		{
			postDeserializePositionID = br.ReadInt32();
			Value = br.ReadInt32();
		}


		public override void Serialize(BinaryWriter bw)
		{
			// Always serialize the base first because we can't pick the deserialization order
			base.Serialize(bw);

			bw.Write(position.ID);
			bw.Write(Value);
		}

		public override void PostDeserializeLink(AsteroidOutpostScreen theGame)
		{
			base.PostDeserializeLink(theGame);
			
			position = theGame.GetComponent(postDeserializePositionID) as Position;

			if (position == null)
			{
				Debugger.Break();
			}
		}


		/// <summary>
		/// Gets or set the radius
		/// </summary>
		public int Value
		{
			get
			{
				return value;
			}
			private set
			{
				this.value = value;
			}
		}


		/// <summary>
		/// Gets the width
		/// </summary>
		public int Width
		{
			get { return Value * 2; }
		}


		/// <summary>
		/// Gets the height
		/// </summary>
		public int Height
		{
			get { return Value * 2; }
		}


		/// <summary>
		/// Gets the Top
		/// </summary>
		public int Top
		{
			get { return (int)(position.Center.Y + 0.5) - Value; }
		}


		/// <summary>
		/// Gets the Left
		/// </summary>
		public int Left
		{
			get { return (int)(position.Center.X + 0.5) - Value; }
		}


		/// <summary>
		/// Gets the Right
		/// </summary>
		public int Right
		{
			get { return (int)(position.Center.X + 0.5) + Value; }
		}


		/// <summary>
		/// Gets the bottom
		/// </summary>
		public int Bottom
		{
			get { return (int)(position.Center.Y + 0.5) + Value; }
		}
		
		
		/// <summary>
		/// The rectangle that defines the object's boundaries.
		/// </summary>
		public Rectangle Rect
		{
			get
			{
				return new Rectangle(Left,
				                     Top,
				                     Value * 2,
				                     Value * 2);
			}
		}



		public bool IsIntersecting(Radius other)
		{
			return IsIntersecting(other.position.Center, other.Value);
		}
		public bool IsIntersecting(Vector2 point, int otherRadius)
		{
			return position.Distance(point) < (Value + otherRadius);
		}
	}
}
