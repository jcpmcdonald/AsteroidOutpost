using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using AsteroidOutpost.Components;
using AsteroidOutpost.Entities.Eventing;
using AsteroidOutpost.Interfaces;
using AsteroidOutpost.Screens;
using C3.XNA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using AsteroidOutpost.Networking;
using XNASpriteLib;

namespace AsteroidOutpost.Entities
{
	public abstract class Entity : IQuadStorable, ISerializable, IIdentifiable, IUpdatable, ICanKillSelf
	{
		protected World world;

		// This ID will uniquely identify this object in the game
		protected int id = -1;
		private bool deleteMe;

		protected Force owningForce;
		private int postDeserializeOwningForceID;		// For serialization linking, don't use this


		// Attributes
		protected SpriteAnimator animator;
		private bool solid = true;


		private Position position;
		private Radius radius;
		private HitPoints hitPoints;
		private int postDeserializePositionID;		// For serialization linking, don't use this
		private int postDeserializeSizeID;			// For serialization linking, don't use this
		private int postDeserializeHitPointsID;		// For serialization linking, don't use this



		// Events
		[EventReplication(EventReplication.ServerToClients)]
		public event Action<EntityDyingEventArgs> DyingEvent;

		
		protected Entity(World world, IComponentList componentList, Force owningForce, Vector2 center, int radius, int totalHitPoints)
		{
			this.world = world;
			this.owningForce = owningForce;

			Position = new Position(world, componentList, center);
			Radius = new Radius(world, componentList, Position, radius);
			HitPoints = new HitPoints(world, componentList, totalHitPoints);
			HitPoints.DyingEvent += KillSelf;

			componentList.AddComponent(Position);
			componentList.AddComponent(Radius);
			componentList.AddComponent(HitPoints);
		}

		protected Entity(World world, IComponentList componentList, Force owningForce, Vector2 center, int totalHitPoints)
		{
			this.world = world;
			this.owningForce = owningForce;

			Position = new Position(world, componentList, center);
			HitPoints = new HitPoints(world, componentList, totalHitPoints);
			HitPoints.DyingEvent += KillSelf;

			componentList.AddComponent(Position);
			componentList.AddComponent(HitPoints);
		}


		#region Serializing and Deserializing

		/// <summary>
		/// Initializes this Entity from a BinaryReader
		/// </summary>
		/// <param name="br">The BinaryReader to Deserialize from</param>
		protected Entity(BinaryReader br)
		{
			id = br.ReadInt32();
			postDeserializeOwningForceID = br.ReadInt32();

			postDeserializePositionID = br.ReadInt32();
			postDeserializeSizeID = br.ReadInt32();
			postDeserializeHitPointsID = br.ReadInt32();
		}


		/// <summary>
		/// Serialize this Entity
		/// </summary>
		/// <param name="bw">The BinaryWriter to stream to</param>
		public virtual void Serialize(BinaryWriter bw)
		{
			bw.Write(id);

			if (owningForce == null)
			{
				// I don't think this should ever be the case
				Debugger.Break();
				bw.Write(-1);
			}
			bw.Write(owningForce.ID);

			bw.Write(position.ID);
			bw.Write(radius.ID);
			bw.Write(hitPoints.ID);
		}


		/// <summary>
		/// After deserializing, this should be called to link this object to other objects
		/// </summary>
		/// <param name="world"></param>
		public virtual void PostDeserializeLink(World world)
		{
			this.world = world;

			owningForce = world.GetForce(postDeserializeOwningForceID);
			if(owningForce == null)
			{
				// I think something is wrong, there should always be an owning force
				Debugger.Break();
			}

			
			position = world.GetComponent(postDeserializePositionID) as Position;
			radius = world.GetComponent(postDeserializeSizeID) as Radius;
			hitPoints = world.GetComponent(postDeserializeHitPointsID) as HitPoints;

			if (position == null || radius == null || hitPoints == null)
			{
				Debugger.Break();
			}
		}

		#endregion



		#region Updating and Drawing

		/// <summary>
		/// Updates this component
		/// </summary>
		/// <param name="deltaTime">The amount of time that has passed since the last frame</param>
		public virtual void Update(TimeSpan deltaTime)
		{
			if (animator != null)
			{
				animator.Update(deltaTime);
			}
		}


