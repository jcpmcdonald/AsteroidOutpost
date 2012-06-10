using System;
using System.Collections.Generic;
using System.IO;
using AsteroidOutpost.Interfaces;
using AsteroidOutpost.Screens;
using C3.XNA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using XNASpriteLib;
using AsteroidOutpost.Entities.Eventing;

namespace AsteroidOutpost.Entities.Structures
{
	enum MiningState
	{
		NoPower,
		Charging,
		Mining,
		Idle
	}

	class LaserMiner : ConstructableEntity
	{
		private static Sprite sprite;
		private static float angleStep;


		MiningState state = MiningState.Idle;
		TimeSpan timeSinceLastStageChange;
		
		private float miningRate = 25;			// minerals per second while mining
		private float energyUsageRate = 5;		// power per second while mining
		private int chargeTime = 3000;			// charging time in milliseconds
		private int mineTime = 900;				// mining time in milliseconds
		
		
		// Remember which asteroid we mined last so that we can mine the next asteroid
		int miningAsteroid = -1;
		Vector2 miningOffset = Vector2.Zero;

		// Collect minerals here, and only extract a whole number of minerals from asteroids
		private double partialMineralsToExtract = 0;

		private bool firstUpdate = true;
		private bool rescanForAsteroids = true;


		private static SoundEffect miningSound;
		protected readonly List<Asteroid> nearbyAsteroids = new List<Asteroid>();
		private int miningRange;


		// Local event only
		public event Action<AccumulationEventArgs> AccumulationEvent;


		public LaserMiner(World world, IComponentList componentList, Force theowningForce, Vector2 theCenter)
			: base(world, componentList, theowningForce, theCenter, 30, 100)
		{
			Init();
			MiningRange = 90;
		}


		public LaserMiner(BinaryReader br) : base(br)
		{
			miningRange = br.ReadInt32();

			state = (MiningState)br.ReadByte();
			timeSinceLastStageChange = new TimeSpan(br.ReadInt64());

			miningRate = br.ReadSingle();
			energyUsageRate = br.ReadSingle();
			chargeTime = br.ReadInt32();
			mineTime = br.ReadInt32();

			miningAsteroid = br.ReadInt32();

			Init();
		}

		private void Init()
		{
			animator = new SpriteAnimator(sprite);
			animator.CurrentOrientation = (angleStep * GlobalRandom.Next(0, sprite.OrientationLookup.Count - 1)).ToString();
		}


		/// <summary>
		/// Serialize this Entity
		/// </summary>
		/// <param name="bw">The BinaryWriter to stream to</param>
		public override void Serialize(BinaryWriter bw)
		{
			// Always serialize the base first because we can't pick the deserialization order
			base.Serialize(bw);

			bw.Write(miningRange);

			bw.Write((byte)state);
			bw.Write(timeSinceLastStageChange.Ticks);

			bw.Write(miningRate);
			bw.Write(energyUsageRate);
			bw.Write(chargeTime);
			bw.Write(mineTime);

			bw.Write(miningAsteroid);
		}


		/// <summary>
		/// This is where all entities should do any resource loading that will be required. This will be called once per game.
		/// </summary>
		/// <param name="graphicsDevice">The graphics device</param>
		/// <param name="content">The content manager</param>
		public static void LoadContent(GraphicsDevice graphicsDevice, ContentManager content)
		{
			miningSound = content.Load<SoundEffect>(@"Sound Effects\BeamLaser");

			sprite = new Sprite(File.OpenRead(@"..\Sprites\LaserMiner.sprx"), graphicsDevice);
			angleStep = 360.0f / sprite.OrientationLookup.Count;
		}


		/// <summary>
		/// After deserializing, this should be called to link this object to other objects
		/// </summary>
		/// <param name="world"></param>
		public override void PostDeserializeLink(World world)
		{
			base.PostDeserializeLink(world);

			ScanForAsteroids();
			firstUpdate = false;
			rescanForAsteroids = false;
		}


		/// <summary>
		/// Entities should initialize their possible upgrades here
		/// </summary>
		protected override void InitializeUpgrades()
		{
			// TODO: This should be stored in an XML file somewhere. Maybe part of the sprite xml?
			Upgrade increaseMiningRate1 = new Upgrade("Upgrade Mining Rate", "Increases the number of minerals this miner extracts per second", 300, FinishedMiningRate1);
			Upgrade increaseMiningRate2 = new Upgrade("Upgrade Mining Rate 2", "Increases the number of minerals this miner extracts per second", 400, FinishedMiningRate2, increaseMiningRate1);
			Upgrade increaseMiningRate3 = new Upgrade("Upgrade Mining Rate 3", "Increases the number of minerals this miner extracts per second", 500, FinishedMiningRate3, increaseMiningRate2);

			Upgrade reduceChargeTime1 = new Upgrade("Reduce Charge Time", "Reduces the time required to charge the lasers", 300, FinishedChargeTime1);

			Upgrade increaseRange1 = new Upgrade("Increase Range", "Increases the mining range", 300, FinishedIncreaseRange);
			Upgrade increaseRange2 = new Upgrade("Increase Range 2", "Increases the mining range", 300, FinishedIncreaseRange, increaseRange1);
			Upgrade increaseRange3 = new Upgrade("Increase Range 3", "Increases the mining range", 300, FinishedIncreaseRange, increaseRange2);

			allUpgrades.AddRange(new[] { increaseMiningRate1, increaseMiningRate2, increaseMiningRate3, reduceChargeTime1, increaseRange1, increaseRange2, increaseRange3 });
		}


