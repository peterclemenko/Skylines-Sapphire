using System;
using System.IO;
using System.Xml.Serialization;
using ColossalFramework.IO;
using UnityEngine;

namespace Quartz
{

	public class Configuration
	{

		public bool ShowQuartzIconInGame = true;
		public bool ApplySkinOnStartup = false;
		public string SelectedSkinPath = "";
		public bool IgnoreMissingComponents = false;

		private void OnPreSerialize()
		{
		}

		private void OnPostDeserialize()
		{
		}

		public void Serialize(string filename)
		{
			var serializer = new XmlSerializer(typeof (Configuration));

			if (!Directory.Exists(Path.GetDirectoryName(filename)))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(filename));
			}

			using (var writer = new StreamWriter(filename))
			{
				OnPreSerialize();
				serializer.Serialize(writer, this);
			}
		}

		public Configuration Deserialize(string filename)
		{
			var serializer = new XmlSerializer(typeof (Configuration));

			try
			{
				using (var reader = new StreamReader(filename))
				{
					var config = (Configuration) serializer.Deserialize(reader);
					OnPostDeserialize();
					return config;
				}
			}
			catch(Exception ex)
			{
				Debug.LogError("Could not load configuration " + ex);
			}

			return null;
		}
	}
}
