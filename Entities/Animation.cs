using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AsteroidOutpost.Entities
{
	public class Animation
	{
		String name;
		Texture2D texture;
		int frameWidth;
		int frameHeight;
		int startX, startY;
		
		int frameCount;
		int numCols;
		
		int currentCol;
		int currentRow;
		
		bool reverseOrder;
		
		
		public Animation(String name, String imageName, int startX, int startY, int frameWidth, int frameHeight, int frameCount, int numCols)
		{
			construct(name, TextureDictionary.Get(imageName), startX, startY, frameWidth, frameHeight, frameCount, numCols, false);
		}
		public Animation(String name, String imageName, int startX, int startY, int frameWidth, int frameHeight, int frameCount, int numCols, bool reverseOrder)
		{
			construct(name, TextureDictionary.Get(imageName), startX, startY, frameWidth, frameHeight, frameCount, numCols, reverseOrder);
		}
		public Animation(String name, Texture2D image, int startX, int startY, int frameWidth, int frameHeight, int frameCount, int numCols)
		{
			construct(name, image, startX, startY, frameWidth, frameHeight, frameCount, numCols, false);
		}
		public Animation(String name, Texture2D image, int startX, int startY, int frameWidth, int frameHeight, int frameCount, int numCols, bool reverseOrder)
		{
			construct(name, image, startX, startY, frameWidth, frameHeight, frameCount, numCols, reverseOrder);
		}
		
		
		private void construct(String theName, Texture2D theImage, int theStartX, int theStartY, int theFrameWidth, int theFrameHeight, int theFrameCount, int theNumCols, bool isReverseOrder)
		{
			name = theName;
			texture = theImage;
			startX = theStartX;
			startY = theStartY;
			frameWidth = theFrameWidth;
			frameHeight = theFrameHeight;
			frameCount = theFrameCount;
			numCols = theNumCols;
			reverseOrder = isReverseOrder;
		
			currentCol = theStartX / theFrameWidth;
			currentRow = theStartY / theFrameHeight;
		}
		
		
		/// <summary>
		/// Gets the number of frames in this animation
		/// </summary>
		public int FrameCount
		{
			get
			{
				return frameCount;
			}
		}
		
		
		/// <summary>
		/// Draw the current frame of this animation
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="centreX">The world X of the animating object</param>
		/// <param name="centreY">The world Y of the animating object</param>
		public void Draw(SpriteBatch spriteBatch, int centreX, int centreY)
		{
			Draw(spriteBatch, centreX, centreY, Color.White, 1.0f);
		}


		/// <summary>
		/// Draw the current frame of this animation with a tint
		/// </summary>
		/// <param name="spriteBatch">The destination drawing surface</param>
		/// <param name="centreX">The world X of the animating object</param>
		/// <param name="centreY">The world Y of the animating object</param>
		/// <param name="color">The color channel modulation to use. Use Color.White for full color with no tinting</param>
		/// <param name="scale">The scale value</param>
		public void Draw(SpriteBatch spriteBatch, int centreX, int centreY, Color color, float scale)
		{
			spriteBatch.Draw(texture,
							 new Rectangle(centreX - (int)((frameWidth / scale / 2.0) + 0.5),
										   centreY - (int)((frameHeight / scale / 2.0) + 0.5),
										   (int)((frameWidth / scale) + 0.5),
										   (int)((frameHeight / scale) + 0.5)),
							 new Rectangle(startX + (currentCol * frameWidth),
										   startY + (currentRow * frameHeight),
										   frameWidth,
										   frameHeight),
							 color);
			/*
			spriteBatch.Draw(texture,
			                 new Rectangle((centerX - (frameWidth / 2)) - focusScreen.X,
			                               (centerY - (frameHeight / 2)) - focusScreen.Y,
			                               frameWidth,
			                               frameHeight),
			                 new Rectangle(startX + (currentCol * frameWidth),
			                               startY + (currentRow * frameHeight),
			                               frameWidth,
			                               frameHeight),
			                 color);
			*/
		}
		
		
		/// <summary>
		/// Is the current animation done?
		/// </summary>
		/// <returns>Returns true if the animation is complete, false otherwise</returns>
		public bool IsAnimationDone()
		{
			if(!reverseOrder)
			{
				return currentRow == 0;
			}
			else
			{
				return currentRow == frameCount - 1;
			}
		}
		
		
		/// <summary>
		/// Gets the name of this animation
		/// </summary>
		public String Name
		{
			get
			{
				return name;
			}
		}
		
		
		/// <summary>
		/// Gets or Sets the Column
		/// </summary>
		public int Col
		{
			get
			{
				return currentCol;
			}
			set
			{
				currentCol = value % numCols;
				while(currentCol < 0)
				{
					currentCol += numCols;
				}
			}
		}
		
		
		/// <summary>
		/// Gets or Sets the Row
		/// </summary>
		public int Row
		{
			get
			{
				if(!reverseOrder)
				{
					return currentRow;
				}
				else
				{
					return frameCount - currentRow;
				}
			}
			set
			{
				if(!reverseOrder)
				{
					currentRow = value;
				}
				else
				{
					currentRow = frameCount - 1 - value;
				}
			}
		}
		
		
		/// <summary>
		/// Resets the Row back to the start
		/// </summary>
		public void ResetRow()
		{
			Row = 0;
		}
		
		
		/// <summary>
		/// Advances to the next animation frame
		/// </summary>
		public void AdvanceFrame()
		{
			if(!reverseOrder)
			{
				currentRow ++;
			}
			else
			{
				currentRow --;
			}
			currentRow %= frameCount;
			while(currentRow < 0)
			{
				currentRow += frameCount;
			}
		}
		
		
		public int FrameWidth
		{
			get
			{
				return frameWidth;
			}
		}
		
		public int FrameHeight
		{
			get
			{
				return frameHeight;
			}
		}
	}
}