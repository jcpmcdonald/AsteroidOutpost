using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AsteroidOutpost.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace AsteroidOutpost.Scenes
{
	internal class SceneManager : DrawableGameComponent
	{
		private AOGame game;
		private SpriteBatch spriteBatch;

		public Dictionary<String, Scene> Scenes = new Dictionary<String, Scene>();
		private Scene currentScene;

		private Vector2? lastFocusPoint;


		public SceneManager(AOGame game)
			: base(game)
		{
			this.game = game;
		}


		public void LoadScenes()
		{
			foreach (var templateFileName in Directory.EnumerateFiles(@"..\data\scenes\", "*.json"))
			{
				String json = File.ReadAllText(templateFileName);

				Scene scene = new Scene();
				JsonConvert.PopulateObject(json, scene);
				Scenes.Add(scene.Name.ToLowerInvariant(), scene);
			}
		}


		public void SetScene(String name)
		{
			currentScene = Scenes[name.ToLowerInvariant()];
		}


		protected override void LoadContent()
		{
			spriteBatch = new SpriteBatch(Game.GraphicsDevice);

			LoadScenes();

			foreach (var scene in Scenes.Values)
			{
				scene.LoadContent(Game.GraphicsDevice);
			}
		}


		public override void Update(GameTime gameTime)
		{
			if (game.World == null)
			{
				// world is not started yet, so do nothing
				return;
			}

			if (lastFocusPoint == null)
			{
				// can't really do anything
			}
			else
			{
				Vector2 delta = lastFocusPoint.Value - game.World.HUD.FocusWorldPoint;
				currentScene.Move(delta);
			}

			lastFocusPoint = game.World.HUD.FocusWorldPoint;
		}


		public override void Draw(GameTime gameTime)
		{
			spriteBatch.Begin();
			spriteBatch.GraphicsDevice.Clear(Color.Black);


			foreach (var layer in currentScene.Layers.OrderByDescending(x => x.Distance))
			{
				layer.Draw(spriteBatch, currentScene.offset);
			}
			spriteBatch.End();

			base.Draw(gameTime);
		}
	}
}
