using System;
using System.IO;
using AsteroidOutpost.Entities;
using AsteroidOutpost.Entities.Eventing;
using AsteroidOutpost.Interfaces;
using AsteroidOutpost.Networking;
using AsteroidOutpost.Screens;
using Microsoft.Xna.Framework;

namespace AsteroidOutpost.Components
{
	public class Position : Component, IUpdatable, ISerializable
	{
		private Vector2 offset;		// from the origin (0,0)
		private Vector2 velocity;
		private int radius;


		//[EventReplication(EventReplication.ServerToClients)]
		public event Action<EntityMovedEventArgs> MovedEvent;


		public Position(World world, Vector2 center, int radius = 0)
			: base(world)
		{
			offset = center;
			this.radius = radius;
		}
 
		public Position(World world, Vector2 center, Vector2 velocity, int radius = 0)
			: base(world)
		{
			offset = center;
			this.velocity = velocity;
			this.radius = radius;
		}


		public Position(BinaryReader br)
			: base(br)
		{
			offset = br.ReadVector2();
			velocity = br.ReadVector2();
			radius = br.ReadInt32();
		}
		
		
		/// <summary>
		/// Serializes this object
		/// </summary>
		/// <param name="bw">The binary writer to serialize to</param>
		public override void Serialize(BinaryWriter bw)
		{
			// Always serialize the base first because we can't pick the deserialization order
			base.Serialize(bw);

			bw.Write(offset);
			bw.Write(velocity);
			bw.Write(radius);
		}



		/// <summary>
		/// Gets the center coordinates of this component
		/// </summary>
		public virtual Vector2 Center
		{
			get
			{
				return offset;
			}
			set
			{
				Vector2 delta = offset - value;

				offset = value;

				// Since we moved, make sure to tell others
				//hasMoved = true;		// For the QuadTree, TODO: Change this!!
				if (MovedEvent != null)
				{
					MovedEvent(new EntityMovedEventArgs(this, Center, delta));
				}
			}
		}



		// This is left here (for now) for my reflection system
		public void SetCenter(Vector2 newCenter)
		{
			Center = newCenter;
		}


		/// <summary>
		/// Gets or sets the X velocity
		/// </summary>
		public Vector2 Velocity
		{
			get
			{
				return velocity;
			}
			set
			{
				velocity = value;
			}
		}


		/// <summary>
		/// Gets or set the radius
		/// </summary>
		public int Radius
		{
			get
			{
				return radius;
			}
			set
			{
				radius = value;
			}
		}


		/// <summary>
		/// Gets the width
		/// </summary>
		public int Width
		{
			get { return radius * 2; }
		}


		/// <summary>
		/// Gets the height
		/// </summary>
		public int Height
		{
			get { return radius * 2; }
		}


		/// <summary>
		/// Gets the Top
		/// </summary>
		public int Top
		{
			get { return (int)(Center.Y + 0.5) - radius; }
		}


		/// <summary>
		/// Gets the Left
		/// </summary>
		public int Left
		{
			get { return (int)(Center.X + 0.5) - radius; }
		}


		/// <summary>
		/// Gets the Right
		/// </summary>
		public int Right
		{
			get { return (int)(Center.X + 0.5) + radius; }
		}


		/// <summary>
		/// Gets the bottom
		/// </summary>
		public int Bottom
		{
			get { return (int)(Center.Y + 0.5) + radius; }
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
									 radius * 2,
									 radius * 2);
			}
		}



		public bool IsIntersecting(Position other)
		{
			return IsIntersecting(other.Center, other.radius);
		}
		public bool IsIntersecting(Vector2 point, int otherRadius)
		{
			return Distance(point) < (radius + otherRadius);
		}


		public virtual float Distance(Position other)
		{
			return Distance(other.offset);
		}
		public virtual float Distance(Vector2 thePoint)
		{
			return Vector2.Distance(offset, thePoint);
		}


		/// <summary>
		/// Returns the shortest distance from this object to the given line segment
		/// </summary>
		/// <param name="p1">One of the line endpoints</param>
		/// <param name="p2">One of the line endpoints</param>
		/// <returns>Returns the shortest distance from this entity to the given line segment</returns>
		public virtual float ShortestDistanceToLine(Vector2 p1, Vector2 p2)
		{
			float m1 = (p1.Y - p2.Y) / (p1.X - p2.X);
			float m2 = 1.0f / -m1;
			float c1 = p1.Y - (m1 * p1.X);
			float c2 = offset.Y - (m2 * offset.X);

			float xI = (c1 - c2) / (m2 - m1);
			float yI = (m2 * xI) + c2;

			float distanceToIntersection = Vector2.Distance(offset, new Vector2(xI, yI));
			//Console.WriteLine(distanceToIntersection);
			float distanceToP1 = Vector2.Distance(offset, p1);
			float distanceToP2 = Vector2.Distance(offset, p2);

			if (Vector2.Distance(p1, new Vector2(xI, yI)) > Vector2.Distance(p1, p2))
			{
				return distanceToP2;
			}
			else if (Vector2.Distance(p2, new Vector2(xI, yI)) > Vector2.Distance(p1, p2))
			{
				return distanceToP1;
			}
			else
			{
				return distanceToIntersection;
			}
		}
	}
}
