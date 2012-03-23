using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using AsteroidOutpost.Entities;
using AsteroidOutpost.Interfaces;
using AsteroidOutpost.Screens;
using C3.XNA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AsteroidOutpost.Components
{
	/// <summary>
	/// Displays links to stuff: Linker. Don't complain
	/// </summary>
	public class Linker : Component, ICanKillSelf
	{
		private Position position;
		private int postDeserializePositionID;		// For serialization linking, don't use this

		private List<Tuple<Predicate<Entity>, Color, float>> links = new List<Tuple<Predicate<Entity>, Color, float>>();


		public Linker(AsteroidOutpostScreen theGame, IComponentList componentList, Position position)
			: base(theGame, componentList)
		{
			this.position = position;
		}


		public Linker(BinaryReader br)
			: base(br)
		{
			postDeserializePositionID = br.ReadInt32();
		}


		/// <summary>
		/// After deserializing, this should be called to link this object to other objects
		/// </summary>
		/// <param name="theGame"></param>
		public override void PostDeserializeLink(AsteroidOutpostScreen theGame)
		{
			base.PostDeserializeLink(theGame);

			position = theGame.GetComponent(postDeserializePositionID) as Position;

			if (position == null)
			{
				Debugger.Break();
			}
		}


		public List<Tuple<Predicate<Entity>, Color, float>> Links
		{
			get
			{
				return links;
			}
		}


		public override void Draw(SpriteBatch spriteBatch, float scaleModifier, Color tint)
		{
			float maxDistance = 0;
			foreach (var tuple in Links)
			{
				maxDistance = Math.Max(maxDistance, tuple.Item3);
			}

			if (maxDistance > 0)
			{

				List<Entity> nearbyEntities = theGame.EntitiesInArea((int)(position.Center.X - maxDistance),
																	 (int)(position.Center.Y - maxDistance),
				                                                     (int)(maxDistance * 2),
				                                                     (int)(maxDistance * 2));

				foreach (var nearbyEntity in nearbyEntities)
				{
					foreach (var tuple in Links)
					{
						if (position.Distance(nearbyEntity.Position) <= tuple.Item3 && tuple.Item1(nearbyEntity))
						{
							spriteBatch.DrawLine(theGame.WorldToScreen(position.Center),
												 theGame.WorldToScreen(nearbyEntity.Position.Center),
							                     tuple.Item2,
							                     1);
						}
					}
				}
			}

			//base.Draw(spriteBatch, scaleModifier, tint);
		}
	}
}
