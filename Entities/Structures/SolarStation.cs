using System.IO;
using AsteroidOutpost.Entities.Eventing;
using AsteroidOutpost.Interfaces;
using AsteroidOutpost.Networking;
using AsteroidOutpost.Screens;
using C3.XNA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using XNASpriteLib;

namespace AsteroidOutpost.Entities.Structures
{
	class SolarStation : ConstructableEntity, IPowerProducer
	{
		private static Sprite sprite;
		private static float angleStep;


		private readonly float[] basePowerProductionRate = new[] { 10.0f, 30.0f, 100.0f };		// Per second
		private readonly float[] baseMaxPower = new[] { 70.0f, 200.0f, 600.0f };

		private float currentPower;


		[EventReplication(EventReplication.ServerToClients)]
		public event Action<EntityPowerLevelChangedEventArgs> PowerLevelChangedEvent;


		/// <summary>
		/// Create a new Solar Station
		/// </summary>
		/// <param name="theGame">A reference to the game</param>
		/// <param name="componentList"></param>
		/// <param name="theowningForce"></param>
		/// <param name="theCenter"></param>
		public SolarStation(AsteroidOutpostScreen theGame, IComponentList componentList, Force theowningForce, Vector2 theCenter)
			: base(theGame, componentList, theowningForce, theCenter, 45, 250)
		{
			Init();
		}


		public SolarStation(BinaryReader br) : base (br)
		{
			currentPower = br.ReadSingle();

			Init();
		}

		private void Init()
		{
			PowerLinkPointRelative = new Vector2(-1, -13);
			ConductsPower = true;
			ProducesPower = true;

			animator = new SpriteAnimator(sprite);
			animator.CurrentSet = "Level " + level;
			animator.CurrentOrientation = (angleStep * GlobalRandom.Next(0, sprite.OrientationLookup.Count - 1)).ToString();
		}


		/// <summary>
		/// This is where all entities should do any resource loading that will be required. This will be called once per game.
		/// </summary>
		/// <param name="spriteBatch">The sprite batch</param>
		/// <param name="content">The content manager</param>
		public static void LoadContent(SpriteBatch spriteBatch, ContentManager content)
		{
			sprite = new Sprite(File.OpenRead(@"..\Sprites\SolarStation.sprx"), spriteBatch.GraphicsDevice);
			//angleStep = 360.0f / sprite.OrientationLookup.Count;
			angleStep = 11.25f;
		}


		/// <summary>
		/// Entities should initialize their possible upgrades here
		/// </summary>
		protected override void InitializeUpgrades()
		{
			Upgrade level2 = new Upgrade("Level 2", "Upgrade this solar tower to level 2. This increases it's power production rate and the maximum power this unit can store.", 400, FinishedUpgradingToLevel2);
			Upgrade level3 = new Upgrade("Level 3", "Upgrade this solar tower to level 3. This increases it's power production rate and the maximum power this unit can store.", 600, FinishedUpgradingToLevel3, level2);
			allUpgrades.Add(level2);
			allUpgrades.Add(level3);
		}


		/// <summary>
		/// Serialize this Entity
		/// </summary>
		/// <param name="bw">The BinaryWriter to stream to</param>
		public override void Serialize(BinaryWriter bw)
		{
			// Always serialize the base first because we can't pick the deserialization order
			base.Serialize(bw);

			bw.Write(currentPower);
		}


		public override int Level
		{
			get{ return level; }
			set
			{
				if(value < 1)
				{
					level = 1;
				}
				else if(value > 3)
				{
					level = 3; 
				}
				else
				{
					level = value;
				}
				//animations.SetAnimation("SolarLevel" + level);
				animator.CurrentSet = "Level " + level;
			}
		}
		
		
		
		/// <summary>
		/// Updates this Entity
		/// </summary>
		/// <param name="deltaTime">The current game time</param>
		public override void Update(TimeSpan deltaTime)
		{
			if(IsConstructing)
			{
				UpdateConstructing(deltaTime);
				return;
			}
			else if(IsUpgrading)
			{
				UpdateUpgrading(deltaTime);

				// Returning here will prevent this solar station from producing power during
				// an upgrade. If we return here, we should prevent upgrading a sole power station
				//return;
			}
			
			
			//animations.Update(deltaTime);

			if (theGame.IsServer)
			{
				if (currentPower < MaxPower)
				{
					// Produce power
					SetCurrentPower(currentPower + PowerProductionRate * (float)deltaTime.TotalSeconds);
				}
			}

			base.Update(deltaTime);
		}


