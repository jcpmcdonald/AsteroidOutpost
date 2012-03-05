using System.Reflection;
using C3.XNA;
using C3.XNA.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AsteroidOutpost.Screens
{
	partial class ServerHostScreen : AOMenuScreen
	{
		private Label lblTitle = new Label();
		private Label lblServerName = new Label();
		private TextBox txtServerName = new TextBox();
		private Button btnStartServer = new Button();
		private Button btnBack = new Button();
		private Label lblTemporary = new Label();


		public override void LoadContent(SpriteBatch spriteBatch, Microsoft.Xna.Framework.Content.ContentManager content)
		{
			base.LoadContent(spriteBatch, content);

			//////////////////////////////////////////////////////////////////////////////////////////////////
			// NOTE: DESIGNER CODE, DON'T SCREW WITH THE NUMBERS HERE. EDIT THE SCREEN IN THE DESIGNER
			//////////////////////////////////////////////////////////////////////////////////////////////////
			
			//btnStartHost = new Button("Start Host", 600, 500, 100, 30);


			// 
			// lblTitle
			// 
			this.lblTitle.Location = new Vector2(467, 19);
			this.lblTitle.Size = new Size(90, 18);
			this.lblTitle.Text = "Server Host";
			// 
			// lblTemporary
			// 
			this.lblTemporary.Location = new Vector2(396, 341);
			this.lblTemporary.Size = new Size(232, 18);
			this.lblTemporary.Text = "Put a bunch of cool settings here";
			// 
			// lblServerName
			// 
			this.lblServerName.Location = new Vector2(327, 274);
			this.lblServerName.Size = new Size(104, 18);
			this.lblServerName.Text = "Server Name:";
			// 
			// txtServerName
			// 
			this.txtServerName.Location = new Vector2(437, 274);
			this.txtServerName.Size = new Size(261, 21);
			// 
			// btnBack
			// 
			this.btnBack.Location = new Vector2(3, 737);
			this.btnBack.Size = new Size(100, 28);
			this.btnBack.Text = "Back";
			// 
			// btnStartServer
			// 
			this.btnStartServer.Location = new Vector2(921, 737);
			this.btnStartServer.Size = new Size(100, 28);
			this.btnStartServer.Text = "Start Server";
			
			
			btnStartServer.Click += btnStartHost_Click;
			btnBack.Click += btnBack_Click;

			AddAllLocalControls(this);
		}
	}
}
