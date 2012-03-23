using System.Collections.Generic;
using System.IO;
using AsteroidOutpost.Interfaces;
using AsteroidOutpost.Screens;
using C3.XNA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace AsteroidOutpost.Entities.Structures
{
	abstract class Miner : ConstructableEntity
	{
		protected Miner(AsteroidOutpostScreen theGame, IComponentList componentList, Force theowningForce, Vector2 theCenter, int theRadius, int theMiningRange, int totalHitPoints)
			: base(theGame, componentList, theowningForce, theCenter, theRadius, totalHitPoints)
		{
			
		}


		protected Miner(BinaryReader br) : base(br)
		{
			//miningRange = br.ReadInt32();
		}
	}
}