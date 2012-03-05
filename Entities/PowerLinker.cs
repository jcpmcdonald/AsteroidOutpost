


using System;
using System.Diagnostics;
using System.IO;
using AsteroidOutpost.Screens;
using C3.XNA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AsteroidOutpost.Entities
{
	internal class PowerLinker : Component
	{
		private readonly IPowerGridNode relatedPowerNode;


		public PowerLinker(AsteroidOutpostScreen theGame, IComponentList componentList, Force theOwningForce, IPowerGridNode powerGridNode)
			: base(theGame, componentList, theOwningForce)
		{
			this.relatedPowerNode = powerGridNode;
		}


		public PowerLinker(BinaryReader br)
			: base(br)
		{
		}


		public override void Draw(SpriteBatch spriteBatch, float scaleModifier, Color tint)
		{
			foreach (var powerLink in theGame.PowerGrid(owningForce).GetAllPowerLinks(relatedPowerNode))
			{
				Color linkColor;
				if (theGame.PowerGrid(owningForce).IsPowerRoutableBetween(relatedPowerNode, powerLink.Value))
				{
					linkColor = Color.Yellow;
				}
				else
				{
					linkColor = Color.Red;
				}

				spriteBatch.DrawLine(theGame.WorldToScreen(relatedPowerNode.PowerLinkPointAbsolute),
									 theGame.WorldToScreen(powerLink.Value.PowerLinkPointAbsolute),
									 linkColor);
			}


			base.Draw(spriteBatch, scaleModifier, tint);
		}
	}
}
