using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using C3.XNA;
using C3.XNA.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AsteroidOutpost.Screens
{
	partial class MissionSelectScreen
	{
		private Label lblTitle = new Label();
		private Button btnTutorial = new Button();
		private Button btnEndless = new Button();
		private Button btnBack = new Button();


		public override void LoadContent(SpriteBatch spriteBatch, Microsoft.Xna.Framework.Content.ContentManager content)
		{
			base.LoadContent(spriteBatch, content);

			//////////////////////////////////////////////////////////////////////////////////////////////////
			// NOTE: DESIGNER CODE, DON'T SCREW WITH THE NUMBERS HERE. EDIT THE SCREEN IN THE DESIGNER
			//////////////////////////////////////////////////////////////////////////////////////////////////

			// 
			// btnTutorual
			// 
			this.btnTutorial.Location = new Vector2(312, 182);
			this.btnTutorial.Size = new Size(400, 60);
			this.btnTutorial.Text = "Tutorial";
			// 
			// btnEndless
			// 
			this.btnEndless.Location = new Vector2(312, 248);
			this.btnEndless.Size = new Size(400, 60);
			this.btnEndless.Text = "Endless";
			// 
			// btnBack
			// 
			this.btnBack.Location = new Vector2(312, 708);
			this.btnBack.Size = new Size(400, 60);
			this.btnBack.Text = "Back";


			btnTutorial.Click += btnTutorial_Click;
			btnEndless.Click += btnEndless_Click;
			btnBack.Click += btnBack_Click;

			AddAllLocalControls(this);
		}
	}
}
