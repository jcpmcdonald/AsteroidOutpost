using Microsoft.Xna.Framework.Input;

namespace AsteroidOutpost
{
	public enum EnhancedButtonState
	{
		PRESSED,
		RELEASED,
		JUST_PRESSED,
		JUST_RELEASED
	}

	public enum MouseButton
	{
		LEFT = 0,
		MIDDLE = 1,
		RIGHT = 2
	}

	public class EnhancedMouseState
	{
		MouseState lastState = Mouse.GetState();
		MouseState currentState = Mouse.GetState();

		public EnhancedButtonState LeftButton
		{
			get
			{
				if (lastState.LeftButton == ButtonState.Pressed && currentState.LeftButton == ButtonState.Pressed)
				{
					return EnhancedButtonState.PRESSED;
				}
				else if (lastState.LeftButton == ButtonState.Pressed && currentState.LeftButton == ButtonState.Released)
				{
					return EnhancedButtonState.JUST_RELEASED;
				}
				else if (lastState.LeftButton == ButtonState.Released && currentState.LeftButton == ButtonState.Pressed)
				{
					return EnhancedButtonState.JUST_PRESSED;
				}
				else // if(lastState.LeftButton == ButtonState.RELEASED && currentState.LeftButton == ButtonState.RELEASED)
				{
					return EnhancedButtonState.RELEASED;
				}
			}
		}

		public EnhancedButtonState RightButton
		{
			get
			{
				if (lastState.RightButton == ButtonState.Pressed && currentState.RightButton == ButtonState.Pressed)
				{
					return EnhancedButtonState.PRESSED;
				}
				else if (lastState.RightButton == ButtonState.Pressed && currentState.RightButton == ButtonState.Released)
				{
					return EnhancedButtonState.JUST_RELEASED;
				}
				else if (lastState.RightButton == ButtonState.Released && currentState.RightButton == ButtonState.Pressed)
				{
					return EnhancedButtonState.JUST_PRESSED;
				}
				else // if(lastState.RightButton == ButtonState.RELEASED && currentState.RightButton == ButtonState.RELEASED)
				{
					return EnhancedButtonState.RELEASED;
				}
			}
		}


		public EnhancedButtonState MiddleButton
		{
			get
			{
				if (lastState.MiddleButton == ButtonState.Pressed && currentState.MiddleButton == ButtonState.Pressed)
				{
					return EnhancedButtonState.PRESSED;
				}
				else if (lastState.MiddleButton == ButtonState.Pressed && currentState.MiddleButton == ButtonState.Released)
				{
					return EnhancedButtonState.JUST_RELEASED;
				}
				else if (lastState.MiddleButton == ButtonState.Released && currentState.MiddleButton == ButtonState.Pressed)
				{
					return EnhancedButtonState.JUST_PRESSED;
				}
				else // if(lastState.MiddleButton == ButtonState.RELEASED && currentState.MiddleButton == ButtonState.RELEASED)
				{
					return EnhancedButtonState.RELEASED;
				}
			}
		}


		public int ScrollWheelValue
		{
			get { return currentState.ScrollWheelValue; }
		}


		public int ScrollWheelDelta
		{
			get { return currentState.ScrollWheelValue - lastState.ScrollWheelValue; }
		}


		public int X
		{
			get { return currentState.X; }
		}


		public int Y
		{
			get { return currentState.Y; }
		}


		public void UpdateState()
		{
			lastState = currentState;
			currentState = Mouse.GetState();
		}
	}
}