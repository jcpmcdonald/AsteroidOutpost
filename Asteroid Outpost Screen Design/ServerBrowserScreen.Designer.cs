using System.Drawing;
using System.Windows.Forms;

namespace Asteroid_Outpost_Screens
{
	partial class ServerBrowserScreen
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
			this.btnMainMenu = new System.Windows.Forms.Button();
			this.lstServers = new System.Windows.Forms.ListView();
			this.colServerName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colState = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colPlayers = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colPing = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.btnHost = new System.Windows.Forms.Button();
			this.btnRefresh = new System.Windows.Forms.Button();
			this.btnConnect = new System.Windows.Forms.Button();
			this.menuPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// menuPanel
			// 
			this.menuPanel.Controls.Add(this.btnMainMenu);
			this.menuPanel.Controls.Add(this.lstServers);
			this.menuPanel.Controls.Add(this.btnHost);
			this.menuPanel.Controls.Add(this.btnRefresh);
			this.menuPanel.Controls.Add(this.btnConnect);
			this.menuPanel.Location = new System.Drawing.Point(29, 29);
			this.menuPanel.Name = "menuPanel";
			this.menuPanel.Size = new System.Drawing.Size(1024, 768);
			this.menuPanel.TabIndex = 5;
			// 
			// btnMainMenu
			// 
			this.btnMainMenu.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnMainMenu.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnMainMenu.Location = new System.Drawing.Point(3, 737);
			this.btnMainMenu.Name = "btnMainMenu";
			this.btnMainMenu.Size = new System.Drawing.Size(100, 28);
			this.btnMainMenu.TabIndex = 10;
			this.btnMainMenu.Text = "Main Menu";
			this.btnMainMenu.UseVisualStyleBackColor = true;
			// 
			// lstServers
			// 
			this.lstServers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colServerName,
            this.colState,
            this.colPlayers,
            this.colPing});
			this.lstServers.Location = new System.Drawing.Point(3, 3);
			this.lstServers.Name = "lstServers";
			this.lstServers.Size = new System.Drawing.Size(1018, 728);
			this.lstServers.TabIndex = 9;
			this.lstServers.UseCompatibleStateImageBehavior = false;
			this.lstServers.View = System.Windows.Forms.View.Details;
			// 
			// colServerName
			// 
			this.colServerName.Width = 488;
			// 
			// colState
			// 
			this.colState.Width = 208;
			// 
			// colPlayers
			// 
			this.colPlayers.Width = 123;
			// 
			// colPing
			// 
			this.colPing.Width = 126;
			// 
			// btnHost
			// 
			this.btnHost.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnHost.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnHost.Location = new System.Drawing.Point(815, 737);
			this.btnHost.Name = "btnHost";
			this.btnHost.Size = new System.Drawing.Size(100, 28);
			this.btnHost.TabIndex = 8;
			this.btnHost.Text = "Host";
			this.btnHost.UseVisualStyleBackColor = true;
			// 
			// btnRefresh
			// 
			this.btnRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnRefresh.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnRefresh.Location = new System.Drawing.Point(709, 737);
			this.btnRefresh.Name = "btnRefresh";
			this.btnRefresh.Size = new System.Drawing.Size(100, 28);
			this.btnRefresh.TabIndex = 7;
			this.btnRefresh.Text = "Refresh";
			this.btnRefresh.UseVisualStyleBackColor = true;
			// 
			// btnConnect
			// 
			this.btnConnect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnConnect.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnConnect.Location = new System.Drawing.Point(921, 737);
			this.btnConnect.Name = "btnConnect";
			this.btnConnect.Size = new System.Drawing.Size(100, 28);
			this.btnConnect.TabIndex = 5;
			this.btnConnect.Text = "Connect";
			this.btnConnect.UseVisualStyleBackColor = true;
			// 
			// ServerBrowserScreen
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1235, 895);
			this.Controls.Add(this.menuPanel);
			this.Name = "ServerBrowserScreen";
			this.Text = "Form1";
			this.menuPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel menuPanel;
		private System.Windows.Forms.Button btnHost;
		private System.Windows.Forms.Button btnRefresh;
		private System.Windows.Forms.Button btnConnect;
		private System.Windows.Forms.ListView lstServers;
		private System.Windows.Forms.ColumnHeader colServerName;
		private System.Windows.Forms.ColumnHeader colState;
		private System.Windows.Forms.ColumnHeader colPlayers;
		private System.Windows.Forms.ColumnHeader colPing;
		private System.Windows.Forms.Button btnMainMenu;

	}
}

