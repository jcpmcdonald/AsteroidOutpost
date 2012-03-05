namespace Asteroid_Outpost_Screens
{
	partial class MissionSelectScreen
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
			this.btnTutorial = new System.Windows.Forms.Button();
			this.btnEndless = new System.Windows.Forms.Button();
			this.btnBack = new System.Windows.Forms.Button();
			this.menuPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// menuPanel
			// 
			this.menuPanel.Controls.Add(this.btnBack);
			this.menuPanel.Controls.Add(this.btnEndless);
			this.menuPanel.Controls.Add(this.btnTutorial);
			this.menuPanel.Location = new System.Drawing.Point(12, 12);
			this.menuPanel.Name = "menuPanel";
			this.menuPanel.Size = new System.Drawing.Size(1024, 768);
			this.menuPanel.TabIndex = 1;
			// 
			// btnTutorial
			// 
			this.btnTutorial.Location = new System.Drawing.Point(312, 182);
			this.btnTutorial.Name = "btnTutorial";
			this.btnTutorial.Size = new System.Drawing.Size(400, 60);
			this.btnTutorial.TabIndex = 12;
			this.btnTutorial.Text = "Tutorial";
			this.btnTutorial.UseVisualStyleBackColor = true;
			// 
			// btnEndless
			// 
			this.btnEndless.Location = new System.Drawing.Point(312, 248);
			this.btnEndless.Name = "btnEndless";
			this.btnEndless.Size = new System.Drawing.Size(400, 60);
			this.btnEndless.TabIndex = 13;
			this.btnEndless.Text = "Endless";
			this.btnEndless.UseVisualStyleBackColor = true;
			// 
			// btnBack
			// 
			this.btnBack.Location = new System.Drawing.Point(312, 708);
			this.btnBack.Name = "btnBack";
			this.btnBack.Size = new System.Drawing.Size(400, 60);
			this.btnBack.TabIndex = 14;
			this.btnBack.Text = "Back";
			this.btnBack.UseVisualStyleBackColor = true;
			// 
			// MissionSelectScreen
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1064, 812);
			this.Controls.Add(this.menuPanel);
			this.Name = "MissionSelectScreen";
			this.Text = "MissionSelectScreen";
			this.menuPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel menuPanel;
		private System.Windows.Forms.Button btnTutorial;
		private System.Windows.Forms.Button btnEndless;
		private System.Windows.Forms.Button btnBack;
	}
}