namespace Asteroid_Outpost_Screens
{
	partial class ServerHostScreen
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
			this.lblTemporary = new System.Windows.Forms.Label();
			this.btnStartServer = new System.Windows.Forms.Button();
			this.btnBack = new System.Windows.Forms.Button();
			this.lblServerName = new System.Windows.Forms.Label();
			this.txtServerName = new System.Windows.Forms.TextBox();
			this.lblTitle = new System.Windows.Forms.Label();
			this.menuPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// menuPanel
			// 
			this.menuPanel.Controls.Add(this.lblTitle);
			this.menuPanel.Controls.Add(this.txtServerName);
			this.menuPanel.Controls.Add(this.lblServerName);
			this.menuPanel.Controls.Add(this.lblTemporary);
			this.menuPanel.Controls.Add(this.btnStartServer);
			this.menuPanel.Controls.Add(this.btnBack);
			this.menuPanel.Location = new System.Drawing.Point(32, 31);
			this.menuPanel.Name = "menuPanel";
			this.menuPanel.Size = new System.Drawing.Size(1024, 768);
			this.menuPanel.TabIndex = 0;
			// 
			// lblTemporary
			// 
			this.lblTemporary.AutoSize = true;
			this.lblTemporary.Font = new System.Drawing.Font("Arial", 12F);
			this.lblTemporary.Location = new System.Drawing.Point(396, 341);
			this.lblTemporary.Name = "lblTemporary";
			this.lblTemporary.Size = new System.Drawing.Size(232, 18);
			this.lblTemporary.TabIndex = 13;
			this.lblTemporary.Text = "Put a bunch of cool settings here";
			// 
			// btnStartServer
			// 
			this.btnStartServer.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnStartServer.Location = new System.Drawing.Point(921, 737);
			this.btnStartServer.Name = "btnStartServer";
			this.btnStartServer.Size = new System.Drawing.Size(100, 28);
			this.btnStartServer.TabIndex = 12;
			this.btnStartServer.Text = "Start Server";
			this.btnStartServer.UseVisualStyleBackColor = true;
			// 
			// btnBack
			// 
			this.btnBack.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnBack.Location = new System.Drawing.Point(3, 737);
			this.btnBack.Name = "btnBack";
			this.btnBack.Size = new System.Drawing.Size(100, 28);
			this.btnBack.TabIndex = 11;
			this.btnBack.Text = "Back";
			this.btnBack.UseVisualStyleBackColor = true;
			// 
			// lblServerName
			// 
			this.lblServerName.AutoSize = true;
			this.lblServerName.Font = new System.Drawing.Font("Arial", 12F);
			this.lblServerName.Location = new System.Drawing.Point(327, 274);
			this.lblServerName.Name = "lblServerName";
			this.lblServerName.Size = new System.Drawing.Size(104, 18);
			this.lblServerName.TabIndex = 14;
			this.lblServerName.Text = "Server Name:";
			// 
			// txtServerName
			// 
			this.txtServerName.Font = new System.Drawing.Font("Arial", 10F);
			this.txtServerName.Location = new System.Drawing.Point(437, 273);
			this.txtServerName.Name = "txtServerName";
			this.txtServerName.Size = new System.Drawing.Size(261, 23);
			this.txtServerName.TabIndex = 15;
			// 
			// lblTitle
			// 
			this.lblTitle.AutoSize = true;
			this.lblTitle.Font = new System.Drawing.Font("Arial", 12F);
			this.lblTitle.Location = new System.Drawing.Point(467, 19);
			this.lblTitle.Name = "lblTitle";
			this.lblTitle.Size = new System.Drawing.Size(90, 18);
			this.lblTitle.TabIndex = 16;
			this.lblTitle.Text = "Server Host";
			// 
			// ServerHostScreen
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1147, 846);
			this.Controls.Add(this.menuPanel);
			this.Name = "ServerHostScreen";
			this.Text = "ServerHostScreen";
			this.menuPanel.ResumeLayout(false);
			this.menuPanel.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel menuPanel;
		private System.Windows.Forms.Button btnBack;
		private System.Windows.Forms.Button btnStartServer;
		private System.Windows.Forms.Label lblTemporary;
		private System.Windows.Forms.TextBox txtServerName;
		private System.Windows.Forms.Label lblServerName;
		private System.Windows.Forms.Label lblTitle;
	}
}