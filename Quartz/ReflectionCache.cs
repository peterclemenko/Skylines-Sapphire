using System.Collections.Generic;
using System;
using System.Reflection;

namespace Quartz
{
    public static class ReflectionCache
    {

        private static readonly Dictionary<Type, Dictionary<string, PropertyInfo>> Cache = new Dictionary<Type, Dictionary<string, PropertyInfo>>();

        public static PropertyInfo GetPropertyForType(Type type, string name)
        {
            if (!Cache.ContainsKey(type))
            {
                Cache[type] = new Dictionary<string, PropertyInfo>(); 
            }

            if (!Cache[type].ContainsKey(name))
            {
                Cache[type][name] = type.GetProperty(name);
            }

            return Cache[type][name];
        }

    }

}
