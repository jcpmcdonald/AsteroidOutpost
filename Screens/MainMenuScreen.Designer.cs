using C3.XNA;
using C3.XNA.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace AsteroidOutpost.Screens
{
	partial class MainMenuScreen : AOMenuScreen
	{
		private TextureFrame titleText;

		private TexturedButton btnExit;
		private TexturedButton btnCredits;
		private TexturedButton btnMultiPlayer;
		private TexturedButton btnSinglePlayer;

		private SoundEffect mouseOverSound;


		/// <summary>
		/// LoadContent will be called once per game and is the place to load all of your content.
		/// </summary>
		/// <param name="spriteBatch">The related sprite batch</param>
		/// <param name="content">The content manager to load your content with</param>
		public override void LoadContent(SpriteBatch spriteBatch, Microsoft.Xna.Framework.Content.ContentManager content)
		{
			base.LoadContent(spriteBatch, content);
			mouseOverSound = content.Load<SoundEffect>("menu");
			

			//////////////////////////////////////////////////////////////////////////////////////////////////
			// NOTE: DESIGNER CODE, DON'T SCREW WITH THE NUMBERS HERE. EDIT THE SCREEN IN THE DESIGNER
			//////////////////////////////////////////////////////////////////////////////////////////////////

			// TODO: Convert this screen over to use the Windows Form Designer

			titleText = new TextureFrame(content.Load<Texture2D>("Title"), new Rectangle(0, 0, 550, 80));

			Texture2D mainMenuButtonTexture = content.Load<Texture2D>("MainMenuButtons");
			int menuButtonWidth = 400;
			int menuButtonHeight = 60;

			TextureFrame singlePlayerTexture = new TextureFrame(mainMenuButtonTexture, new Rectangle(0, 0, menuButtonWidth, menuButtonHeight));
			TextureFrame singlePlayerMouseOverTexture = new TextureFrame(mainMenuButtonTexture, new Rectangle(menuButtonWidth, 0, menuButtonWidth, menuButtonHeight));

			TextureFrame multiPlayerTexture = new TextureFrame(mainMenuButtonTexture, new Rectangle(0, menuButtonHeight, menuButtonWidth, menuButtonHeight));
			TextureFrame multiPlayerMouseOverTexture = new TextureFrame(mainMenuButtonTexture, new Rectangle(menuButtonWidth, menuButtonHeight, menuButtonWidth, menuButtonHeight));

			TextureFrame creditsTexture = new TextureFrame(mainMenuButtonTexture, new Rectangle(0, menuButtonHeight * 2, menuButtonWidth, menuButtonHeight));
			TextureFrame creditsMouseOverTexture = new TextureFrame(mainMenuButtonTexture, new Rectangle(menuButtonWidth, menuButtonHeight * 2, menuButtonWidth, menuButtonHeight));

			TextureFrame exitTexture = new TextureFrame(mainMenuButtonTexture, new Rectangle(0, menuButtonHeight * 3, menuButtonWidth, menuButtonHeight));
			TextureFrame exitMouseOverTexture = new TextureFrame(mainMenuButtonTexture, new Rectangle(menuButtonWidth, menuButtonHeight * 3, menuButtonWidth, menuButtonHeight));


			btnSinglePlayer = new TexturedButton((menuPanel.Width / 2) - (menuButtonWidth / 2),
			                                     (menuPanel.Height / 2) - 200,
			                                     menuButtonWidth, menuButtonHeight,
			                                     singlePlayerTexture, singlePlayerMouseOverTexture);

			btnMultiPlayer = new TexturedButton((menuPanel.Width / 2) - (menuButtonWidth / 2),
			                                    (menuPanel.Height / 2) - 200 + menuButtonHeight,
			                                    menuButtonWidth, menuButtonHeight,
			                                    multiPlayerTexture, multiPlayerMouseOverTexture);

			btnCredits = new TexturedButton((menuPanel.Width / 2) - (menuButtonWidth / 2),
			                                (menuPanel.Height / 2) - 200 + (menuButtonHeight * 2),
			                                menuButtonWidth, menuButtonHeight,
			                                creditsTexture, creditsMouseOverTexture);

			btnExit = new TexturedButton((menuPanel.Width / 2) - (menuButtonWidth / 2),
			                             (menuPanel.Height / 2) - 200 + (menuButtonHeight * 3),
			                             menuButtonWidth, menuButtonHeight,
			                             exitTexture, exitMouseOverTexture);

			btnSinglePlayer.Click += btnSinglePlayer_Clicked;
			btnMultiPlayer.Click += btnMultiPlayer_Clicked;
			btnCredits.Click += btnCredits_Clicked;
			btnExit.Click += btnExit_Clicked;


			AddAllLocalControls(this);


			//////////////////////////////////////////////////////////////////////////////////////////////////
			// END DESIGNER CODE
			//////////////////////////////////////////////////////////////////////////////////////////////////

			btnSinglePlayer.MouseEnter += button_MouseEnter;
			btnMultiPlayer.MouseEnter += button_MouseEnter;
			btnCredits.MouseEnter += button_MouseEnter;
			btnExit.MouseEnter += button_MouseEnter;
		}
	}
}
