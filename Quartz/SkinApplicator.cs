using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using ColossalFramework.UI;
using UnityEngine;

namespace Quartz
{

    public class SkinApplicator
    {

        struct StickyProperty
        {
            public XmlNode ChildNode;
            public XmlNode Node;
            public UIComponent Component;
        }

        private readonly Skin _skin;

        private List<StickyProperty> _stickyProperties = new List<StickyProperty>();

        private delegate void RollbackAction();
        private List<RollbackAction> _rollbackStack = new List<RollbackAction>();
        public Dictionary<object, List<KeyValuePair<PropertyInfo, object>>> RollbackDataMap = new Dictionary<object, List<KeyValuePair<PropertyInfo, object>>>();

        private AspectRatio _currentAspectRatio = AspectRatio.R16_9;

        public SkinApplicator(Skin _skin)
        {
            this._skin = _skin;
        }

        private void ApplyUIMultiStateButtonSpriteStateProperty(XmlNode node, UIComponent component)
        {
            var index = XmlUtil.GetIntAttribute(node, "index");

            var type = XmlUtil.GetStringAttribute(node, "type");
            if (type != "background" && type != "foreground")
            {
                throw new ParseException(String.Format
                    ("Invalid value for SpriteState attribute \"type\" (only \"foreground\" and \"background\" are allowed - \"{0}\"",
                        index), node);
            }

            var button = component as UIMultiStateButton;
            UIMultiStateButton.SpriteSetState sprites = null;

	        if (button != null) sprites = type == "background" ? button.backgroundSprites : button.foregroundSprites;

	        if (sprites != null && index >= sprites.Count)
            {
                throw new ParseException(String.Format
                ("Invalid value for SpriteState attribute \"index\", object has only \"{1}\" states - \"{0}\"",
                   index, sprites.Count), node);
            }

            foreach (XmlNode stateNode in node.ChildNodes)
            {
                try
                {
	                if (sprites != null)
	                {
		                var property = ReflectionCache.GetPropertyForType(sprites[index].GetType(), stateNode.Name);
		                if (property == null)
		                {
			                throw new ParseException(String.Format
				                                         ("Invalid property \"{0}\" for SpriteState, allowed are \"normal\", \"hovered\", \"focused\", \"pressed\", \"disabled\"",
				                                          stateNode.InnerText), node);
		                }

		                SetPropertyValueWithRollback(sprites[index], property, stateNode.InnerText);
	                }
                }
                catch (Exception ex)
                {
                    throw new ParseException(String.Format
                        ("Exception while processing SpriteState node - {0}",
                            ex), node);
                }
            }
        }

        private void ApplyGenericProperty(XmlNode node, UIComponent component)
        {
            bool optional = XmlUtil.TryGetBoolAttribute(node, "optional");
            bool sticky = XmlUtil.TryGetBoolAttribute(node, "sticky");
            string aspect = XmlUtil.TryGetStringAttribute(node, "aspect", "any");
            if (aspect != "any")
            {
                if (Util.AspectRatioFromString(aspect) != _currentAspectRatio)
                {
                    return;
                }
            }

            if (sticky)
            {
                _stickyProperties.Add(new StickyProperty
                {
                    ChildNode = node,
                    Component = component,
                    Node = node
                });
            }

            SetPropertyValue(node, node, component, optional, true);
        }

        private void SetPropertyValue(XmlNode setNode, XmlNode node, UIComponent component, bool optional, bool rollback)
        {
            var property = ReflectionCache.GetPropertyForType(component.GetType(), setNode.Name);

            if (property == null)
            {
                if (optional)
                {
                    return;
                }

                throw new MissingComponentPropertyException(setNode.Name, component, node);
            }

            if (!property.CanWrite)
            {
                throw new ParseException(String.Format("Property \"{0}\" of component \"{1}\" is read-only", property.Name, component.name), setNode);
            }

            object value;

            bool raw = XmlUtil.TryGetBoolAttribute(setNode, "raw");

            if (property.PropertyType == typeof(Color32) && !raw)
            {
                value = _skin.GetNamedColor(setNode.InnerText);
            }
            else
            {
                value = XmlUtil.GetValueForType(setNode, property.PropertyType, setNode.InnerText, _skin.spriteAtlases);
            }

            if (rollback)
            {
                SetPropertyValueWithRollback(component, property, value);
            }
            else
            {
                SetPropertyValue(component, property, value);
            }
        }

