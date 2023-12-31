﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using AsteroidOutpost.Components;
using AsteroidOutpost.Entities;
using AsteroidOutpost.Extensions;
using AsteroidOutpost.Screens;
using C3.XNA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace AsteroidOutpost.Systems
{
	class LaserMinerSystem : DrawableGameComponent
	{
		private SpriteBatch spriteBatch;
		private ParticleEffectManager particleEffectManager;
		private readonly World world;
		private readonly PowerGridSystem powerGridSystem;

		private static SoundEffect miningSound;

		public LaserMinerSystem(AOGame game, World world, PowerGridSystem powerGridSystem)
			: base(game)
		{
			spriteBatch = new SpriteBatch(game.GraphicsDevice);
			this.world = world;
			this.powerGridSystem = powerGridSystem;
			this.particleEffectManager = game.ParticleEffectManager;
		}


		/// <summary>
		/// This is where all entities should do any resource loading that will be required. This will be called once per game.
		/// </summary>
		/// <param name="graphicsDevice">The graphics device</param>
		/// <param name="content">The content manager</param>
		protected override void LoadContent()
		{
			miningSound = Game.Content.Load<SoundEffect>(@"Sound Effects\BeamLaser");
			base.LoadContent();
		}


		public override void Update(GameTime gameTime)
		{
			if (world.Paused) { return; }

			foreach (var laserMiner in  world.GetComponents<LaserMiner>())
			{
				
				if (laserMiner.RescanForAsteroids)
				{
					Constructing constructable = world.GetNullableComponent<Constructing>(laserMiner);
					if (constructable == null || !constructable.IsBeingPlaced)
					{
						laserMiner.RescanForAsteroids = false;
						laserMiner.nearbyAsteroids.Clear();
						laserMiner.nearbyAsteroids.AddRange(ScanForAsteroids(laserMiner));
					}
				}

				if (laserMiner.FirstUpdate)
				{
					Constructing constructable = world.GetNullableComponent<Constructing>(laserMiner);
					if (constructable != null)
					{
						continue;
					}

					laserMiner.FirstUpdate = false;

					// Start off in the charging state
					laserMiner.State = MiningState.Charging;
					laserMiner.TimeSinceLastStageChange = TimeSpan.FromMilliseconds(0);

					// Attach to the accumulator
					Accumulator relatedAccumulator = world.GetNullableComponent<Accumulator>(laserMiner);
					if(relatedAccumulator != null)
					{
						laserMiner.AccumulationEvent += relatedAccumulator.Accumulate;
					}
				}
				else
				{
					laserMiner.TimeSinceLastStageChange += gameTime.ElapsedGameTime;
				}


				// The state machine
				switch (laserMiner.State)
				{
				case MiningState.Charging:
				{
					UpdateCharging(gameTime, laserMiner);
					break;
				}


				case MiningState.Mining:
				{
					UpdateMining(gameTime, laserMiner);
					break;
				}


				case MiningState.Idle:
				{
					if (laserMiner.nearbyAsteroids.Count > 0)
					{
						laserMiner.State = MiningState.Charging;
					}
					break;
				}
				} // End Switch


			}

			base.Update(gameTime);
		}


		private void UpdateCharging(GameTime gameTime, LaserMiner laserMiner)
		{
			if (laserMiner.TimeSinceLastStageChange.TotalSeconds > laserMiner.ChargeTime)
			{
				// Loose any partial mining progress when we start charging. If the math and upgrades are set properly, this shouldn't really come into play
				laserMiner.PartialMineralsToExtract = 0.0;

				// I don't want to loose fractions of a second, so just subtract instead of set
				laserMiner.TimeSinceLastStageChange = laserMiner.TimeSinceLastStageChange.Subtract(new TimeSpan( 0, 0, 0, 0, (int)(laserMiner.ChargeTime * 1000f)));
				if (laserMiner.nearbyAsteroids.Count > 0)
				{
					StartMining(laserMiner);
				}
				else
				{
					laserMiner.State = MiningState.Idle;
				}
			}
		}


		private void UpdateMining(GameTime gameTime, LaserMiner laserMiner)
		{
			// Extract minerals from the asteroid
			float mineralsToExtract = laserMiner.MiningRate * (float)gameTime.ElapsedGameTime.TotalSeconds;
			float powerToUse = laserMiner.EnergyUsageRate * (float)gameTime.ElapsedGameTime.TotalSeconds;


			Minable currentAsteroid = null;
			if (laserMiner.MiningAsteroid >= 0)
			{
				currentAsteroid = laserMiner.nearbyAsteroids[laserMiner.MiningAsteroid];
			}


			if (currentAsteroid != null)
			{
				Force owningForce = world.GetOwningForce(laserMiner);
				if (powerGridSystem.GetPower(laserMiner, powerToUse))
				{
					if (currentAsteroid.Minerals < mineralsToExtract)
					{
						mineralsToExtract = currentAsteroid.Minerals;

						// Stop mining
						StartCharging(laserMiner);

						// and delete this asteroid from our list
						laserMiner.nearbyAsteroids.Remove(currentAsteroid);
						laserMiner.MiningAsteroid--;
					}

					// Only extract a whole number of minerals from the asteroid
					laserMiner.PartialMineralsToExtract += mineralsToExtract;
					int wholeMineralsToExtract = (int)laserMiner.PartialMineralsToExtract;

					if (wholeMineralsToExtract > 0)
					{
						currentAsteroid.Minerals -= wholeMineralsToExtract;
						owningForce.SetMinerals(owningForce.GetMinerals() + wholeMineralsToExtract);
						laserMiner.PartialMineralsToExtract -= wholeMineralsToExtract;

						laserMiner.OnAccumulate(wholeMineralsToExtract);
					}
				}
				else
				{
					// No power, stop mining
					laserMiner.TimeSinceLastStageChange = TimeSpan.FromMilliseconds(0);
					laserMiner.State = MiningState.Charging;
				}
			}

			if (laserMiner.TimeSinceLastStageChange.TotalSeconds > laserMiner.MineTime)
			{
				StartCharging(laserMiner);
			}
		}


		private List<Minable> ScanForAsteroids(LaserMiner laserMiner)
		{
			//laserMiner.nearbyAsteroids.Clear();
			List<Minable> rv = new List<Minable>();

			// Search for nearby asteroids (in a square)
			Position laserPosition = world.GetComponent<Position>(laserMiner);
			List<int> fairlyNearEntities = world.EntitiesInArea(laserPosition.Center, laserMiner.MiningRange);

			foreach (int entity in fairlyNearEntities)
			{
				Minable minable = world.GetNullableComponent<Minable>(entity);
				if (minable != null)
				{
					Position minablePosition = world.GetComponent<Position>(entity);
					if(laserPosition.Distance(minablePosition) < laserMiner.MiningRange)
					{
						if (!MathHelperEx.IsObstructed(world, laserPosition, minablePosition))
						{
							rv.Add(minable);
						}
					}
				}
			}

			return rv;
		}


		private void StartMining(LaserMiner laserMiner)
		{
			laserMiner.TimeSinceLastStageChange = TimeSpan.FromMilliseconds(0);

			// Start mining
			//miningSound.Play(Math.Max(1, world.Scale(0.5f)) * 0.3f, 0, 0);
			laserMiner.State = MiningState.Mining;

			// Move to the next asteroid
			laserMiner.MiningAsteroid++;
			if (laserMiner.MiningAsteroid >= laserMiner.nearbyAsteroids.Count)
			{
				laserMiner.MiningAsteroid = 0;
			}

			if (laserMiner.nearbyAsteroids.Count >= 1)
			{
				Position asteroidPos = world.GetComponent<Position>(laserMiner.nearbyAsteroids[0]);
				int randFactor = asteroidPos.Radius / 2;
				laserMiner.MiningDestinationOffset = new Vector2(GlobalRandom.Next(-randFactor, randFactor),
				                                                 GlobalRandom.Next(-randFactor, randFactor));
			}
		}


		private void StartCharging(LaserMiner laserMiner)
		{
			// I don't want to loose fractions of a second, so just add instead of set
			laserMiner.TimeSinceLastStageChange = laserMiner.TimeSinceLastStageChange.Subtract(new TimeSpan(0, 0, 0, 0, (int)(laserMiner.MineTime * 1000f)));

			laserMiner.State = MiningState.Charging;
		}


		public override void Draw(GameTime gameTime)
		{
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);

			foreach (var laserMiner in world.GetComponents<LaserMiner>())
			{
				if (laserMiner.FirstUpdate)
				{
					Constructing constructable = world.GetNullableComponent<Constructing>(laserMiner);
					if (constructable != null && constructable.IsBeingPlaced)
					{
						// Draw the links to nearby minables
						Color color = new Color((int)(20f + world.Scale(150)),
						                        0,
						                        0,
						                        (int)(20f + world.Scale(150)));

						Position laserMinerPosition = world.GetComponent<Position>(laserMiner);
						List<Minable> minables = ScanForAsteroids(laserMiner);
						foreach (var minable in minables)
						{
							Position minablePosition = world.GetComponent<Position>(minable);
							Vector2 start = world.WorldToScreen(laserMinerPosition.Center + laserMiner.MiningSourceOffset);
							Vector2 end = world.WorldToScreen(minablePosition.Center + laserMiner.MiningDestinationOffset);
							spriteBatch.DrawLaser(start, end, color, world.Scale, 1f);
						}


						// Draw link radius
						DrawLinkRadius(spriteBatch, laserMiner, laserMinerPosition);
					}
				}
				else if (laserMiner.State == MiningState.Mining)
				{
					//Asteroid asteroid = nearbyAsteroids[miningAsteroid];

					float amplitude;
					if (laserMiner.TimeSinceLastStageChange.TotalSeconds > 0.3f)
					{
						amplitude = 1.0f;
					}
					else
					{
						amplitude = (float)(laserMiner.TimeSinceLastStageChange.TotalSeconds / 0.3f);
					}

					Color color = new Color((int)((20f + world.Scale(150)) * amplitude),
					                        0,
					                        0,
					                        (int)((20f + world.Scale(150)) * amplitude));

					// Connect!
					Position laserMinerPosition = world.GetComponent<Position>(laserMiner);
					Position minablePosition = world.GetComponent<Position>(laserMiner.nearbyAsteroids[laserMiner.MiningAsteroid]);

					Vector2 start = world.WorldToScreen(laserMinerPosition.Center + laserMiner.MiningSourceOffset);
					Vector2 end = world.WorldToScreen(minablePosition.Center + laserMiner.MiningDestinationOffset);

					const float timeToBeamContact = 0.25f;
					if (laserMiner.TimeSinceLastStageChange.TotalSeconds > timeToBeamContact)
					{
						// Draw the full beam and a particle drilling effect at the end
						spriteBatch.DrawLaser(start, end, color, world.Scale, 1f);

						particleEffectManager.Trigger("Mining", minablePosition.Center + laserMiner.MiningDestinationOffset);
					}
					else
					{
						// Draw a partial beam
						float percent = ((float)laserMiner.TimeSinceLastStageChange.TotalSeconds / timeToBeamContact);
						spriteBatch.DrawLaser(start, end, color, world.Scale, 0.8f, percent);
					}

				}

			}

			spriteBatch.End();

			base.Draw(gameTime);
		}


		private void DrawLinkRadius(SpriteBatch spriteBatch, LaserMiner laserMiner, Position laserMinerPosition)
		{
			spriteBatch.DrawEllipse(world.WorldToScreen(laserMinerPosition.Center),
			                        world.Scale(laserMiner.MiningRange),
			                        Color.Red);
		}
	}
}