		/// <summary>
		/// How many minerals does this constructable take to build?
		/// </summary>
		public override int MineralsToConstruct
		{
			// TODO: Move this to an XML file
			get { return 300; }
		}

		/// <summary>
		/// Gets the name of this entity
		/// </summary>
		public override string Name
		{
			// TODO: Move this to an XML file
			get { return "Laser Miner"; }
		}

		public int MiningRange
		{
			get
			{
				return miningRange;
			}
			set
			{
				miningRange = value;
			}
		}


		private void ScanForAsteroids()
		{
			nearbyAsteroids.Clear();

			// Search for nearby asteroids (in a square)
			List<Entity> fairlyNearEntities = world.EntitiesInArea(new Rectangle((int)(Position.Center.X - MiningRange),
																				 (int)(Position.Center.Y - MiningRange),
																				 MiningRange * 2,
																				 MiningRange * 2));
			foreach (Entity entity in fairlyNearEntities)
			{
				if (entity is Asteroid && Position.Distance(entity.Position) - entity.Radius.Value < MiningRange)
				{
					Asteroid asteroid = (Asteroid)entity;

					if (!IsObstructed(asteroid))
					{
						nearbyAsteroids.Add(asteroid);
					}
				}
			}
		}


		/// <summary>
		/// Mine nearby asteroids
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
				return;
			}

			// Only search for nearby asteroids the first time that I update
			if (rescanForAsteroids)
			{
				rescanForAsteroids = false;
				ScanForAsteroids();
			}

			if(firstUpdate)
			{
				firstUpdate = false;

				// Start off in the charging state
				state = MiningState.Charging;
				timeSinceLastStageChange = TimeSpan.FromMilliseconds(0);
			}


			// How much time has passed since I last did something?
			//TimeSpan timeSinceLastStageChange = deltaTime.TotalGameTime.Subtract(lastStageChange);
			timeSinceLastStageChange += deltaTime;
			Asteroid currentAsteroid = null;
			if (miningAsteroid >= 0)
			{
				currentAsteroid = nearbyAsteroids[miningAsteroid];
			}
			
			
			// The state machine
			switch(state)
			{
			case MiningState.Charging:
			{
				if(timeSinceLastStageChange.TotalMilliseconds > chargeTime)
				{
					// Loose any partial mining progress when we start charging. If the math and upgrades are set properly, this shouldn't really come into play
					partialMineralsToExtract = 0.0;

					// I don't want to loose fractions of a second, so just subtract instead of set
					timeSinceLastStageChange = timeSinceLastStageChange.Subtract (new TimeSpan(0, 0, 0, 0, chargeTime));
					if(nearbyAsteroids.Count > 0)
					{
						StartMining();
					}
					else
					{
						state = MiningState.Idle;
					}
				}
					
				break;
			}
				
				
			case MiningState.Mining:
			{
				// Extract minerals from the asteroid
				float mineralsToExtract = miningRate * (float)deltaTime.TotalSeconds;
				float powerToUse = energyUsageRate * (float)deltaTime.TotalSeconds;

				if (currentAsteroid != null)
				{
					if (world.PowerGrid(owningForce).GetPower(this, powerToUse))
					{
						if (currentAsteroid.GetMinerals() < mineralsToExtract)
						{
							mineralsToExtract = currentAsteroid.GetMinerals();

							// Stop mining
							StartCharging();

							// and delete this asteroid from our list
							nearbyAsteroids.Remove(currentAsteroid);
							miningAsteroid--;
						}

						// Only extract a whole number of minerals from the asteroid
						partialMineralsToExtract += mineralsToExtract;
						int wholeMineralsToExtract = (int)partialMineralsToExtract;

						if (wholeMineralsToExtract > 0)
						{
							currentAsteroid.SetMinerals(currentAsteroid.GetMinerals() - wholeMineralsToExtract);
							owningForce.SetMinerals(owningForce.GetMinerals() + wholeMineralsToExtract);
							partialMineralsToExtract -= wholeMineralsToExtract;

							OnAccumulate(wholeMineralsToExtract);
						}
					}
					else
					{
						// No power, stop mining
						timeSinceLastStageChange = TimeSpan.FromMilliseconds(0);
						state = MiningState.Charging;
					}
				}

				if (timeSinceLastStageChange.TotalMilliseconds > mineTime)
				{
					StartCharging();
				}
				break;
			}
				
				
			case MiningState.Idle:
			{
				if(nearbyAsteroids.Count > 0)
				{
					state = MiningState.Charging;
				}
				break;
			}
				
			}

