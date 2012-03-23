﻿using System;
using System.Collections.Generic;
using System.IO;
using AsteroidOutpost.Entities.Weapons;
using AsteroidOutpost.Interfaces;
using AsteroidOutpost.Screens;
using C3.XNA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using XNASpriteLib;

namespace AsteroidOutpost.Entities.Structures
{
	class LaserTower : ConstructableEntity
	{
		private static Sprite sprite;
		private static float angleStep;


		private Weapon weapon;
		private Entity target;

		// For serialized linking
		private int targetID;

		public LaserTower(AsteroidOutpostScreen theGame, IComponentList componentList, Force theowningForce, Vector2 theCenter)
			: base(theGame, componentList, theowningForce, theCenter, 15, 100)
		{
			Init();
		}


		public LaserTower(BinaryReader br) : base(br)
		{
			targetID = br.ReadInt32();
			Init();
		}

		private void Init()
		{
			weapon = new Laser(theGame, this);
			animator = new SpriteAnimator(sprite);
			PowerLinkPointRelative = new Vector2(0, -13);
		}


		/// <summary>
		/// This is where all entities should do any resource loading that will be required. This will be called once per game.
		/// </summary>
		/// <param name="spriteBatch">The sprite batch</param>
		/// <param name="content">The content manager</param>
		public static void LoadContent(SpriteBatch spriteBatch, ContentManager content)
		{
			sprite = new Sprite(File.OpenRead(@"..\Sprites\LaserTower.sprx"), spriteBatch.GraphicsDevice);
			angleStep = 360.0f / sprite.OrientationLookup.Count;
		}


		/// <summary>
		/// Serialize this Entity
		/// </summary>
		/// <param name="bw">The BinaryWriter to stream to</param>
		public override void Serialize(BinaryWriter bw)
		{
			// Always serialize the base first because we can't pick the deserialization order
			base.Serialize(bw);

			if (target != null)
			{
				bw.Write(target.ID);
			}
			else
			{
				bw.Write(-1);
			}
		}


		/// <summary>
		/// After deserializing, this should be called to link this object to other objects
		/// </summary>
		/// <param name="theGame"></param>
		public override void PostDeserializeLink(AsteroidOutpostScreen theGame)
		{
			base.PostDeserializeLink(theGame);

			if (targetID >= 0)
			{
				target = theGame.GetEntity(targetID);
			}
			else
			{
				target = null;
			}
		}


		/// <summary>
		/// How many minerals does this constructable take to build?
		/// </summary>
		public override int MineralsToConstruct
		{
			get { return 400; }
		}


		/// <summary>
		/// Entities should initialize their possible upgrades here
		/// </summary>
		protected override void InitializeUpgrades()
		{
			// No upgrades (yet)
		}


		/// <summary>
		/// Gets the name of this entity
		/// </summary>
		public override string Name
		{
			get { return "Laser Tower"; }
		}


		public Vector2 WeaponPointRelative()
		{
			return new Vector2(0, -20);
		}


		/// <summary>
		/// Updates this entity
		/// </summary>
		/// <param name="deltaTime">The elapsed time since the last update</param>
		public override void Update(TimeSpan deltaTime)
		{
			if (IsConstructing)
			{
				UpdateConstructing(deltaTime);
				//return;
			}
			else if (IsUpgrading)
			{
				UpdateUpgrading(deltaTime);
				//return;
			}
			else
			{
				// TODO:  Change this to server-side code
				float powerToUse = (float)(5.0 * deltaTime.TotalSeconds);
				if (theGame.PowerGrid(owningForce).GetPower(this, powerToUse))
				{
					ScanForTarget();
					if (target != null)
					{
						weapon.Shoot(target);
					}
					weapon.Update(deltaTime);
				}
			}

			base.Update(deltaTime);
		}


		/// <summary>
		/// Draw this entity to the screen
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="scaleModifier"></param>
		/// <param name="tint"></param>
		public override void Draw(SpriteBatch spriteBatch, float scaleModifier, Color tint)
		{
			weapon.Draw(spriteBatch, tint);
			base.Draw(spriteBatch, scaleModifier * 0.7f, tint);
		}


		private void ScanForTarget()
		{
			// Search for nearby enemies
			List<Entity> fairlyNearEntities = theGame.EntitiesInArea(new Rectangle((int)(Position.Center.X - weapon.MaxRange),
																				 (int)(Position.Center.Y - weapon.MaxRange),
																				 weapon.MaxRange * 2,
																				 weapon.MaxRange * 2));
			foreach (Entity entity in fairlyNearEntities)
			{
				if (entity.OwningForce.Team == Team.AI && Position.Distance(entity.Position) - entity.Radius.Value < weapon.MaxRange)
				{
					target = entity;
				}
			}
		}

		public override void GetRangeRings(ref List<Tuple<int, Color, string>> rangeRingDefinition)
		{
			rangeRingDefinition.Add(Tuple.Create(weapon.MaxRange, new Color(150, 50, 50), "Max. Attack Range"));
			rangeRingDefinition.Add(Tuple.Create(weapon.OptimumRange, new Color(200, 50, 50), "Opt. Attack Range"));
			base.GetRangeRings(ref rangeRingDefinition);
		}
	}
}
