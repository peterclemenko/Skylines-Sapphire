using System;
using ColossalFramework.Plugins;
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
				Debug.Log("Name accessed");
	            return "Quartz";
            }
        }

        public string Description
        {
			get
			{
				try
				{
					Debug.Log("Description accessed");
					Core.Bootstrap(Skin.ModuleClass.MainMenu);
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
				}
				return "UI reskin framework";
			}
        }

		public void OnEnabled()
		{
		}

		public void OnDisabled()
		{
			try
			{
				Core.Deinitialize();
			}
			catch
			{
				Debug.LogError("Failed to deinitialize maybe the mod isn't loaded");
			}
		}

    }

    public class ModLoad : LoadingExtensionBase
    {

        public override void OnLevelLoaded(LoadMode mode)
        {
			Debug.Log("OnLevelLoaded fired");
	        try
	        {
		        Debug.LogWarning("LoadMode is: " + Enum.GetName(typeof (LoadMode), mode) + " Using that to set moduleClass");
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