			base.Update(deltaTime);
		}


		protected virtual void OnAccumulate(int wholeMineralsToExtract)
		{
			if(AccumulationEvent != null)
			{
				AccumulationEvent(new AccumulationEventArgs(wholeMineralsToExtract));
			}
		}


		private void StartMining()
		{
			timeSinceLastStageChange = TimeSpan.FromMilliseconds(0);

			// Start mining
			miningSound.Play(Math.Max(1, world.Scale(0.5f)), 0, 0);
			state = MiningState.Mining;

			// Move to the next asteroid
			miningAsteroid++;
			if (miningAsteroid >= nearbyAsteroids.Count)
			{
				miningAsteroid = 0;
			}

			if (nearbyAsteroids.Count >= 1)
			{
				int randFactor = nearbyAsteroids[0].Radius.Value / 2;
				miningOffset.X = GlobalRandom.Next(-randFactor, randFactor);
				miningOffset.Y = GlobalRandom.Next(-randFactor, randFactor);
			}
		}


		private void StartCharging()
		{
			// I don't want to loose fractions of a second, so just add instead of set
			timeSinceLastStageChange = timeSinceLastStageChange.Subtract(new TimeSpan(0, 0, 0, 0, mineTime));

			state = MiningState.Charging;
		}


		/// <summary>
		/// Update this upgrading building
		/// </summary>
		/// <param name="deltaTime"></param>
		/// <returns></returns>
		protected override bool UpdateUpgrading(TimeSpan deltaTime)
		{
			timeSinceLastStageChange = TimeSpan.FromMilliseconds(0);
			state = MiningState.Idle;
			return base.UpdateUpgrading(deltaTime);
		}


		/// <summary>
		/// Draw this entity to the screen
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="scaleModifier"></param>
		/// <param name="tint"></param>
		public override void Draw(SpriteBatch spriteBatch, float scaleModifier, Color tint)
		{
			if(state == MiningState.Mining)
			{
				//Rectangle focusScreen = world.Hud.FocusScreen;
				Asteroid asteroid = nearbyAsteroids[miningAsteroid];

				float amplitude;
				if(timeSinceLastStageChange.TotalMilliseconds > 300)
				{
					amplitude = 1.0f;
				}
				else
				{
					amplitude = (float)(timeSinceLastStageChange.TotalMilliseconds / 300f);
				}

				Color color = new Color((int)((20f + world.Scale(150)) * amplitude),
										0,
										0,
										(int)((20f + world.Scale(150)) * amplitude));

				// Connect!
				spriteBatch.DrawLine(world.WorldToScreen(Position.Center + PowerLinkPointRelative),
									 world.WorldToScreen(asteroid.Position.Center + miningOffset),
									 color);
			}

			base.Draw(spriteBatch, scaleModifier * 0.6f, tint);
		}



		public List<Asteroid> NearbyAsteroids()
		{
			if (rescanForAsteroids)
			{
				rescanForAsteroids = false;
				ScanForAsteroids();
			}

			return nearbyAsteroids;
		}



		#region Upgrade Complete
		// TODO: All of these effects should be placed into an XML file
		// Thinking some more, I could even put code straight in some external file, then load and execute it directly

		private void FinishedMiningRate1(Upgrade upgrade)
		{
			miningRate += 10;
			energyUsageRate += 1;
			level++;
		}

		private void FinishedMiningRate2(Upgrade upgrade)
		{
			miningRate += 20;
			energyUsageRate += 2;
			level++;
		}
		
		private void FinishedMiningRate3(Upgrade upgrade)
		{
			miningRate += 30;
			energyUsageRate += 3;
			level++;
		}
		
		private void FinishedChargeTime1(Upgrade upgrade)
		{
			chargeTime -= 100;
			level++;
		}

		private void FinishedIncreaseRange(Upgrade upgrade)
		{
			MiningRange += 30;
			level++;

			// Initiate a re-scan of the nearby asteroids (it can't be done here because I don't have a reference to the game)
			rescanForAsteroids = true;
		}

		#endregion


		public override void GetRangeRings(ref List<Tuple<int, Color, string>> rangeRingDefinition)
		{
			rangeRingDefinition.Add(Tuple.Create(miningRange, new Color(200, 50, 50), "Mining Range"));
			base.GetRangeRings(ref rangeRingDefinition);
		}
	}
}