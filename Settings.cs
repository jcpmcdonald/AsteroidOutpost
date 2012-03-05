using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AsteroidOutpost
{
	class Settings
	{
		public float MusicVolume { get; private set; }
		public bool DisplayRangeNames { get; private set; }


		public Settings()
		{
			// Load some default values
			MusicVolume = 0.00f;//0.80f;
			DisplayRangeNames = true;
		}

		public Settings(BinaryReader br)
		{
			MusicVolume = br.ReadSingle();
			DisplayRangeNames = br.ReadBoolean();
		}


		public void Save(String filename)
		{
			using (var file = new FileStream(filename, FileMode.Truncate))
			{
				Save(new BinaryWriter(file));
				file.Flush();
			}
		}

		public void Save(BinaryWriter bw)
		{
			bw.Write(MusicVolume);
			bw.Write(DisplayRangeNames);
		}
	}
}
