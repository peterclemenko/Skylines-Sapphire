using System;
using System.Xml;
using ColossalFramework.UI;

namespace Quartz
{

    public class XmlNodeException : Exception
    {
        private readonly XmlNode _nodeInternal;
        public XmlNode Node
        {
            get { return _nodeInternal; }
        }

        public XmlNodeException(XmlNode node)
        {
            _nodeInternal = node;
        }
    }

    public class ParseException : XmlNodeException
    {
        private readonly string _msgInternal;

        public ParseException(string msg, XmlNode node)
            : base(node)
        {
            _msgInternal = msg;
        }

        public override string ToString()
        {
            return _msgInternal;
        }
    }

    public class MissingComponentPropertyException : XmlNodeException
    {
        private readonly string _propertyNameInternal;
        private readonly UIComponent _componentInternal;

        public string PropertyName
        {
            get { return _propertyNameInternal; }
        }

        public UIComponent Component
        {
            get { return _componentInternal; }
        }

        public MissingComponentPropertyException(string propertyName, UIComponent component, XmlNode node)
            : base(node)
        {
            _propertyNameInternal = propertyName;
            _componentInternal = component;
        }

        public override string ToString()
        {
            return String.Format("Missing component property \"{0}\" for component \"{1}\"",
                _propertyNameInternal, _componentInternal == null ? "null" : Component.name);
        }
    }

    public class MissingAttributeException : XmlNodeException
    {
        private readonly string _attributeInternal;

        public string Attribute
        {
            get { return _attributeInternal; }
        }

        public MissingAttributeException(string attribute, XmlNode node)
            : base(node)
        {
            _attributeInternal = attribute;
        }

        public override string ToString()
        {
            return String.Format("Missing or malformed attribute - \"{0}\"", _attributeInternal);
        }
    }

    public class MissingAttributeValueException : XmlNodeException
    {
        private readonly string _attributeInternal;

        public string Attribute
        {
            get { return _attributeInternal; }
        }

        public MissingAttributeValueException(string attribute, XmlNode node)
            : base(node)
        {
            _attributeInternal = attribute;
        }

        public override string ToString()
        {
            return String.Format("Missing or malformed attribute value - \"{0}\"", _attributeInternal);
        }
    }

    public class MissingUIComponentException : XmlNodeException
    {
        private readonly string _componentNameInternal;
        private readonly UIComponent _componentParentInternal;

        public string ComponentName
        {
            get { return _componentNameInternal; }
        }

        public UIComponent ComponentParent
        {
            get { return _componentParentInternal; }
        }

        public MissingUIComponentException(string componentName, UIComponent parent, XmlNode node)
            : base(node)
        {
            _componentNameInternal = componentName;
            _componentParentInternal = parent;
        }

        public override string ToString()
        {
            return String.Format("Missing UI component - \"{0}\" with parent \"{1}\"",
                _componentNameInternal, _componentParentInternal == null ? "None" : _componentParentInternal.name);
        }
    }

    public class UnsupportedTypeException : XmlNodeException
    {
        private readonly Type _typeInternal;

        public Type Type
        {
            get { return _typeInternal; }
        }

        public UnsupportedTypeException(Type type, XmlNode node)
            : base(node)
        {
            _typeInternal = type;
        }

        public override string ToString()
        {
            return String.Format("Unsupported type \"{0}\"", _typeInternal);
        }
    }

    public class SpriteAtlasNotFoundException : Exception
    {
        private readonly string _atlasNameInternal;

        public string AtlasName
        {
            get { return _atlasNameInternal; }
        }


        public SpriteAtlasNotFoundException(string atlasName)
        {
            _atlasNameInternal = atlasName;
        }

        public override string ToString()
        {
            return String.Format("Failed to find atlas \"{0}\" in skin.xml", _atlasNameInternal);
        }
    }

    public class SpriteNotFoundException : Exception
    {
        private readonly UITextureAtlas _atlasInternal;
        private readonly string _spriteNameInternal;

        public UITextureAtlas Atlas
        {
            get { return _atlasInternal; }
        }

        public string SpriteName
        {
            get { return _spriteNameInternal; }
        }

        public SpriteNotFoundException(string spriteName, UITextureAtlas atlas)
        {
            _spriteNameInternal = spriteName;
            _atlasInternal = atlas;
        }

        public override string ToString()
        {
            return String.Format("Failed to find sprite \"{0}\" in atlas \"{1}\"", _spriteNameInternal, _atlasInternal.name);
        }
    }

    public class ColorNotFoundException : Exception
    {
        private readonly string _colorNameInternal;

        public string ColorName
        {
            get {  return _colorNameInternal; }
        }

        public ColorNotFoundException(string colorName)
        {
            _colorNameInternal = colorName;
        }

        public override string ToString()
        {
            return String.Format("Failed to find definition for color \"{0}\" in skin.xml", _colorNameInternal);
        }
    }

}
