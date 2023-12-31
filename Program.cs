using System;
using System.Windows.Forms;
using Microsoft.Xna.Framework.Graphics;

namespace AsteroidOutpost
{
	static class Program
	{
		private const String usageString = "asteroidoutpost <arguments>";  // TODO: put in an actual usage string that describes what may go wrong

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main(string[] args)
		{
			AOGame game;

			bool move = false;
			int x = 0;
			int y = 0;

			int width;
			int height;
			bool fullScreen;
			bool performanceGraph = false;

#if DEBUG
			move = true;
			width = 1920;
			height = 1080;
			fullScreen = false;
			performanceGraph = false;
#else
			width = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
			height = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
			fullScreen = true;
#endif

			if (processArgs(args, ref move, ref x, ref y, ref width, ref height, ref fullScreen, ref performanceGraph) == false)
			{
				// Problem in command-line processing, so exit.
				return;
			}

			try
			{
				if (!move)
				{
					game = new AOGame(width, height, fullScreen, performanceGraph);
				}
				else
				{
					game = new AOGame(x, y, width, height, performanceGraph);
				}

				// Auto-disposes when leaving the using statement
				using (game)
				{
					game.Run();
				}
				
			}
			catch (NoSuitableGraphicsDeviceException)
			{
				System.Windows.Forms.MessageBox.Show("No suitable graphics device was found on this computer. Sorry.", "NoSuitableGraphicsDeviceException", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private static bool processArgs(string[] args, ref bool move, ref int x, ref int y, ref int width, ref int height, ref bool fullScreen, ref bool performanceGraph)
		{
			try
			{
				for (int i = 0; i < args.Length; i++)
				{
					string lowered = args[i].ToLower();
					switch (lowered)
					{
						// Performance Graph
						case "-p":
						case "\\p":
						case "/p":
							performanceGraph = true;
							break;
						// Fullscreen
						case "-f":
						case "\\f":
						case "/f":
							fullScreen = true;
							break;
						// Windowed
						case "-win":
						case "\\win":
						case "/win":
							fullScreen = false;
							break;
						// Window X
						case "-x":
						case "\\x":
						case "/x":
							x = int.Parse(args[++i]);
							move = true;
							break;
						// Window Y
						case "-y":
						case "\\y":
						case "/y":
							y = int.Parse(args[++i]);
							move = true;
							break;
						// Window Width
						case "-w":
						case "\\w":
						case "/w":
							width = int.Parse(args[++i]);
							break;
						// Window Height
						case "-h":
						case "\\h":
						case "/h":
							height = int.Parse(args[++i]);
							break;
						// Default for error
						default:
							throw new ArgumentException("Unknown command line argument '" + lowered + "' encountered.");
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error while processing command-line arguments.  Error was: " + ex.Message
					+ System.Environment.NewLine + "Correct format is: " + usageString);
				return false;
			}

			return true;
		}
	}
}