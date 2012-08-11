using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteroidOutpost.Components;
using AsteroidOutpost.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AsteroidOutpost.Systems
{
	class ConstructionSystem
	{
		/// <summary>
		/// Draw this constructing entity to the screen
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="scaleModifier"></param>
		/// <param name="tint"></param>
		protected void DrawConstructing(SpriteBatch spriteBatch, float scaleModifier, Color tint)
		{
			float percentComplete = (float)(MineralsToConstruct - mineralsLeftToConstruct) / MineralsToConstruct;
			byte rgb = (byte)((percentComplete * 100.0) + 150.0);
			tint = new Color(rgb, rgb, rgb).Blend(tint);

			//base.Draw(spriteBatch, scaleModifier, tint);
			
			// Draw a progress bar
			//spriteBatch.FillRectangle(world.Scale(new Vector2(-Radius.Value, Radius.Value - 6)) + world.WorldToScreen(Position.Center), world.Scale(new Vector2(Radius.Width, 6)), Color.Gray);
			//spriteBatch.FillRectangle(world.Scale(new Vector2(-Radius.Value, Radius.Value - 6)) + world.WorldToScreen(Position.Center), world.Scale(new Vector2(Radius.Width * percentComplete, 6)), Color.RoyalBlue);
		}


		/// <summary>
		/// Is this a valid place to build?
		/// </summary>
		/// <returns>True if it's legal to build here, false otherwise</returns>
		public virtual bool IsValidToBuildHere()
		{
			bool valid = true;

			// This will grab all objects who's bounding square intersects with us
			List<Entity> nearbyEntities = world.EntitiesInArea(Rect);
			foreach (Entity entity in nearbyEntities)
			{
				// Now determine if they are really intersecting
				if(entity.Solid && Position.IsIntersecting(entity.Position))
				{
					valid = false;
					break;
				}
			}
			return valid;
		}
	}
}
