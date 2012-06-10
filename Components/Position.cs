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


		//[EventReplication(EventReplication.ServerToClients)]
		public event Action<EntityMovedEventArgs> MovedEvent;


		public Position(World world, IComponentList componentList, Vector2 center)
			: base(world, componentList)
		{
			offset = center;
		}
 
		public Position(World world, IComponentList componentList, Vector2 center, Vector2 velocity)
			: base(world, componentList)
		{
			offset = center;
			this.velocity = velocity;
		}


		public Position(BinaryReader br)
			: base(br)
		{
			offset = br.ReadVector2();
			velocity = br.ReadVector2();
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
		/// Updates this component
		/// </summary>
		/// <param name="deltaTime">The elapsed time since the last update</param>
		public override void Update(TimeSpan deltaTime)
		{
			if (velocity.X != 0.0 || velocity.Y != 0.0)
			{
				// Move!
				Center += velocity * (float)deltaTime.TotalSeconds;
			}
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
		/// Returns the shortest distance from this object to the given line
		/// </summary>
		/// <param name="p1">One of the line endpoints</param>
		/// <param name="p2">One of the line endpoints</param>
		/// <returns>Returns the shortest distance from this entity to the given line</returns>
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
