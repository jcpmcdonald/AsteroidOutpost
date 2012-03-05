using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using C3.XNA;
using C3.XNA.Controls;
using Microsoft.Xna.Framework;

namespace AsteroidOutpost.Screens
{
	partial class LobbyScreen : AOMenuScreen
	{
		private Label lblTemporary = new Label();
		private Button btnStartGame = new Button();
		private Button btnLeave = new Button();
		private ListBox lstPlayers = new ListBox();
		private Label lblTitle = new Label();
		private Label lblTemporary2 = new Label();
		private Label lblServerName = new Label();
		private Label txtServerName = new Label();


		/// <summary>
		/// LoadContent will be called once per game and is the place to load all of your content.
		/// </summary>
		/// <param name="spriteBatch">The related sprite batch</param>
		/// <param name="content">The content manager to load your content with</param>
		public override void LoadContent(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, Microsoft.Xna.Framework.Content.ContentManager content)
		{
			base.LoadContent(spriteBatch, content);

			ColumnHeader colPlayers = new ColumnHeader("Player List", 1.0f);

			//////////////////////////////////////////////////////////////////////////////////////////////////
			// NOTE: DESIGNER CODE, DON'T SCREW WITH THE NUMBERS HERE. EDIT THE SCREEN IN THE DESIGNER
			//////////////////////////////////////////////////////////////////////////////////////////////////

			// 
			// lblTemporary
			// 
			this.lblTemporary.Location = new Vector2(79, 259);
			this.lblTemporary.Size = new Size(244, 18);
			this.lblTemporary.Text = "Put a bunch of cool settings here...";
			// 
			// btnStartGame
			// 
			this.btnStartGame.Location = new Vector2(921, 737);
			this.btnStartGame.Size = new Size(100, 28);
			this.btnStartGame.Text = "Start Game";
			// 
			// btnLeave
			// 
			this.btnLeave.Location = new Vector2(3, 737);
			this.btnLeave.Size = new Size(100, 28);
			this.btnLeave.Text = "Leave";
			// 
			// lstPlayers
			// 
			this.lstPlayers.AddColumns(new ColumnHeader[] { colPlayers });
			this.lstPlayers.Location = new Vector2(657, 144);
			this.lstPlayers.Size = new Size(364, 417);
			// 
			// colPlayers
			// 
			//colPlayers.Width = 312;
			// 
			// lblTitle
			// 
			this.lblTitle.Location = new Vector2(462, 16);
			this.lblTitle.Size = new Size(101, 18);
			this.lblTitle.Text = "Server Lobby";
			// 
			// lblTemporary2
			// 
			this.lblTemporary2.Location = new Vector2(303, 296);
			this.lblTemporary2.Size = new Size(85, 18);
			this.lblTemporary2.Text = "...and there";
			// 
			// lblServerName
			// 
			this.lblServerName.Location = new Vector2(79, 162);
			this.lblServerName.Size = new Size(104, 18);
			this.lblServerName.Text = "Server Name:";
			// 
			// txtServerName
			// 
			this.txtServerName.Location = new Vector2(189, 162);
			this.txtServerName.Size = new Size(78, 18);
			this.txtServerName.Text = "";




			lblTemporary2.MouseEnter += lblTemporary2_MouseEnter;
			btnLeave.Click += btnLeave_Click;
			btnStartGame.Click += btnStartGame_Click;


			AddAllLocalControls(this);
		}


		private void lblTemporary2_MouseEnter(object sender, C3.XNA.Events.MouseEventArgs e)
		{
			// hehehe
			Random rand = new Random();
			lblTemporary2.Location = new Vector2(rand.Next(0, (int)lstPlayers.Location.X - 100), rand.Next((int)lblTemporary.Location.Y + 20, menuPanel.Height - 30));
		}
	}
}
