using System;
using ICities;
using UnityEngine;

namespace Quartz
{

    public class Mod : IUserMod
    {

        public string Name
        {
            get
            {
                try
                {
                    Core.Bootstrap(Skin.ModuleClass.MainMenu);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }

                return "Quartz";
            }
        }

        public string Description
        {
            get { return "UI reskin framework"; }
        }

    }

    public class ModLoad : LoadingExtensionBase
    {

        public override void OnLevelLoaded(LoadMode mode)
        {
            try
            {
				Debug.LogWarning("LoadMode is: " + Enum.GetName(typeof(LoadMode), mode) + " Using that to set moduleClass");
                if (mode == LoadMode.NewGame || mode == LoadMode.LoadGame)
                {
                    Core.Bootstrap(Skin.ModuleClass.InGame);
                }
                else if (mode == LoadMode.NewMap || mode == LoadMode.LoadMap)
                {
                    Core.Bootstrap(Skin.ModuleClass.MapEditor);
                }
                else if (mode == LoadMode.NewAsset || mode == LoadMode.LoadAsset)
                {
                    Core.Bootstrap(Skin.ModuleClass.AssetEditor);
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public override void OnLevelUnloading()
        {
            Core.Deinitialize();
        }
    }

}
