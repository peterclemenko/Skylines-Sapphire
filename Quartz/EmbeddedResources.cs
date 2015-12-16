using System;
using System.Reflection;
using ColossalFramework.UI;
using UnityEngine;

namespace Quartz
{
    public static class EmbeddedResources
    {

        private static UITextureAtlas _atlas;

		public static UITextureAtlas GetQuartzAtlas()
        {
            if (_atlas == null)
            {
                var atlasPacker = new AtlasPacker();
				atlasPacker.AddSprite("QuartzIcon", GetTextureResource("QuartzIcon.png"));
				atlasPacker.AddSprite("QuartzIconHover", GetTextureResource("QuartzIconHover.png"));
				atlasPacker.AddSprite("QuartzIconPressed", GetTextureResource("QuartzIconPressed.png"));
                atlasPacker.AddSprite("DefaultPanelBackground", GetTextureResource("DefaultPanelBackground.png"));
				_atlas = atlasPacker.GenerateAtlas("QuartzIconsAtlas");
            }

            return _atlas;
        }

        private static Texture2D GetTextureResource(string fileName)
        {
            var texture = new Texture2D(0, 0);
            texture.LoadImage(GetResource(String.Format("Quartz.Resources.{0}", fileName)));
            return texture;
        }

        private static byte[] GetResource(string name)
        {
            var asm = Assembly.GetExecutingAssembly();
            var stream = asm.GetManifestResourceStream(name);
	        if (stream != null)
	        {
		        byte[] data = new byte[stream.Length];
		        stream.Read(data, 0, (int)stream.Length);
		        return data;
	        }
	        return null;
        }

    }

}
