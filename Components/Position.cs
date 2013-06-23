using System;
using System.IO;
using System.Xml.Serialization;
using AsteroidOutpost.Entities;
using AsteroidOutpost.Entities.Eventing;
using AsteroidOutpost.Eventing;
using AsteroidOutpost.Interfaces;
using AsteroidOutpost.Networking;
using AsteroidOutpost.Screens;
using C3.XNA;
using Microsoft.Xna.Framework;

namespace AsteroidOutpost.Components
{
	public class Position : Component, IQuadStorable
	{
		private Vector2 offset;		// from the origin (0,0)


		//[EventReplication(EventReplication.ServerToClients)]
		public event Action<EntityMovedEventArgs> MovedEvent;


		public Position(int entityID) : base(entityID) {}
		public Position(int entityID, Vector2 center, int radius = 0)
			: base(entityID)
		{
			Solid = true;
			offset = center;
			this.Radius = radius;
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
				if (MovedEvent != null)
				{
					MovedEvent(new EntityMovedEventArgs(this, delta));
				}
			}
		}



		// This is left here (for now) for my reflection system
		public void SetCenter(Vector2 newCenter)
		{
			Center = newCenter;
		}


		/// <summary>
		/// Gets whether this entity is solid or not
		/// </summary>
		public bool Solid { get; set; }


		/// <summary>
		/// Gets or set the radius
		/// </summary>
		public int Radius { get; set; }


		/// <summary>
		/// Gets the width
		/// </summary>
		[XmlIgnore]
		public int Width
		{
			get { return Radius * 2; }
		}


		/// <summary>
		/// Gets the height
		/// </summary>
		[XmlIgnore]
		public int Height
		{
			get { return Radius * 2; }
		}


		/// <summary>
		/// Gets the Top
		/// </summary>
		[XmlIgnore]
		public int Top
		{
			get { return (int)(Center.Y + 0.5) - Radius; }
		}


		/// <summary>
		/// Gets the Left
		/// </summary>
		[XmlIgnore]
		public int Left
		{
			get { return (int)(Center.X + 0.5) - Radius; }
		}


		/// <summary>
		/// Gets the Right
		/// </summary>
		[XmlIgnore]
		public int Right
		{
			get { return (int)(Center.X + 0.5) + Radius; }
		}


		/// <summary>
		/// Gets the bottom
		/// </summary>
		[XmlIgnore]
		public int Bottom
		{
			get { return (int)(Center.Y + 0.5) + Radius; }
		}


		/// <summary>
		/// The rectangle that defines the object's boundaries.
		/// </summary>
		[XmlIgnore]
		public Rectangle Rect
		{
			get
			{
				return new Rectangle(Left,
				                     Top,
				                     Radius * 2,
				                     Radius * 2);
			}
		}



		public bool IsIntersecting(Position other)
		{
			return IsIntersecting(other.Center, other.Radius);
		}
		public bool IsIntersecting(Vector2 point, int otherRadius)
		{
			return Distance(point) < (Radius + otherRadius);
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
