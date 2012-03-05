namespace Asteroid_Outpost_Screens
{
	partial class LobbyScreen
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.menuPanel = new System.Windows.Forms.Panel();
			this.lblTemporary2 = new System.Windows.Forms.Label();
			this.lblTitle = new System.Windows.Forms.Label();
			this.lstPlayers = new System.Windows.Forms.ListView();
			this.colPlayers = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.lblTemporary = new System.Windows.Forms.Label();
			this.btnStartGame = new System.Windows.Forms.Button();
			this.btnLeave = new System.Windows.Forms.Button();
			this.lblServerName = new System.Windows.Forms.Label();
			this.txtServerName = new System.Windows.Forms.Label();
			this.menuPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// menuPanel
			// 
			this.menuPanel.Controls.Add(this.txtServerName);
			this.menuPanel.Controls.Add(this.lblServerName);
			this.menuPanel.Controls.Add(this.lblTemporary2);
			this.menuPanel.Controls.Add(this.lblTitle);
			this.menuPanel.Controls.Add(this.lstPlayers);
			this.menuPanel.Controls.Add(this.lblTemporary);
			this.menuPanel.Controls.Add(this.btnStartGame);
			this.menuPanel.Controls.Add(this.btnLeave);
			this.menuPanel.Location = new System.Drawing.Point(30, 12);
			this.menuPanel.Name = "menuPanel";
			this.menuPanel.Size = new System.Drawing.Size(1024, 768);
			this.menuPanel.TabIndex = 1;
			// 
			// lblTemporary2
			// 
			this.lblTemporary2.AutoSize = true;
			this.lblTemporary2.Font = new System.Drawing.Font("Arial", 12F);
			this.lblTemporary2.Location = new System.Drawing.Point(303, 296);
			this.lblTemporary2.Name = "lblTemporary2";
			this.lblTemporary2.Size = new System.Drawing.Size(85, 18);
			this.lblTemporary2.TabIndex = 16;
			this.lblTemporary2.Text = "...and there";
			// 
			// lblTitle
			// 
			this.lblTitle.AutoSize = true;
			this.lblTitle.Font = new System.Drawing.Font("Arial", 12F);
			this.lblTitle.Location = new System.Drawing.Point(462, 16);
			this.lblTitle.Name = "lblTitle";
			this.lblTitle.Size = new System.Drawing.Size(101, 18);
			this.lblTitle.TabIndex = 15;
			this.lblTitle.Text = "Server Lobby";
			// 
			// lstPlayers
			// 
			this.lstPlayers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colPlayers});
			this.lstPlayers.Location = new System.Drawing.Point(657, 144);
			this.lstPlayers.Name = "lstPlayers";
			this.lstPlayers.Size = new System.Drawing.Size(364, 417);
			this.lstPlayers.TabIndex = 14;
			this.lstPlayers.UseCompatibleStateImageBehavior = false;
			this.lstPlayers.View = System.Windows.Forms.View.Details;
			// 
			// colPlayers
			// 
			this.colPlayers.Width = 312;
			// 
			// lblTemporary
			// 
			this.lblTemporary.AutoSize = true;
			this.lblTemporary.Font = new System.Drawing.Font("Arial", 12F);
			this.lblTemporary.Location = new System.Drawing.Point(79, 259);
			this.lblTemporary.Name = "lblTemporary";
			this.lblTemporary.Size = new System.Drawing.Size(244, 18);
			this.lblTemporary.TabIndex = 13;
			this.lblTemporary.Text = "Put a bunch of cool settings here...";
			// 
			// btnStartGame
			// 
			this.btnStartGame.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnStartGame.Location = new System.Drawing.Point(921, 737);
			this.btnStartGame.Name = "btnStartGame";
			this.btnStartGame.Size = new System.Drawing.Size(100, 28);
			this.btnStartGame.TabIndex = 12;
			this.btnStartGame.Text = "Start Game";
			this.btnStartGame.UseVisualStyleBackColor = true;
			// 
			// btnLeave
			// 
			this.btnLeave.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnLeave.Location = new System.Drawing.Point(3, 737);
			this.btnLeave.Name = "btnLeave";
			this.btnLeave.Size = new System.Drawing.Size(100, 28);
			this.btnLeave.TabIndex = 11;
			this.btnLeave.Text = "Leave";
			this.btnLeave.UseVisualStyleBackColor = true;
			// 
			// lblServerName
			// 
			this.lblServerName.AutoSize = true;
			this.lblServerName.Font = new System.Drawing.Font("Arial", 12F);
			this.lblServerName.Location = new System.Drawing.Point(79, 162);
			this.lblServerName.Name = "lblServerName";
			this.lblServerName.Size = new System.Drawing.Size(104, 18);
			this.lblServerName.TabIndex = 17;
			this.lblServerName.Text = "Server Name:";
			// 
			// txtServerName
			// 
			this.txtServerName.AutoSize = true;
			this.txtServerName.Font = new System.Drawing.Font("Arial", 12F);
			this.txtServerName.Location = new System.Drawing.Point(189, 162);
			this.txtServerName.Name = "txtServerName";
			this.txtServerName.Size = new System.Drawing.Size(78, 18);
			this.txtServerName.TabIndex = 18;
			this.txtServerName.Text = "My Server";
			// 
			// LobbyScreen
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1155, 839);
			this.Controls.Add(this.menuPanel);
			this.Name = "LobbyScreen";
			this.Text = "LobbyScreen";
			this.menuPanel.ResumeLayout(false);
			this.menuPanel.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel menuPanel;
		private System.Windows.Forms.Label lblTemporary;
		private System.Windows.Forms.Button btnStartGame;
		private System.Windows.Forms.Button btnLeave;
		private System.Windows.Forms.ListView lstPlayers;
		private System.Windows.Forms.ColumnHeader colPlayers;
		private System.Windows.Forms.Label lblTitle;
		private System.Windows.Forms.Label lblTemporary2;
		private System.Windows.Forms.Label lblServerName;
		private System.Windows.Forms.Label txtServerName;
	}
}