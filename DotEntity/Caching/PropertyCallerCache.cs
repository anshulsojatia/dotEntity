// #region Author Information
// // PropertyCallerCache.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using DotEntity.Extensions;
using DotEntity.Reflection;

namespace DotEntity.Caching
{
    internal static class PropertyCallerCache
    {
        public class OfType
        {
            private ConcurrentDictionary<string, Delegate> _setter = null;

            public ConcurrentDictionary<string, Delegate> Setter
            {
                get
                {
                    if (_setter == null)
                        InitSetter();

                    return _setter;
                }
            }


            private ConcurrentDictionary<string, Delegate> _getter = null;

            public ConcurrentDictionary<string, Delegate> Getter
            {
                get
                {
                    if (_getter == null)
                        InitGetter();

                    return _getter;
                }
            }

            private readonly Type _type;

            public OfType(Type type)
            {
                _type = type;
            }

            private void InitSetter()
            {
                _setter = new ConcurrentDictionary<string, Delegate>();
                //exclude virtual properties
                var typeProperties = _type.GetDatabaseUsableProperties();
                foreach (var property in typeProperties)
                {

                    var propertyType = property.PropertyType;
                    Delegate setter = null;
                    if (propertyType == typeof(int))
                        setter = property.CreateSetter<int>(_type);
                    else if (propertyType == typeof(string))
                        setter = property.CreateSetter<string>(_type);
                    else if (propertyType == typeof(DateTime))
                        setter = property.CreateSetter<DateTime>(_type);
                    else if (propertyType == typeof(decimal))
                        setter = property.CreateSetter<decimal>(_type);
                    else if (propertyType == typeof(double))
                        setter = property.CreateSetter<double>(_type);
                    else if (propertyType == typeof(bool))
                        setter = property.CreateSetter<bool>(_type);

                    _setter.TryAdd(property.Name, setter);
                }
            }

            private void InitGetter()
            {
                _getter = new ConcurrentDictionary<string, Delegate>();
                //exclude virtual properties
                var typeProperties = _type.GetDatabaseUsableProperties();
                foreach (var property in typeProperties)
                {

                    var propertyType = property.PropertyType;
                    Delegate getter = null;
                    if (propertyType == typeof(int))
                        getter = property.CreateGetter<int>(_type);
                    else if (propertyType == typeof(string))
                        getter = property.CreateGetter<string>(_type);
                    else if (propertyType == typeof(DateTime))
                        getter = property.CreateGetter<DateTime>(_type);
                    else if (propertyType == typeof(decimal))
                        getter = property.CreateGetter<decimal>(_type);
                    else if (propertyType == typeof(double))
                        getter = property.CreateGetter<double>(_type);
                    else if (propertyType == typeof(bool))
                        getter = property.CreateGetter<bool>(_type);

                    _getter.TryAdd(property.Name, getter);
                }
            }
        }

        private static readonly ConcurrentDictionary<Type, OfType> Cache;
        static PropertyCallerCache()
        {
            Cache = new ConcurrentDictionary<Type, OfType>();
        }

        private static OfType GetOfType(Type type)
        {
            if (!Cache.TryGetValue(type, out OfType ofType))
            {
                ofType = new OfType(type);
                Cache.TryAdd(type, ofType);
            }
            return ofType;
        }

        public static PropertyReader GetterOfType(Type type)
        {
            return new PropertyReader(GetOfType(type).Getter);
        }

        public static PropertyWriter SetterOfType(Type type)
        {
            return new PropertyWriter(GetOfType(type).Setter);
        }

    }

    internal class PropertyReaderWriter
    {
        protected ConcurrentDictionary<string, Delegate> _getterMap;
        protected ConcurrentDictionary<string, Delegate> _setterMap;

        public ICollection<string> Keys => _setterMap?.Keys;
    }

    internal class PropertyReader : PropertyReaderWriter
    {
        public PropertyReader(ConcurrentDictionary<string, Delegate> getterMap)
        {
            _getterMap = getterMap;
        }

        public TType Get<TType>(object instance, string propertyName)
        {
            if (!_getterMap.TryGetValue(propertyName, out Delegate callback))
            {
                return default(TType);
            }
            return (TType) callback.GetMethodInfo().Invoke(instance, null);
        }
    }

    internal class PropertyWriter : PropertyReaderWriter
    {
        public PropertyWriter(ConcurrentDictionary<string, Delegate> setterMap)
        {
            _setterMap = setterMap;
        }

        public void Set(object instance, string propertyName, object propertyValue)
        {
            if (!_setterMap.TryGetValue(propertyName, out Delegate callback))
            {
                return;
            }
            callback.GetMethodInfo().Invoke(instance, new[] { propertyValue } );
        }
    }

}