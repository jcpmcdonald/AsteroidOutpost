using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AsteroidOutpost.Entities
{
	public class AnimationSet
	{
		readonly List<Animation> animations = new List<Animation>();
		Animation currentAnimation;
		
		TimeSpan timeSinceLastFrame = new TimeSpan(0);
		
				
		public AnimationSet()
		{
			framesPerSecond = 10.0;
		}


		private double framesPerSecond;
		public double FramesPerSecond{
			get
			{
				return framesPerSecond;
			}
			set
			{
				framesPerSecond = value;
			}
		}
		
		public void AddAnimation(Animation anim)
		{
			animations.Add(anim);
		}
	
		public void SetAnimation(String animationName)
		{
			foreach(Animation anim in animations)
			{
				if(anim.Name == animationName)
				{
					// we want to remember the old col because it is typically the direction
					int oldCol = (currentAnimation == null) ? 0 : currentAnimation.Col;
					currentAnimation = anim;
					currentAnimation.Col = oldCol;
					// and reset the row so we are ready to start anew
					currentAnimation.ResetRow();
					
					// We found what we were looking for, stop
					break;
				}
			}
		}
	
	
		public bool IsAnimationDone()
		{
			// TODO: Investigate why I needed "currAdvanceFrameCounter - 1.0 < 0" in the following:
			return (currentAnimation == null ||
			        (currentAnimation.IsAnimationDone() /* && currAdvanceFrameCounter - 1.0 < 0 */));
		}


		public void Draw(SpriteBatch spriteBatch, Vector2 centre)
		{
			Draw(spriteBatch, (int)(centre.X + 0.5), (int)(centre.Y + 0.5), Color.White, 1.0f);
		}

		public void Draw(SpriteBatch spriteBatch, Vector2 centre, Color color)
		{
			Draw(spriteBatch, (int)(centre.X + 0.5), (int)(centre.Y + 0.5), color, 1.0f);
		}

		public void Draw(SpriteBatch spriteBatch, Vector2 centre, Color color, float scale)
		{
			Draw(spriteBatch, (int)(centre.X + 0.5), (int)(centre.Y + 0.5), color, scale);
		}

		public void Draw(SpriteBatch spriteBatch, int centreX, int centreY)
		{
			Draw(spriteBatch, centreX, centreY, Color.White, 1.0f);
		}

		public void Draw(SpriteBatch spriteBatch, int centreX, int centreY, Color color)
		{
			Draw(spriteBatch, centreX, centreY, color, 1.0f);
		}

		public void Draw(SpriteBatch spriteBatch, int centreX, int centreY, Color color, float scale)
		{
			if(currentAnimation != null)
			{
				currentAnimation.Draw(spriteBatch, centreX, centreY, color, scale);
			}
		}

		public void Update(TimeSpan deltaTime)
		{
			if(currentAnimation != null)
			{
				double millisecondsBetweenFrames = 1000.0 / framesPerSecond;
				timeSinceLastFrame = timeSinceLastFrame.Add(deltaTime);
				if(timeSinceLastFrame.TotalMilliseconds > millisecondsBetweenFrames)
				{
					currentAnimation.AdvanceFrame();
					timeSinceLastFrame = timeSinceLastFrame.Subtract(new TimeSpan(0, 0, 0, 0, (int)millisecondsBetweenFrames));
				}
				/*
				currAdvanceFrameCounter += 1.0;
				if(currAdvanceFrameCounter >= FramesPerSecond)
				{
					currAdvanceFrameCounter -= (FramesPerSecond * gameTime.ElapsedGameTime.TotalSeconds);
					currentAnimation.AdvanceFrame();
				}
				*/
			}
		}
	
		public int Col
		{
			get
			{
				if(currentAnimation != null)
				{
					return currentAnimation.Col;
				}
				return -1;
			}
			set
			{
				if(currentAnimation != null)
				{
					currentAnimation.Col = value;
				}
			}
		}
	
	
		public int Frame
		{
			get
			{
				if(currentAnimation != null)
				{
					return currentAnimation.Row;
				}
				return -1;
			}
			set
			{
				if(currentAnimation != null)
				{
					currentAnimation.Row = value;
				}
			}
		}
		
	
		public int FrameCount
		{
			get
			{
				if(currentAnimation != null)
				{
					return currentAnimation.FrameCount;
				}
				return -1;
			}
		}
	
		public String AnimationName
		{
			get
			{
				if(currentAnimation != null)
				{
					return currentAnimation.Name;
				}
				return null;
			}
		}
		
		
		public int FrameWidth
		{
			get
			{
				return currentAnimation.FrameWidth;
			}
		}
		
		public int FrameHeight
		{
			get
			{
				return currentAnimation.FrameHeight;
			}
		}
	}
}