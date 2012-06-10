using System;
using System.Diagnostics;
using System.IO;
using AsteroidOutpost.Interfaces;
using AsteroidOutpost.Screens;
using Microsoft.Xna.Framework;

namespace AsteroidOutpost.Components
{
	public class Accumulator : Component
	{
		private readonly IComponentList componentList;
		private Position position;
		private int postDeserializePositionID;		// For serialization linking, don't use this

		private int accumulator;
		private TimeSpan timeSinceLastPost = new TimeSpan(0);
		private Color color;
		private int postTimeMilis;
		private readonly Vector2 velocity;
		private readonly float fade;




		public Accumulator(World world, IComponentList componentList, Position position, Color color, int postTimeMilis, Vector2 velocity, float fade)
			: base(world, componentList)
		{
			this.componentList = componentList;
			this.position = position;

			this.color = color;
			this.postTimeMilis = postTimeMilis;
			this.velocity = velocity;
			this.fade = fade;
		}


		public Accumulator(BinaryReader br)
			: base(br)
		{
			postDeserializePositionID = br.ReadInt32();
		}


		/// <summary>
		/// After deserializing, this should be called to link this object to other objects
		/// </summary>
		/// <param name="world"></param>
		public override void PostDeserializeLink(World world)
		{
			base.PostDeserializeLink(world);

			position = world.GetComponent(postDeserializePositionID) as Position;

			if (position == null)
			{
				Debugger.Break();
			}
		}


		public void Accumulate(IQuantifiable e)
		{
			accumulator += e.Delta;
		}


		public override void Update(TimeSpan deltaTime)
		{
			base.Update(deltaTime);

			timeSinceLastPost += deltaTime;

			if(timeSinceLastPost.TotalMilliseconds > postTimeMilis)
			{
				timeSinceLastPost = timeSinceLastPost.Subtract(new TimeSpan(0, 0, 0, 0, postTimeMilis));

				if (accumulator != 0)
				{
					/*
					// Be your own position
					Position textPos = new Position(world, componentList, owningForce, position.Center, velocity);
					/*/
					// Follow the parent's position
					Position textPos = new PositionOffset(world, componentList, position, Vector2.Zero, velocity);
					//*/

					FloatingText floatingText = new FloatingText(world, componentList, textPos, accumulator.ToString("+0;-0;+0"), color, fade);
					world.HUD.AddComponent(textPos);
					world.HUD.AddComponent(floatingText);

					accumulator = 0;
				}
			}
		}

	}
}
