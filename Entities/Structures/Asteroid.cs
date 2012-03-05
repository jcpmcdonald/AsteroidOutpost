using System;
using System.Diagnostics;
using System.IO;
using AsteroidOutpost.Entities.Eventing;
using AsteroidOutpost.Networking;
using AsteroidOutpost.Screens;
using C3.XNA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using XNASpriteLib;

namespace AsteroidOutpost.Entities.Structures
{
	class Asteroid : Entity
	{
		private static Sprite sprite;
		private static float angleStep;

		private float z;	// Testing some depth stuff

		readonly int[] asteroidSizeValueIndex = new[]{ 200, 400, 800, 1600, 3200, 6400, 12800, 25600, 51200 };
		
		int currentMinerals;
		readonly int startingMinerals;

		//private readonly int size;
		private readonly float sizePercent;
		private readonly int style;

		[EventReplication(EventReplication.ServerToClients)]
		public event Action<EntityMineralValueChangedEventArgs> MineralsChangedEvent;


		public Asteroid(AsteroidOutpostScreen theGame, IComponentList componentList, Force theForce, Vector2 theCenter, int value)
			: base(theGame, componentList, theForce, theCenter, 1000)
		{
			startingMinerals = value;
			currentMinerals = startingMinerals;

			// Create an asteroid with an appropriate size
			int radius = 1;
			foreach(int indexedValue in asteroidSizeValueIndex)
			{
				if(startingMinerals > indexedValue)
				{
					radius++;
				}
			}
			radius = radius * 10;
			sizePercent = radius / 100.0f;
			Radius = new Radius(theGame, componentList, owningForce, Position, (int)(sizePercent * 25));
			componentList.AddComponent(Radius);
			
			// And select a random asteroid type
			style = GlobalRandom.Next(1, 4);

			z = (float)(GlobalRandom.NextDouble() * 10.0f) - 5f;

			Init();
		}


		/// <summary>
		/// Initializes this Asteroid from a BinaryReader
		/// </summary>
		/// <param name="br">The BinaryReader to Deserialize from</param>
		public Asteroid(BinaryReader br) : base(br)
		{
			currentMinerals = br.ReadInt32();
			startingMinerals = br.ReadInt32();
			style = br.ReadInt32();
			sizePercent = br.ReadSingle();

			Init();
		}


		private void Init()
		{
			animator = new SpriteAnimator(sprite);
			animator.CurrentSet = "Asteroid " + style;
			animator.CurrentOrientation = (angleStep * GlobalRandom.Next(0, sprite.OrientationLookup.Count - 1)).ToString();
		}


		/// <summary>
		/// This is where all entities should do any resource loading that will be required. This will be called once per game.
		/// </summary>
		/// <param name="spriteBatch">The sprite batch</param>
		/// <param name="content">The content manager</param>
		public static void LoadContent(SpriteBatch spriteBatch, ContentManager content)
		{
			sprite = new Sprite(File.OpenRead(@"..\Sprites\Asteroids.sprx"), spriteBatch.GraphicsDevice);
			angleStep = 360.0f / sprite.OrientationLookup.Count;
		}


		/// <summary>
		/// Serialize this Asteroid
		/// </summary>
		/// <param name="bw">The BinaryWriter to stream to</param>
		public override void Serialize(BinaryWriter bw)
		{
			// Always serialize the base first because we can't pick the deserialization order
			base.Serialize(bw);

			bw.Write(currentMinerals);
			bw.Write(startingMinerals);
			bw.Write(style);
			bw.Write(sizePercent);
		}


		public override string Name
		{
			get { return "Asteroid " + z; }
		}


		public void SetMinerals(int value)
		{
			SetMinerals(value, theGame.IsServer);
		}

		public void SetMinerals(int value, bool authoritative)
		{
			if (!authoritative)
			{
				return;
			}


			bool changed = (currentMinerals != value);
			currentMinerals = value;
			if (currentMinerals < 0) { currentMinerals = 0; }

			if(changed && MineralsChangedEvent != null)
			{
				MineralsChangedEvent(new EntityMineralValueChangedEventArgs(this, currentMinerals));
			}
		}

		public int GetMinerals()
		{
			return currentMinerals;
		}
		
		public double StartingMinerals
		{
			get{ return startingMinerals; }
		}


		/// <summary>
		/// Draw this entity to the screen
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="scaleModifier"></param>
		/// <param name="tint"></param>
		public override void Draw(SpriteBatch spriteBatch, float scaleModifier, Color tint)
		{
			// Note: Not sure I entirely like how this effect turns out, think about looking at it again. Maybe after a model change
			Color mineralTint = Color.White;
			if(currentMinerals == 0.0)
			{
				mineralTint = Color.DarkGray;
			}
			else if ((float)currentMinerals / startingMinerals < 0.20)
			{
				float percent = (float)(currentMinerals / (startingMinerals * 0.2));
				mineralTint = new Color((percent * 86.0f) + 169, (percent * 86.0f) + 169, (percent * 86.0f) + 169, 255);
			}

			//*
			base.Draw(spriteBatch, sizePercent * scaleModifier * 0.5f, ColorPalette.ApplyTint(tint, mineralTint));
			/*/
			// Test a fake 3D effect with my asteroids
			if (animator != null)
			{
				animator.Draw(spriteBatch, WorldToScreen(Position.Center), 0, (sizePercent * scaleModifier * 0.5f) / theGame.ScaleFactor, tint);
			}
			//*/

		}

		
		// These are here to test a possible change to use 3D coordinates. I must say it looks pretty sweet
		public Vector2 WorldToScreen(Vector2 point)
		{
			return WorldToScreen(point.X, point.Y);
		}
		public Vector2 WorldToScreen(float x, float y)
		{
			float deltaX = x - theGame.HUD.FocusWorldPoint.X;
			float deltaY = y - theGame.HUD.FocusWorldPoint.Y;

			deltaX = deltaX / theGame.ScaleFactor * (float)Math.Sqrt(3);
			deltaY = deltaY / theGame.ScaleFactor;

			// Z CHANGES
			float percentOffHorizontal = (deltaX / (theGame.Width / 2f));
			deltaX += percentOffHorizontal * theGame.Scale(z * 15f);

			float percentOffVertical = (deltaY / (theGame.Height / 2f));
			deltaY += percentOffVertical * theGame.Scale(z * 15f);
			// END Z CHANGES

			return new Vector2(theGame.Width / 2f + deltaX, theGame.Height / 2f + deltaY);
		}
	}
}