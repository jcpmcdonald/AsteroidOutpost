using System;
using AsteroidOutpost.Networking;
using AsteroidOutpost.Scenarios;
using C3.XNA;
using C3.XNA.Controls;

namespace AsteroidOutpost.Screens
{
	public partial class LobbyScreen : AOMenuScreen
	{
		private AsteroidOutpostScreen theGame;

		public LobbyScreen(AsteroidOutpostScreen theGame, ScreenManager theScreenManager, LayeredStarField starField)
			: base(theScreenManager, starField)
		{
			this.theGame = theGame;
		}



		void btnLeave_Click(object sender, C3.XNA.Events.MouseButtonEventArgs e)
		{
			ScreenMan.SwitchScreens("Server Browser");
		}


		void btnStartGame_Click(object sender, C3.XNA.Events.MouseButtonEventArgs e)
		{
			theGame.StartServer(new RandomScenario(theGame));
			ScreenMan.SwitchScreens("Game");
		}


		/// <summary>
		/// Updates this screen
		/// </summary>
		/// <param name="deltaTime">The amount of time that has passed since the last update</param>
		/// <param name="theMouse">The current state of the mouse</param>
		/// <param name="theKeyboard">The current state of the keyboard</param>
		public override void Update(TimeSpan deltaTime, EnhancedMouseState theMouse, EnhancedKeyboardState theKeyboard)
		{
			theGame.Network.ProcessIncomingQueue(deltaTime);
			theGame.Network.ProcessOutgoingQueue();

			base.Update(deltaTime, theMouse, theKeyboard);
		}


		/// <summary>
		/// Updates this screen while we are being transitioned toward
		/// </summary>
		/// <param name="deltaTime">The amount of time that has passed since the last update</param>
		/// <param name="theMouse">The current state of the mouse</param>
		/// <param name="theKeyboard">The current state of the keyboard</param>
		/// <param name="percentComplete">The transition's percentage complete (0-1)</param>
		protected override void UpdateTransitionToward(TimeSpan deltaTime, EnhancedMouseState theMouse, EnhancedKeyboardState theKeyboard, float percentComplete)
		{
			theGame.Network.ProcessIncomingQueue(deltaTime);
			theGame.Network.ProcessOutgoingQueue();

			base.UpdateTransitionToward(deltaTime, theMouse, theKeyboard, percentComplete);
		}


		/// <summary>
		/// Called when this screen is first being transitioned toward
		/// </summary>
		/// <param name="deltaTime">The amount of time that has passed since the last update</param>
		/// <param name="theMouse">The current state of the mouse</param>
		/// <param name="theKeyboard">The current state of the keyboard</param>
		protected override void StartTransitionToward(TimeSpan deltaTime, EnhancedMouseState theMouse, EnhancedKeyboardState theKeyboard)
		{
			lstPlayers.ClearListItems();

			theGame.Network.ClientJoinedGame += AONetwork_ClientJoinedGame;

			txtServerName.Text = theGame.Network.ServerName;

			if (theGame.IsServer)
			{
				theGame.Network.StartServerBeacon();
				theGame.Network.StartServerInfoServer();
				theGame.Network.StartListening(18189);

				btnStartGame.Enabled = true;
			}
			else
			{
				btnStartGame.Enabled = false;
			}


			base.StartTransitionToward(deltaTime, theMouse, theKeyboard);
		}


		private void AONetwork_ClientJoinedGame(object sender, ClientJoinedGameHandlerArgs args)
		{
			lstPlayers.AddListItem(new SimpleListRow(new String[]{ args.PlayerName }));
		}


		/// <summary>
		/// Called when this screen is first being transitioned toward
		/// </summary>
		/// <param name="deltaTime">The amount of time that has passed since the last update</param>
		/// <param name="theMouse">The current state of the mouse</param>
		/// <param name="theKeyboard">The current state of the keyboard</param>
		protected override void StartTransitionAway(TimeSpan deltaTime, EnhancedMouseState theMouse, EnhancedKeyboardState theKeyboard)
		{
			theGame.Network.ClientJoinedGame -= AONetwork_ClientJoinedGame;
			cleanup();

			base.StartTransitionAway(deltaTime, theMouse, theKeyboard);
		}


		/// <summary>
		/// UnloadContent will be called once per game and is the place to unload all non ContentManager content here
		/// </summary>
		public override void UnloadContent()
		{
			base.UnloadContent();

			cleanup();
		}


		private void cleanup()
		{
			// TODO: Make sure this doesn't screw anything up on the client since they didn't actually start these
			theGame.Network.StopServerBeacon();
			theGame.Network.StopServerInfoServer();
		}
	}
}
