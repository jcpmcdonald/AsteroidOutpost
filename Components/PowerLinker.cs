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
	internal class PowerLinker : Component
	{
		private readonly IPowerGridNode relatedPowerNode;

		protected Force owningForce;
		private int postDeserializeOwningForceID;		// For serialization linking, don't use this


		public PowerLinker(World world, Force owningForce, IPowerGridNode powerGridNode)
			: base(world)
		{
			relatedPowerNode = powerGridNode;

			this.owningForce = owningForce;
		}


		public PowerLinker(BinaryReader br)
			: base(br)
		{
			postDeserializeOwningForceID = br.ReadInt32();
		}

		public override void Serialize(BinaryWriter bw)
		{
			// Always serialize the base first because we can't pick the deserialization order
			base.Serialize(bw);

			if (owningForce == null)
			{
				// I don't think this should ever be the case
				Debugger.Break();
				bw.Write(-1);
			}
			bw.Write(owningForce.ID);
		}

		/// <summary>
		/// After deserializing, this should be called to link this object to other objects
		/// </summary>
		/// <param name="world"></param>
		public override void PostDeserializeLink(World world)
		{
			owningForce = world.GetForce(postDeserializeOwningForceID);
			if (owningForce == null)
			{
				// I think something is wrong, there should always be an owning force
				Debugger.Break();
			}
		}


		public override void Draw(SpriteBatch spriteBatch, float scaleModifier, Color tint)
		{
			foreach (var powerLink in world.PowerGrid(owningForce).GetAllPowerLinks(relatedPowerNode))
			{
				Color linkColor;
				if (world.PowerGrid(owningForce).IsPowerRoutableBetween(relatedPowerNode, powerLink.Value))
				{
					linkColor = Color.Yellow;
				}
				else
				{
					linkColor = Color.Red;
				}

				spriteBatch.DrawLine(world.WorldToScreen(relatedPowerNode.PowerLinkPointAbsolute),
									 world.WorldToScreen(powerLink.Value.PowerLinkPointAbsolute),
									 linkColor);
			}


			base.Draw(spriteBatch, scaleModifier, tint);
		}
	}
}
