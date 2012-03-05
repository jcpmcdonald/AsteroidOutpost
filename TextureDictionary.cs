using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace AsteroidOutpost
{
	class TextureDictionary
	{
		private static readonly List<PrecachedImage> images = new List<PrecachedImage>();
		private static ContentManager content;

		~TextureDictionary()
		{
			images.Clear();
		}
		
		// returns the index of the image with the given name
		// returns -1 if the image name is not found
		private static int indexOfImage(String name)
		{
			int index = -1;
			
			for(int i = 0; i < images.Count && index == -1; i++)
			{
				PrecachedImage pre = images[i];
				if(pre.Name.ToLower().Equals(name.ToLower()))
				{
					index = i;
				}
			}
			
			return index;
		}
		
		
		public static void Add(String contentPath)
		{
			Add(contentPath, contentPath);
		}
		
		// adds the image to the selection of precached images with a custom name
		public static void Add(String contentPath, String name)
		{
			if(indexOfImage(name) == -1)
			{
				images.Add(new PrecachedImage(content, contentPath, name));
			}
		}
		
		
		public static Texture2D Get(String name)
		{
			int i = indexOfImage(name);
		
			if(i == -1)
			{
				// if the image they want does not exist, error
				throw new Exception("Image named '" + name + "' not found!");
			}
			else
			{
				return images[i].Texture;
			}
		}
		
		public static void SetContent(ContentManager theContent)
		{
			content = theContent;
		}
	}

	class PrecachedImage{
		private readonly String name;
		private readonly Texture2D texture;
		
		public PrecachedImage(ContentManager content, String contentPath, String name)
		{
			this.name = name;
			texture = content.Load<Texture2D>(contentPath);
		}
		
		~PrecachedImage()
		{
			texture.Dispose();
		}
		
		public Texture2D Texture
		{
			get
			{
				return texture;
			}
		}
		
		public String Name
		{
			get
			{
				return name;
			}
		}
	}
}