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
		//private Position position;
		//private int postDeserializePositionID;		// For serialization linking, don't use this

		private List<Tuple<Predicate<int>, Color, float>> links = new List<Tuple<Predicate<int>, Color, float>>();


		public Linker(World world, int entityID)
			: base(world, entityID)
		{
			//this.position = position;
		}


		//public Linker(BinaryReader br)
		//    : base(br)
		//{
		//    //postDeserializePositionID = br.ReadInt32();
		//}


		///// <summary>
		///// After deserializing, this should be called to link this object to other objects
		///// </summary>
		///// <param name="world"></param>
		//public override void PostDeserializeLink(World world)
		//{
		//    base.PostDeserializeLink(world);

		//    // TODO: 2012-08-10 I probably need this down the line
		//    //position = world.GetComponents(postDeserializePositionID) as Position;

		//    //if (position == null)
		//    //{
		//    //    Debugger.Break();
		//    //}
		//}


		public List<Tuple<Predicate<int>, Color, float>> Links
		{
			get
			{
				return links;
			}
		}


		//public override void Draw(SpriteBatch spriteBatch, float scaleModifier, Color tint)
		//{
		//    float maxDistance = 0;
		//    foreach (var tuple in Links)
		//    {
		//        maxDistance = Math.Max(maxDistance, tuple.Item3);
		//    }

		//    if (maxDistance > 0)
		//    {

		//        Position position = world.GetComponent<Position>(entityID);
		//        List<int> nearbyEntities = world.EntitiesInArea((int)(position.Center.X - maxDistance),
		//                                                        (int)(position.Center.Y - maxDistance),
		//                                                        (int)(maxDistance * 2),
		//                                                        (int)(maxDistance * 2));

		//        foreach (var nearbyEntity in nearbyEntities)
		//        {
		//            foreach (var tuple in Links)
		//            {
		//                Position nearbyPosition = world.GetComponent<Position>(nearbyEntity);
		//                if (position.Distance(nearbyPosition) <= tuple.Item3 && tuple.Item1(nearbyEntity))
		//                {
		//                    spriteBatch.DrawLine(world.WorldToScreen(position.Center),
		//                                         world.WorldToScreen(nearbyPosition.Center),
		//                                         tuple.Item2,
		//                                         1);
		//                }
		//            }
		//        }
		//    }

		//    //base.Draw(spriteBatch, scaleModifier, tint);
		//}
	}
}