		/// <summary>
		/// Draws this component
		/// </summary>
		/// <param name="spriteBatch">The sprite batch to draw to</param>
		/// <param name="scaleModifier">The scale modifier to use</param>
		/// <param name="tint">The tint to use</param>
		public virtual void Draw(SpriteBatch spriteBatch, float scaleModifier, Color tint)
		{
			// draw the unit
			if (animator != null)
			{
				animator.Draw(spriteBatch, world.WorldToScreen(Position.Center), 0, scaleModifier / world.ScaleFactor, tint);
			}
		}

		#endregion



		#region Math Stuff




		/// <summary>
		/// Is the path between me and the other entity obstructed by any other entity?
		/// </summary>
		/// <param name="otherEntity"></param>
		/// <returns></returns>
		public bool IsObstructed(Entity otherEntity)
		{
			// We can't be obstructed by ourselves, so create an ignore list
			List<Entity> ignoreList = new List<Entity>();
			ignoreList.Add(this);
			ignoreList.Add(otherEntity);

			return IsLineObstructed(Position.Center, otherEntity.Position.Center, world, ignoreList);
		}
		
		
		/// <summary>
		/// Is the line between two points obstructed by a entity?
		/// </summary>
		/// <param name="point1">The first point on the line</param>
		/// <param name="point2">The second point on the line</param>
		/// <param name="world">A reference to the game</param>
		/// <param name="ignoreList">A list of non-obstructable entities, these entities will be ignored. Use null if you don't want to ignore anything</param>
		/// <returns>True if an entity is blocking the line, false otherwise</returns>
		protected static bool IsLineObstructed(Vector2 point1, Vector2 point2, World world, List<Entity> ignoreList)
		{
			// Check for obstacles in the way
			List<Entity> nearbyEntities = world.EntitiesInArea((int)(Math.Min(point1.X, point2.X) - 0.5),
			                                                   (int)(Math.Min(point1.Y, point2.Y - 0.5)),
			                                                   (int)(Math.Abs(point1.X - point2.X) + 0.5),
			                                                   (int)(Math.Abs(point1.Y - point2.Y) + 0.5));

			foreach (Entity obstructingEntity in nearbyEntities)
			{
				if (obstructingEntity.solid && (ignoreList == null || !ignoreList.Contains(obstructingEntity)))
				{
					if (obstructingEntity.Position.ShortestDistanceToLine(point1, point2) < obstructingEntity.Radius.Value)
					{
						// It's obstructed
						return true;
					}
				}
			}
			
			// Good line
			return false;
		}

		#endregion



		/// <summary>
		/// Gets the Entity's ID
		/// </summary>
		public int ID
		{
			get { return id; }
			set
			{
				id = value;
			}
		}

		
		/// <summary>
		/// Gets the name of this entity
		/// </summary>
		public abstract String Name
		{
			get;
		}


		/// <summary>
		/// Gets the owning force
		/// </summary>
		public Force OwningForce
		{
			get { return owningForce; }
		}


		/// <summary>
		/// Gets whether this entity is solid or not
		/// </summary>
		public bool Solid
		{
			get
			{
				return solid;
			}
			protected set
			{
				solid = value;
			}
		}


		/// <summary>
		/// The rectangle that defines the object's boundaries.
		/// </summary>
		public Rectangle Rect
		{
			get
			{
				return Radius.Rect;
			}
		}



		public Position Position
		{
			get
			{
				return position;
			}
			protected set
			{
				position = value;
			}
		}

		public Radius Radius
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

		public HitPoints HitPoints
		{
			get
			{
				return hitPoints;
			}
			protected set
			{
				hitPoints = value;
			}
		}


		public void KillSelf(EventArgs e)
		{
			// This should only be a local event, so I am authoritative
			SetDead(true, true);
		}


		/// <summary>
		/// Gets whether this Entity should be deleted after this update cycle
		/// </summary>
		public bool IsDead()
		{
			return deleteMe;
		}

		/// <summary>
		/// Sets whether this Entity should be deleted after this update cycle
		/// </summary>
		public void SetDead(bool delMe)
		{
			SetDead(delMe, world.IsServer);
		}


		public void SetDead(bool delMe, bool authoritative)
		{
			if (!authoritative)
			{
				return;
			}

			deleteMe = delMe;

			// Tell everyone that's interested in my death
			if (DyingEvent != null)
			{
				DyingEvent(new EntityDyingEventArgs(this));
			}
		}

		public override string ToString()
		{
			return base.ToString() + " (" + position.Center.X + ", " + position.Center.Y + ")";
		}
	}
}