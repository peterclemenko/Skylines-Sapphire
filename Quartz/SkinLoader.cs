using System.Collections.Generic;
using System.IO;
using ColossalFramework.Plugins;
using UnityEngine;

namespace Quartz
{

    public class SkinLoader
    {

        public static List<SkinMetadata> FindAllSkins()
        {
            var skins = new List<SkinMetadata>();

            var plugins = PluginManager.instance.GetPluginsInfo();
            foreach (var plugin in plugins)
            {
                if (!plugin.isEnabled)
                {
                    continue;
                }

                var path = plugin.modPath;
				var sapphirePath = Path.Combine(path, "_SapphireSkin"); //Legacy path
				var quartzPath = Path.Combine(path, "_QuartzSkin");
	            string filePath;

				if (Directory.Exists(quartzPath))
				{
					filePath = quartzPath;
				}
                else if (Directory.Exists(sapphirePath)) //Fallback for legacy
                {
					filePath = sapphirePath;
                }
				else
				{
					continue;
				}

				if (!File.Exists(Path.Combine(filePath, "skin.xml")))
                {
					Debug.LogWarningFormat("\"skin.xml\" not found in \"{0}\", skipping", filePath);
                    continue;
                }

				var metadata = Skin.MetadataFromXmlFile(Path.Combine(filePath, "skin.xml"));
                if (metadata != null)
                {
                    skins.Add(metadata);
                }
            }

            return skins;
        }

    }

}
