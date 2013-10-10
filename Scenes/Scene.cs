using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AsteroidOutpost.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace AsteroidOutpost.Scenes
{
	public class Scene
	{
		public String Name { get; set; }
		public Color BackgroundColor { get; set; }
		public List<Layer> Layers = new List<Layer>();

		public Vector2 offset = Vector2.Zero;


		internal void LoadContent(GraphicsDevice graphicsDevice)
		{
			foreach (var layer in Layers)
			{
				layer.LoadContent(graphicsDevice);
			}
		}

		public void Move(Vector2 delta)
		{
			offset += delta;
		}
	}
}
