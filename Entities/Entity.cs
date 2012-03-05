using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using AsteroidOutpost.Entities.Eventing;
using AsteroidOutpost.Screens;
using C3.XNA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using AsteroidOutpost.Networking;
using XNASpriteLib;

namespace AsteroidOutpost.Entities
{
	public abstract class Entity : Component, IQuadStorable, ISerializable, IReflectionTarget, IUpdatable
	{
		// Attributes
		protected SpriteAnimator animator;
		private bool solid = true;


		private Position position;
		private Radius radius;
		private HitPoints hitPoints;
		private int postDeserializePositionID;		// For serialization linking, don't use this
		private int postDeserializeSizeID;			// For serialization linking, don't use this
		private int postDeserializeHitPointsID;		// For serialization linking, don't use this

		
		protected Entity(AsteroidOutpostScreen theGame, IComponentList componentList, Force owningForce, Vector2 center, int radius, int totalHitPoints)
			: base(theGame, componentList, owningForce)
		{
			Position = new Position(theGame, componentList, owningForce, center);
			Radius = new Radius(theGame, componentList, owningForce, Position, radius);
			HitPoints = new HitPoints(theGame, componentList, owningForce, totalHitPoints);
			HitPoints.DyingEvent += KillSelf;

			componentList.AddComponent(Position);
			componentList.AddComponent(Radius);
			componentList.AddComponent(HitPoints);
		}

		//*
		protected Entity(AsteroidOutpostScreen theGame, IComponentList componentList, Force owningForce, Vector2 center, int totalHitPoints)
			: base(theGame, componentList, owningForce)
		{
			Position = new Position(theGame, componentList, owningForce, center);
			HitPoints = new HitPoints(theGame, componentList, owningForce, totalHitPoints);
			HitPoints.DyingEvent += KillSelf;

			componentList.AddComponent(Position);
			componentList.AddComponent(HitPoints);
		}
		//*/


		#region Serializing and Deserializing

		/// <summary>
		/// Initializes this Entity from a BinaryReader
		/// </summary>
		/// <param name="br">The BinaryReader to Deserialize from</param>
		protected Entity(BinaryReader br)
			: base(br)
		{
			postDeserializePositionID = br.ReadInt32();
			postDeserializeSizeID = br.ReadInt32();
			postDeserializeHitPointsID = br.ReadInt32();
		}


		/// <summary>
		/// Serialize this Entity
		/// </summary>
		/// <param name="bw">The BinaryWriter to stream to</param>
		public override void Serialize(BinaryWriter bw)
		{
			// Always serialize the base first because we can't pick the deserialization order
			base.Serialize(bw);

			bw.Write(position.ID);
			bw.Write(radius.ID);
			bw.Write(hitPoints.ID);
		}


		/// <summary>
		/// After deserializing, this should be called to link this object to other objects
		/// </summary>
		/// <param name="theGame"></param>
		public override void PostDeserializeLink(AsteroidOutpostScreen theGame)
		{
			base.PostDeserializeLink(theGame);

			position = theGame.GetComponent(postDeserializePositionID) as Position;
			radius = theGame.GetComponent(postDeserializeSizeID) as Radius;
			hitPoints = theGame.GetComponent(postDeserializeHitPointsID) as HitPoints;

			if(Position == null || Radius == null)
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
		public override void Update(TimeSpan deltaTime)
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
		public override void Draw(SpriteBatch spriteBatch, float scaleModifier, Color tint)
		{
			// draw the unit
			if (animator != null)
			{
				animator.Draw(spriteBatch, theGame.WorldToScreen(Position.Center), 0, scaleModifier / theGame.ScaleFactor, tint);
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

			return IsLineObstructed(Position.Center, otherEntity.Position.Center, theGame, ignoreList);
		}
		
		
		/// <summary>
		/// Is the line between two points obstructed by a entity?
		/// </summary>
		/// <param name="point1">The first point on the line</param>
		/// <param name="point2">The second point on the line</param>
		/// <param name="theGame">A reference to the game</param>
		/// <param name="ignoreList">A list of non-obstructable entities, these entities will be ignored. Use null if you don't want to ignore anything</param>
		/// <returns>True if an entity is blocking the line, false otherwise</returns>
		protected static bool IsLineObstructed(Vector2 point1, Vector2 point2, AsteroidOutpostScreen theGame, List<Entity> ignoreList)
		{
			// Check for obstacles in the way
			List<Entity> nearbyEntities = theGame.EntitiesInArea((int)(Math.Min(point1.X, point2.X) - 0.5),
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
	}
}