		/// <summary>
		/// Draw this entity to the screen
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="scaleModifier"></param>
		/// <param name="tint"></param>
		public override void Draw(SpriteBatch spriteBatch, float scaleModifier, Color tint)
		{
			base.Draw(spriteBatch, 0.7f, tint);
			
			if(!isConstructing)
			{

				double percentFull = currentPower / MaxPower;
				Color color = Color.Green;
				if(percentFull < 0.20)
				{
					color = Color.Red;
				}
				else if(percentFull < 0.40)
				{
					color = Color.Yellow;
				}


				/*
				// Draw one bar for each 10%
				int numColorBars = (int)(percentFull * 10.0);
				
				for(int i = 0; i < 10; i++)
				{
					if(i == numColorBars)
					{
						color = Color.Gray;
					}
					
					//spriteBatch.GraphicsDevice. .FillRectangle(brush, X - focusScreen.X, Y + Height - (6 * (i+1)) - focusScreen.Y, 5, 5);
					SimpleShapes.FillRectangle(spriteBatch,
										new Vector2(0, 0),
										new Rectangle(Left - 6 - theGame.Hud.FocusScreen.X,
												Top + Height - (6 * (i+1)) - theGame.Hud.FocusScreen.Y,
												5,
												5),
										0,
										color); 
				}
				
				/*/ // OR

				// Draw an arc

				float startingAngle = 270 - ((MaxPower / baseMaxPower[2]) * 135f);
				float degrees = (MaxPower / baseMaxPower[2]) * 270f;

				if(percentFull > 0.1)
				{
					spriteBatch.DrawArc(theGame.WorldToScreen(Position.Center),
					                    theGame.Scale(Radius.Value + 5),
					                    40,
										startingAngle,
										degrees,
					                    ColorPalette.ApplyTint(Color.Gray, tint),
					                    theGame.Scale(5.0f));

					spriteBatch.DrawArc(theGame.WorldToScreen(Position.Center),
					                    theGame.Scale(Radius.Value + 5),
					                    40,
										startingAngle,
										(float)(degrees * percentFull),
					                    ColorPalette.ApplyTint(color, tint),
					                    theGame.Scale(5.0f));
				}
				else
				{
					spriteBatch.DrawArc(theGame.WorldToScreen(Position.Center),
					                    theGame.Scale(Radius.Value + 5),
					                    40,
										startingAngle,
					                    degrees,
					                    ColorPalette.ApplyTint(Color.Red, tint),
					                    theGame.Scale(5.0f));
				}
				//*/
			}
		}
		
		
		/// <summary>
		/// The maximum amount of power that this solar station can hold
		/// </summary>
		private float MaxPower
		{
			get
			{
				return baseMaxPower[level - 1];
			}
		}
		
		
		/// <summary>
		/// The rate at which this solar station can produce power at, in power units per second
		/// </summary>
		private float PowerProductionRate
		{
			get
			{
				return basePowerProductionRate[level - 1];
			}
		}


		public override int MineralsToConstruct
		{
			get { return 300; }
		}

		public override string Name
		{
			get { return "Solar Station"; }
		}


		public float AvailablePower
		{
			get
			{
				if(IsConstructing)
				{
					return 0;
				}
				else
				{
					return currentPower;
				}
			}
		}

		public bool GetPower(float amount)
		{
			if (AvailablePower >= amount)
			{
				SetCurrentPower(currentPower - amount);
				return true;
			}
			else
			{
				return false;
			}
		}


		public void SetCurrentPower(float value)
		{
			int powerBefore = (int)currentPower;
			currentPower = MathHelper.Clamp(value, 0, MaxPower);

			// Tell all my friends
			if (PowerLevelChangedEvent != null)
			{
				int delta = (int)currentPower - powerBefore;
				if (delta != 0)
				{
					PowerLevelChangedEvent(new EntityPowerLevelChangedEventArgs(this, currentPower, delta));
				}
			}
		}
		
		
		public void FinishedUpgradingToLevel2(Upgrade u)
		{
			Level = 2;
		}
		
		public void FinishedUpgradingToLevel3(Upgrade u)
		{
			Level = 3;
		}
	}
}