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

		public Accumulator(int entityID) : base(entityID)
		{
			EndingColor = new Color(0, 0, 0, 0);
		}


		public void Accumulate(IQuantifiable e)
		{
			Value += e.Delta;
		}


		public int Value { get; set; }
		public Vector2 Offset { get; set; }
		public Vector2 Velocity { get; set; }

		public Color StartingColor { get; set; }
		public Color EndingColor { get; set; }
		public float FadeTime { get; set; }
	}
}