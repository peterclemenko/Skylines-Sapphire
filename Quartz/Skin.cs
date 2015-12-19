using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using ColossalFramework.UI;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Quartz
{

    public class SkinMetadata
    {
        public string Name;
        public string Author;
        public string Path;

	    public bool Legacy;
    }

    public class Skin
    {

        public enum ModuleClass
        {
            MainMenu = 0,
            InGame = 1,
            MapEditor = 2,
            AssetEditor = 3
        }

        public static Skin FromXmlFile(string skinXmlPath, bool autoReloadOnChange)
        {
            Skin skin = null;

            try
            {
                skin = new Skin(skinXmlPath, autoReloadOnChange);
            }
            catch (XmlNodeException ex)
            {
                Debug.LogErrorFormat("{0} while parsing xml \"{1}\" at node \"{2}\": {3}",
                    ex.GetType(), skinXmlPath, ex.Node == null ? "null" : ex.Node.Name, ex.ToString());
            }
            catch (XmlException ex)
            {
                Debug.LogErrorFormat("XmlException while parsing xml \"{0}\" at line {1}, col {2}: {3}",
                    skinXmlPath, ex.LineNumber, ex.LinePosition, ex.Message);
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat("{0} while parsing xml \"{1}\": {2}", ex.GetType(),
                    skinXmlPath, ex.ToString());
            }

            return skin;
        }

        public static SkinMetadata MetadataFromXmlFile(string skinXmlPath)
        {
            SkinMetadata metadata = null;

            try
            {
                var document = new XmlDocument();
                document.LoadXml(File.ReadAllText(skinXmlPath));
				bool legacy = true; // TODO: make conditional
                var root = document.SelectSingleNode("/SapphireSkin"); //TODO: Load Quartz
                if (root == null)
                {
                    throw new ParseException("Skin missing root SapphireSkin node at " + skinXmlPath, null);
                }

                var name = XmlUtil.GetStringAttribute(root, "name");
                var author = XmlUtil.GetStringAttribute(root, "author");
                metadata = new SkinMetadata {Name = name, Author = author, Path = Path.GetDirectoryName(skinXmlPath), Legacy = legacy};
            }
            catch (XmlNodeException ex)
            {
                Debug.LogErrorFormat("{0} while parsing xml \"{1}\" at node \"{2}\": {3}",
                    ex.GetType(), skinXmlPath, ex.Node == null ? "null" : ex.Node.Name, ex.ToString());
            }
            catch (XmlException ex)
            {
                Debug.LogErrorFormat("XmlException while parsing xml \"{0}\" at line {1}, col {2}: {3}",
                    skinXmlPath, ex.LineNumber, ex.LinePosition, ex.Message);
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat("{0} while parsing xml \"{1}\": {2}", ex.GetType(),
                    skinXmlPath, ex.ToString());
            }

            return metadata;
        }

        private readonly Dictionary<ModuleClass, List<SkinModule>> _modules = new Dictionary<ModuleClass, List<SkinModule>>();

        private string name;
        
        public string Name
        {
            get { return name; }
        }

        private string author;

        public string Author
        {
            get { return author; }
        }

        private readonly Dictionary<string, Color32> colorDefinitions = new Dictionary<string, Color32>(); 

        public Color32 GetNamedColor(string colorName)
        {
            if (!colorDefinitions.ContainsKey(colorName))
            {
                throw new ColorNotFoundException(colorName);
            }

            return colorDefinitions[colorName];
        }

        public Dictionary<string, UITextureAtlas> spriteAtlases = new Dictionary<string, UITextureAtlas>();

        private readonly string _sapphirePath;
        private readonly string _skinXmlPath;

        public string SapphirePath
        {
            get { return _sapphirePath; }
        }

        private XmlDocument document;
        private XmlNode root;

        private FileWatcher fileWatcher;

        private Rect renderArea = new Rect(0.0f, 0.0f, 1.0f, 1.0f);

        public Rect RenderArea
        {
            get
            {
                return renderArea;
            }
        }

        public bool IsValid
        {
            get { return isValid; }
        }

        private bool isValid = true;

        private ModuleClass _currentModuleClass;

        private readonly SkinApplicator _applicator;

        public Skin(string skinXmlPath, bool autoReloadOnChange)
        {
            _skinXmlPath = skinXmlPath;
            _sapphirePath = Path.GetDirectoryName(this._skinXmlPath);
            _applicator = new SkinApplicator(this);

            Reload(autoReloadOnChange);
        }

        public void SafeReload(bool autoReloadOnChange)
        {
            try
            {
                Reload(autoReloadOnChange);
            }
            catch (XmlNodeException ex)
            {
                ErrorLogger.LogErrorFormat("{0} while parsing xml \"{1}\" at node \"{2}\": {3}",
                    ex.GetType(), _skinXmlPath, ex.Node == null ? "null" : ex.Node.Name, ex.ToString());
                isValid = false;
            }
            catch (XmlException ex)
            {
                ErrorLogger.LogErrorFormat("XmlException while parsing xml \"{0}\" at line {1}, col {2}: {3}",
                    _skinXmlPath, ex.LineNumber, ex.LinePosition, ex.Message);
                isValid = false;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogErrorFormat("{0} while parsing xml \"{1}\": {2}", ex.GetType(),
                    _skinXmlPath, ex.ToString());
                isValid = false;
            }
        }

        private void Reload(bool autoReloadOnChange)
        {
            isValid = true;

            Dispose();

            _modules[ModuleClass.MainMenu] = new List<SkinModule>();
            _modules[ModuleClass.InGame] = new List<SkinModule>();
            _modules[ModuleClass.MapEditor] = new List<SkinModule>();
            _modules[ModuleClass.AssetEditor] = new List<SkinModule>();

            document = new XmlDocument();
            document.LoadXml(File.ReadAllText(_skinXmlPath));

            if (fileWatcher != null)
            {
                fileWatcher.Dispose();
                fileWatcher = null;
            }

            if (autoReloadOnChange)
            {
                fileWatcher = new FileWatcher(_sapphirePath);
                fileWatcher.WatchFile("skin.xml");
            }

            root = document.SelectSingleNode("/SapphireSkin");
            if (root == null)
            {
                isValid = false;
                throw new ParseException("Skin missing root SapphireSkin node at " + _sapphirePath, null);
            }

            name = XmlUtil.GetStringAttribute(root, "name");
            author = XmlUtil.GetStringAttribute(root, "author");

            LoadSkinSettings();
            LoadSprites();
            LoadColors();
            LoadModules();
        }

        public void Dispose()
        {
            Rollback();
            foreach (var atlas in spriteAtlases)
            {
                Object.Destroy(atlas.Value.material.mainTexture);
                Object.Destroy(atlas.Value);
            }

            colorDefinitions.Clear();

            spriteAtlases.Clear();

            renderArea = new Rect(0.0f, 0.0f, 1.0f, 1.0f);

            if (fileWatcher != null)
            {
                fileWatcher.Dispose();
                fileWatcher = null;
            }
        }

        private void LoadModules()
        {
            foreach (XmlNode childNode in root.ChildNodes)
            {
                if (childNode.Name == "Module")
                {
                    var modulePath = Path.Combine(_sapphirePath, childNode.InnerText);
                    var moduleClass = XmlUtil.GetStringAttribute(childNode, "class");

                    if (moduleClass == "MainMenu")
                    {
                        AddModuleAtPath(ModuleClass.MainMenu, modulePath);
                    }
                    else if (moduleClass == "InGame")
                    {
                        AddModuleAtPath(ModuleClass.InGame, modulePath);
                    }
                    else if (moduleClass == "MapEditor")
                    {
                        AddModuleAtPath(ModuleClass.MapEditor, modulePath);
                    }
                    else if (moduleClass == "AssetEditor")
                    {
                        AddModuleAtPath(ModuleClass.AssetEditor, modulePath);
                    }
                    else
                    {
                        throw new ParseException(String.Format
                            ("Invalid module class \"{0}\"", moduleClass), childNode);
                    }
                }
            }

            CheckAndLoadOverride(ModuleClass.MainMenu);
            CheckAndLoadOverride(ModuleClass.AssetEditor);
            CheckAndLoadOverride(ModuleClass.MapEditor);
            CheckAndLoadOverride(ModuleClass.InGame);
        }

        private void CheckAndLoadOverride(ModuleClass moduleClass)
        {
            var overridePath = Path.Combine(Core.OverrideDirectory, String.Format("{0}_{1}.xml", name, moduleClass.ToString()));
            if (File.Exists(overridePath))
            {
                Debug.LogWarningFormat("Found override for skin \"{0}\" at \"{1}\"", name, overridePath);
                AddModuleAtPath(moduleClass, overridePath);
            }
        }

        private void LoadSkinSettings()
        {
            try
            {
                LoadSkinSettingsInternal();
            }
            catch (XmlNodeException ex)
            {
                ErrorLogger.LogErrorFormat("{0} while loading skin settings for skin \"{1}\" at node \"{2}\": {3}",
                    ex.GetType(), name, ex.Node == null ? "null" : ex.Node.Name, ex.ToString());
                isValid = false;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogErrorFormat("{0} while loading skin settings for skin \"{1}\": {2}", ex.GetType(), name, ex.Message);
                isValid = false;
            }
        }

        private void LoadSkinSettingsInternal()
        {
            Debug.LogWarning("Loading skin settings");

            var rootSettingsNode = document.SelectSingleNode("/SapphireSkin/SkinSettings"); //TODO: make Quartz with fallback

            if (rootSettingsNode == null)
            {
                Debug.LogWarning("Skin defines no settings");
                return;
            }

            foreach (XmlNode childNode in rootSettingsNode)
            {
                var settingsKey = childNode.Name;
                var text = childNode.InnerText;

                if (text.Length == 0)
                {
                    throw new ParseException(String.Format("Empty value for settings key \"{0}\"", settingsKey), childNode);
                }

                if (settingsKey == "InGameRenderArea")
                {
                    var rect = (Rect)XmlUtil.GetValueForType(childNode, typeof (Rect), childNode.InnerText, null);
                    Debug.LogWarning(String.Format("Setting skin \"InGameRenderArea\" to {0}", rect));

                    rect.width /= 1920.0f;
                    rect.height /= 1080.0f;

                    rect.x /= 1920.0f;
                    rect.y /= 1080.0f;
                    rect.y = 1.0f - rect.y - rect.height;

                    renderArea = rect;
                }
            }
        }

        private void LoadColors()
        {
            try
            {
                LoadColorsInternal();
            }
            catch (XmlNodeException ex)
            {
                ErrorLogger.LogErrorFormat("{0} while loading colors for skin \"{1}\" at node \"{2}\": {3}",
                    ex.GetType(), name, ex.Node == null ? "null" : ex.Node.Name, ex.ToString());
                isValid = false;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogErrorFormat("Exception while loading colors for skin \"{0}\": {1}", name, ex.Message);
                isValid = false;
            }
        }

        private void LoadColorsInternal()
        {
            Debug.LogWarning("Loading colors");

            var rootColorsNode = document.SelectSingleNode("/SapphireSkin/Colors");

            if (rootColorsNode == null)
            {
                Debug.LogWarning("Skin defines no colors");
                return;
            }

            foreach (XmlNode childNode in rootColorsNode)
            {
                if (childNode.Name != "Color")
                {
                    continue;
                }

                var colorName = XmlUtil.GetStringAttribute(childNode, "name");
                if (colorDefinitions.ContainsKey(colorName))
                {
                    Debug.LogWarningFormat("Duplicate color name \"{0}\", ignoring second definition..", colorName);
                    continue;
                }

                var text = childNode.InnerText;

                if (text.Length == 0)
                {
                    throw new ParseException(String.Format("Empty color value for color \"{0}\"", colorName), childNode);
                }

                Color32 color;

                if (text[0] == '#')
                {
                    int colorHex = Int32.Parse(text.Replace("#", ""), NumberStyles.HexNumber);
                    byte r = (byte)((colorHex >> 16) & 0xFF);
                    byte g = (byte)((colorHex >> 8) & 0xFF);
                    byte b = (byte)((colorHex) & 0xFF);
                    color = new Color32(r, g, b, 255);
                }
                else
                {
                    var values = text.Split(',');
                    if (values.Length != 4)
                    {
                        throw new ParseException("Color32 definition must have four components", childNode);
                    }

                    color = new Color32(byte.Parse(values[0]), byte.Parse(values[1]), byte.Parse(values[2]), byte.Parse(values[3]));
                }
                
                colorDefinitions.Add(colorName, color);
                Debug.LogWarningFormat("Color \"{0}\" defined as \"{1}\"", colorName, color.ToString());
            }
        }

        private void LoadSprites()
        {
            try
            {
                LoadSpritesInternal();
            }
            catch (XmlNodeException ex)
            {
                ErrorLogger.LogErrorFormat("{0} while loading sprites for skin \"{1}\" at node \"{2}\": {3}",
                    ex.GetType(), name, ex.Node == null ? "null" : ex.Node.Name, ex.ToString());
                isValid = false;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogErrorFormat("{0} while loading sprites for skin \"{1}\": {2}", ex.GetType(), name, ex.Message);
                isValid = false;
            }
        }

        private void LoadSpritesInternal()
        {
            Debug.LogWarning("Loading sprites");

            foreach (XmlNode childNode in root)
            {
                if (childNode.Name != "SpriteAtlas")
                {
                    continue;
                }

                var atlasName = XmlUtil.GetStringAttribute(childNode, "name");
                if (spriteAtlases.ContainsKey(atlasName))
                {
                    throw new ParseException(String.Format("Duplicate atlas name \"{0}\"", atlasName), childNode);
                }

                Debug.LogWarningFormat("Generating atlas \"{0}\"", atlasName);

                var atlasPacker = new AtlasPacker();

                int count = 0;
                foreach (XmlNode spriteNode in childNode.ChildNodes)
                {
                    var path = spriteNode.InnerText;
                    if (fileWatcher != null)
                    {
                        fileWatcher.WatchFile(path);
                    }

                    var name = XmlUtil.GetStringAttribute(spriteNode, "name");
                    Debug.LogWarningFormat("Found definition for sprite \"{0}\" in atlas \"{1}\"", name, atlasName);

                    var fullPath = Path.Combine(_sapphirePath, path);

                    if (!File.Exists(fullPath))
                    {
                        throw new FileNotFoundException(String.Format("Sprite at path \"{0}\" not found!", fullPath), fullPath);
                    }

                    atlasPacker.AddSprite(name, fullPath);
                    count++;
                }

                Debug.LogWarningFormat("Added {0} sprites to atlas \"{1}\"", count, atlasName);
                
                try
                {
                    spriteAtlases[atlasName] = atlasPacker.GenerateAtlas(atlasName);
                }
                catch (AtlasPacker.TooManySprites)
                {
                    ErrorLogger.LogError("Too many sprites in atlas \"" + atlasName + "\", move some sprites to a new atlas!");
                    break;
                }

                Debug.LogWarningFormat("Atlas \"{0}\" generated", atlasName);
            }
        }

        private void AddModuleAtPath(ModuleClass moduleClass, string modulePath)
        {
            if (fileWatcher != null)
            {
                fileWatcher.WatchFile(modulePath);
            }

            var name = Path.GetFileNameWithoutExtension(modulePath);
            if (name == null)
            {
                throw new Exception(String.Format("Invalid skin module path \"{0}\"", modulePath));
            }

            var module = SkinModule.FromXmlFile(modulePath);
            if (module == null)
            {
                isValid = false;
                return;
            }

			Debug.LogWarning("Adding module: " + module.SourcePath + " With class: " + moduleClass);

            _modules[moduleClass].Add(module);
        }

        public void ApplyStickyProperties(ModuleClass moduleClass)
        {
            if (!isValid)
            {
                return;
            }

            _applicator.ApplyStickyProperties();
        }

        public void Apply(ModuleClass moduleClass)
        {
            if (!isValid)
            {
                Debug.LogWarning("Trying to apply an invalid skin");
                return;
            }

            _currentModuleClass = moduleClass;

			Debug.LogWarning("Trying to load modules for class: " + moduleClass);

            if (!_applicator.Apply(_modules[moduleClass]))
            {
                _applicator.Rollback();
                Debug.LogWarning("Failed to apply skin module (look for an error in the messages above). All changes have been reverted.");
                isValid = false;
            }
          
            SetCameraRectHelper.CameraRect = renderArea;
        }

        public void Rollback()
        {
            if (!isValid)
            {
                Debug.LogWarning("Trying to roll-back an invalid skin");
                return;
            }
            
            _applicator.Rollback();
            SetCameraRectHelper.ResetCameraRect();
        }

        public void ReloadIfChanged()
        {
            if (fileWatcher != null)
            {
                if (fileWatcher.CheckForAnyChanges())
                {
                    SafeReload(fileWatcher != null);
                    Apply(_currentModuleClass);
                }
            }
        }

    }
}
