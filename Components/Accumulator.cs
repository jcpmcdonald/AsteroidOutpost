using System;
using System.Diagnostics;
using System.IO;
using AsteroidOutpost.Entities.Eventing;
using AsteroidOutpost.Interfaces;
using AsteroidOutpost.Screens;
using Microsoft.Xna.Framework;

namespace AsteroidOutpost.Components
{
	public class Accumulator : Component
	{

		public Accumulator(int entityID) : base(entityID) {}
		public Accumulator(int entityID,
		                   Vector2 offset,
		                   Vector2 velocity,
		                   Color color,
		                   float fadeRate = 120)
			: base(entityID)
		{
			this.Offset = offset;
			this.Velocity = velocity;
			this.Color = color;
			this.FadeRate = fadeRate;
		}




		public void Accumulate(IQuantifiable e)
		{
			Value += e.Delta;
		}


		public int Value { get; set; }
		public Vector2 Offset { get; set; }
		public Vector2 Velocity { get; private set; }
		public float FadeRate { get; set; }
		public Color Color { get; set; }
	}
}