        private void SetPropertyValueWithRollback(object component, PropertyInfo property, object value)
        {
            if (!RollbackDataMap.ContainsKey(component))
            {
                RollbackDataMap.Add(component, new List<KeyValuePair<PropertyInfo, object>>());
            }

            bool valueFound = false;
            object originalValue = null;
            foreach (var item in RollbackDataMap[component])
            {
                if (item.Key == property)
                {
                    originalValue = item.Value;
                    valueFound = true;
                    break;
                }
            }

            if (!valueFound)
            {
                originalValue = property.GetValue(component, null);
                RollbackDataMap[component].Add(new KeyValuePair<PropertyInfo, object>(property, originalValue));
            }

	        if (originalValue == value) return;
	        SetPropertyValue(component, property, value);

	        _rollbackStack.Add(() => property.SetValue(component, originalValue, null));
        }

        private void SetPropertyValue(object component, PropertyInfo property, object value)
        {
            property.SetValue(component, value, null);
        }

        public void ApplyStickyProperties()
        {
            try
            {
                ApplyStickyPropertiesInternal();
            }
            catch (ParseException ex)
            {
                ErrorLogger.LogErrorFormat("Error while applying sticky properties for skin \"{1}\" at node \"{2}\": {3}",
                    ex.GetType(), _skin.Name, XmlUtil.XmlNodeInfo(ex.Node), ex.ToString());
            }
            catch (XmlNodeException ex)
            {
                ErrorLogger.LogErrorFormat("{0} while applying sticky properties for skin \"{1}\" at node \"{2}\": {3}",
                    ex.GetType(), _skin.Name, XmlUtil.XmlNodeInfo(ex.Node), ex.ToString());
            }
            catch (Exception ex)
            {
                ErrorLogger.LogErrorFormat("{0} while applying sticky properties for skin \"{1}\": {2}", ex.GetType(), _skin.Name, ex.ToString());
            }
        }

        private void ApplyStickyPropertiesInternal()
        {
            foreach (var property in _stickyProperties)
            {
                SetPropertyValue(property.ChildNode, property.Node, property.Component, true, false);
            }
        }

        public void Rollback()
        {
            Debug.LogWarningFormat("Rolling back changes");

            _stickyProperties = new List<StickyProperty>();

            try
            {
                RollbackInternal();
            }
            catch (Exception ex)
            {
                ErrorLogger.LogErrorFormat("{0} during skin \"{1}\" rollback: {2}", ex.GetType(), _skin.Name, ex.ToString());
                return;
            }

            Debug.LogFormat("Skin \"{0}\" successfully rolled back!", _skin.Name);
        }

        private void RollbackInternal()
        {
            _rollbackStack.Reverse();

            foreach (var action in _rollbackStack)
            {
                action();
            }

            _rollbackStack.Clear();
        }

        public bool Apply(List<SkinModule> skinModules)
        {
            _currentAspectRatio = Util.AspectRatioFromResolution(Screen.width, Screen.height);

            _stickyProperties = new List<StickyProperty>();
            _rollbackStack = new List<RollbackAction>();

            foreach (var skinModule in skinModules)
            {
                Debug.LogFormat("Applying skin module \"{0}\"", skinModule.SourcePath);

                try
                {
                    ApplyInternal(skinModule);
                    Debug.LogFormat("Skin module \"{0}\" successfully applied!", skinModule.SourcePath);
                }
                catch (ParseException ex)
                {
                    ErrorLogger.LogErrorFormat("Error while applying skin module \"{1}\" at node \"{2}\": {3}",
                        ex.GetType(), skinModule.SourcePath, XmlUtil.XmlNodeInfo(ex.Node), ex.ToString());
                    return false;
                }
                catch (XmlNodeException ex)
                {
                    ErrorLogger.LogErrorFormat("{0} while applying skin module \"{1}\" at node \"{2}\": {3}",
                        ex.GetType(), skinModule.SourcePath, XmlUtil.XmlNodeInfo(ex.Node), ex.ToString());
                    return false;
                }
                catch (Exception ex)
                {
                    ErrorLogger.LogErrorFormat("{0} while applying skin module \"{1}\": {2}", ex.GetType(), skinModule.SourcePath, ex.ToString());
                    return false;
                }
            }

            return true;
        }

        private void ApplyInternal(SkinModule skinModule)
        {
            skinModule.WalkModule(ApplyInternalRecursive);
        }

        private void ApplyInternalRecursive(XmlNode node, UIComponent component)
        {
            if (component != null)
            {
                if (component.GetType() == typeof(UIMultiStateButton) && node.Name == "SpriteState")
                {
                    ApplyUIMultiStateButtonSpriteStateProperty(node, component);
                }
                else
                {
                    ApplyGenericProperty(node, component);
                }
            }
            else
            {
                throw new ParseException("Setting properties on the UIView object is not allowed!", node);
            }
        }

    }

}
