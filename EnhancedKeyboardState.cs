using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework.Input;

namespace AsteroidOutpost
{
	/// <summary>
	/// A definition for all the states a key can be in
	/// </summary>
	public enum EnhancedKeyState
	{
		PRESSED,
		RELEASED,
		JUST_PRESSED,
		JUST_RELEASED
	}

	/// <summary>
	/// A class that wraps the base KeyboardState and provides JUST_PRESSED and JUST_RELEASED functionality
	/// </summary>
	public class EnhancedKeyboardState
	{
		private KeyboardState lastState = Keyboard.GetState();
		private KeyboardState currentState = Keyboard.GetState();


		/// <summary>
		/// Updates the state of the keyboard. This should be done exactly once every cycle to
		/// detect the JUST_PRESSED and JUST_RELEASED states.
		/// </summary>
		public void UpdateState()
		{
			lastState = currentState;
			currentState = Keyboard.GetState();
		}


		/// <summary>
		/// Returns the state of a particular key
		/// </summary>
		/// <param name="key">Enumerated value representing the key to query</param>
		/// <returns>Returns the state of a particular ke.</returns>
		public EnhancedKeyState this[Keys key]
		{
			get
			{
				// Look at the current state of the key and compare it to the state of the
				// last state of the key to determine if the key has changed recently
				if(lastState[key] == KeyState.Up && currentState[key] == KeyState.Up)
				{
					return EnhancedKeyState.RELEASED;
				}
				else if (lastState[key] == KeyState.Down && currentState[key] == KeyState.Down)
				{
					return EnhancedKeyState.PRESSED;
				}
				else if (lastState[key] == KeyState.Up && currentState[key] == KeyState.Down)
				{
					return EnhancedKeyState.JUST_PRESSED;
				}
				else if (lastState[key] == KeyState.Down && currentState[key] == KeyState.Up)
				{
					return EnhancedKeyState.JUST_RELEASED;
				}
				else
				{
					Debug.Assert(true, "An unknown key press combination was detected");
					return EnhancedKeyState.RELEASED;
				}
			}
		}


		/// <summary>
		/// Returns whether a specified key is currently being pressed
		/// </summary>
		/// <param name="key">Enumerated value that specifies the key to query</param>
		/// <returns>Returns whether a specified key is currently being pressed</returns>
		public bool IsKeyDown(Keys key)
		{
			// Pass the query to the current state
			return currentState.IsKeyDown(key);
		}


		/// <summary>
		/// Returns whether a specified key is currently not pressed
		/// </summary>
		/// <param name="key">Enumerated value that specifies the key to query</param>
		/// <returns>Returns whether a specified key is currently not pressed</returns>
		public bool IsKeyUp(Keys key)
		{
			// Pass the query to the current state
			return currentState.IsKeyUp(key);
		}


		/// <summary>
		/// Gets an array of values that correspond to the keyboard keys that are currently being pressed
		/// </summary>
		/// <returns>An array of values that correspond to the keyboard keys that are currently being pressed</returns>
		public Keys[] GetPressedKeys()
		{
			// Pass the query to the current state
			return currentState.GetPressedKeys();
		}


		/// <summary>
		/// Gets a list of Keys that were pressed since the last time the UpdateState function was called
		/// </summary>
		/// <returns>A list of Keys that were pressed since the last time the UpdateState function was called. An empty list is returned if there are no keys that have recently been pressed</returns>
		public List<Keys> GetJustPressedKeys()
		{
			List<Keys> justPressedKeys = new List<Keys>();
			foreach (Keys key in currentState.GetPressedKeys())
			{
				if(lastState.IsKeyUp(key))
				{
					justPressedKeys.Add(key);
				}
			}
			return justPressedKeys;
		}


		/// <summary>
		/// Gets a list of Keys that were released since the last time the UpdateState function was called
		/// </summary>
		/// <returns>A list of Keys that were released since the last time the UpdateState function was called. An empty list is returned if there are no keys that have recently been released</returns>
		public List<Keys> GetJustReleasedKeys()
		{
			List<Keys> justReleasedKeys = new List<Keys>();
			foreach (Keys key in lastState.GetPressedKeys())
			{
				if (currentState.IsKeyUp(key))
				{
					justReleasedKeys.Add(key);
				}
			}
			return justReleasedKeys;
		}

	}
}