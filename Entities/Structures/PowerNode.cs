using System.IO;
using AsteroidOutpost.Interfaces;
using AsteroidOutpost.Screens;
using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using XNASpriteLib;

namespace AsteroidOutpost.Entities.Structures
{
	class PowerNode : ConstructableEntity
	{
		private static Sprite sprite;
		private static float angleStep;


		public PowerNode(World world, IComponentList componentList, Force theowningForce, Vector2 theCenter)
			: base(world, componentList, theowningForce, theCenter, 15, 50)
		{
			Init();
		}


		public PowerNode(BinaryReader br) : base(br)
		{
			Init();
		}

		private void Init()
		{
			ConductsPower = true;
			PowerLinkPointRelative = new Vector2(0, -16);

			animator = new SpriteAnimator(sprite);
			animator.CurrentOrientation = (angleStep * GlobalRandom.Next(0, sprite.OrientationLookup.Count - 1)).ToString();
			//animator.CurrentOrientation = (angleStep * 1).ToString();
		}


		/// <summary>
		/// This is where all entities should do any resource loading that will be required. This will be called once per game.
		/// </summary>
		/// <param name="graphicsDevice">The graphics device</param>
		/// <param name="content">The content manager</param>
		public static void LoadContent(GraphicsDevice graphicsDevice, ContentManager content)
		{
			sprite = new Sprite(File.OpenRead(@"..\Sprites\PowerNode.sprx"), graphicsDevice);
			angleStep = 360.0f / sprite.OrientationLookup.Count;
		}


		public override int MineralsToConstruct
		{
			get { return 50; }
		}

		public override string Name
		{
			get { return "Power Node"; }
		}

		protected override void InitializeUpgrades()
		{
			// No upgrades
		}

		public override void Update(TimeSpan deltaTime)
		{
			if(IsConstructing)
			{
				UpdateConstructing(deltaTime);
				//return;
			}
			else if(IsUpgrading)
			{
				UpdateUpgrading(deltaTime);
				//return;
			}

			base.Update(deltaTime);
		}

		public override void Draw(SpriteBatch spriteBatch, float scaleModifier, Color tint)
		{
			base.Draw(spriteBatch, scaleModifier * 0.3f, tint);
		}
	}
}