using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using ColossalFramework.IO;
using UnityEngine;

namespace Quartz
{
	class ConfigManager
	{
		public static bool ShowQuartzIconInGame
		{
			get
			{
				if(_config == null)
				{
					LoadConfig();
				}
				return _config.ShowQuartzIconInGame;
			}
			set
			{
				_config.ShowQuartzIconInGame = value;
				SaveConfig();
			}
		}

		public static bool ApplySkinOnStartup
		{
			get
			{
				if (_config == null)
				{
					LoadConfig();
				}
				return _config.ApplySkinOnStartup;
			}
			set
			{
				_config.ApplySkinOnStartup = value;
				SaveConfig();
			}
		}
		public static string SelectedSkinPath
		{
			get
			{
				if (_config == null)
				{
					LoadConfig();
				}
				return _config.SelectedSkinPath;
			}
			set
			{
				_config.SelectedSkinPath = value;
				SaveConfig();
			}
		}
		public static bool IgnoreMissingComponents
		{
			get
			{
				if (_config == null)
				{
					LoadConfig();
				}
				return _config.IgnoreMissingComponents;
			}
			set
			{
				_config.IgnoreMissingComponents = value;
				SaveConfig();
			}
		}

		private static readonly string FileName = Path.Combine(DataLocation.localApplicationData,
												Path.Combine("Quartz", "QuartzConfig.xml"));
		private static Configuration _config;
		
		private static void LoadConfig()
		{
			if (_config == null)
			{
				_config = new Configuration();
			}
			_config = _config.Deserialize(FileName);
		}

		private static void SaveConfig()
		{
			_config.Serialize(FileName);
		}
	}
}
