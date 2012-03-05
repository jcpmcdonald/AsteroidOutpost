using C3.XNA;
using C3.XNA.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace AsteroidOutpost.Screens
{
	partial class ServerBrowserScreen : AOMenuScreen
	{
		private ListBox lstServers = new ListBox();
		private Button btnConnect = new Button();
		private Button btnRefresh = new Button();
		private Button btnHost = new Button();
		private Button btnMainMenu = new Button();


		public override void LoadContent(SpriteBatch spriteBatch, ContentManager content)
		{
			base.LoadContent(spriteBatch, content);


			ColumnHeader colServerName = new ColumnHeader("Server Name", 0.50f);
			ColumnHeader colState = new ColumnHeader("State", 0.20f);
			ColumnHeader colPlayers = new ColumnHeader("Players", 0.15f);
			ColumnHeader colPing = new ColumnHeader("Ping", 0.15f);

			//////////////////////////////////////////////////////////////////////////////////////////////////
			// NOTE: DESIGNER CODE, DON'T SCREW WITH THE NUMBERS HERE. EDIT THE SCREEN IN THE DESIGNER
			//////////////////////////////////////////////////////////////////////////////////////////////////


			/*
			//
			// lstServers
			//
			lstServers.Location = new Vector2(3, 3);
			lstServers.Size = new Size(978, 666);
			lstServers.ItemHeight = 18;
			lstServers.HeaderHeight = 20;
			lstServers.AddColumns(new ColumnHeader[] {
										new ColumnHeader("Server Name", 0.50f),
										new ColumnHeader("State", 0.20f),
										new ColumnHeader("Players", 0.15f),
										new ColumnHeader("Ping", 0.15f)});

			// 
			// btnHost
			// 
			btnHost.Location = new Vector2(775, 675);
			btnHost.Size = new Size(100, 28);
			btnHost.Text = "Host";
			// 
			// btnRefresh
			// 
			btnRefresh.Location = new Vector2(669, 675);
			btnRefresh.Size = new Size(100, 28);
			btnRefresh.Text = "Refresh";
			// 
			// btnConnect
			// 
			btnConnect.Location = new Vector2(881, 675);
			btnConnect.Size = new Size(100, 28);
			btnConnect.Text = "Connect";
			// 
			// btnBack
			// 
			btnMainMenu.Location = new Vector2(3, 675);
			btnMainMenu.Size = new Size(100, 28);
			btnMainMenu.Text = "Main Menu";
			*/


			// 
			// btnMainMenu
			// 
			this.btnMainMenu.Location = new Vector2(3, 737);
			this.btnMainMenu.Size = new Size(100, 28);
			this.btnMainMenu.Text = "Main Menu";
			// 
			// lstServers
			// 
			this.lstServers.AddColumns(new ColumnHeader[] {
										colServerName,
										colState,
										colPlayers,
										colPing});
			this.lstServers.Location = new Vector2(3, 3);
			this.lstServers.Size = new Size(1018, 728);
			// 
			// btnHost
			// 
			this.btnHost.Location = new Vector2(815, 737);
			this.btnHost.Size = new Size(100, 28);
			this.btnHost.Text = "Host";
			// 
			// btnRefresh
			// 
			this.btnRefresh.Location = new Vector2(709, 737);
			this.btnRefresh.Size = new Size(100, 28);
			this.btnRefresh.Text = "Refresh";
			// 
			// btnConnect
			// 
			this.btnConnect.Location = new Vector2(921, 737);
			this.btnConnect.Size = new Size(100, 28);
			this.btnConnect.Text = "Connect";



			

			btnConnect.Enabled = false;

			lstServers.SelectionChanged += listBox_SelectionChanged;
			btnConnect.Click += btnConnect_Clicked;
			btnHost.Click += btnHost_Clicked;
			btnRefresh.Click += btnRefresh_Click;
			btnMainMenu.Click += btnMainMenu_Click;


			AddAllLocalControls(this);
		}

	}
}
