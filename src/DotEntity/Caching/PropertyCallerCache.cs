/**
 * Copyright(C) 2017-2021  Sojatia Infocrafts Private Limited
 * 
 * This file (PropertyCallerCache.cs) is part of dotEntity(https://github.com/RoastedBytes/dotentity).
 * 
 * dotEntity is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 
 * dotEntity is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
 * GNU Affero General Public License for more details.
 
 * You should have received a copy of the GNU Affero General Public License
 * along with dotEntity.If not, see<http://www.gnu.org/licenses/>.

 * You can release yourself from the requirements of the AGPL license by purchasing
 * a commercial license (dotEntity Pro). Buying such a license is mandatory as soon as you
 * develop commercial activities involving the dotEntity software without
 * disclosing the source code of your own applications. The activites include:
 * shipping dotEntity with a closed source product, offering paid services to customers
 * as an Application Service Provider.
 * To know more about our commercial license email us at support@roastedbytes.com or
 * visit http://dotentity.net/licensing
 */
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
                    if (!property.CanWrite)
                        continue;
                    var propertyType = property.PropertyType;
                    Delegate setter = null;
                    if (propertyType == typeof(int))
                        setter = property.CreateSetter<int>(_type);
                    if (propertyType == typeof(int?))
                        setter = property.CreateSetter<int?>(_type);
                    else if (propertyType == typeof(string))
                        setter = property.CreateSetter<string>(_type);
                    else if (propertyType == typeof(DateTime))
                        setter = property.CreateSetter<DateTime>(_type);
                    else if (propertyType == typeof(DateTime?))
                        setter = property.CreateSetter<DateTime?>(_type);
                    else if (propertyType == typeof(decimal))
                        setter = property.CreateSetter<decimal>(_type);
                    else if (propertyType == typeof(decimal?))
                        setter = property.CreateSetter<decimal?>(_type);
                    else if (propertyType == typeof(float))
                        setter = property.CreateSetter<float>(_type);
                    else if (propertyType == typeof(float?))
                        setter = property.CreateSetter<float?>(_type);
                    else if (propertyType == typeof(double))
                        setter = property.CreateSetter<double>(_type);
                    else if (propertyType == typeof(double?))
                        setter = property.CreateSetter<double?>(_type);
                    else if (propertyType == typeof(bool))
                        setter = property.CreateSetter<bool>(_type);
                    else if (propertyType == typeof(bool?))
                        setter = property.CreateSetter<bool?>(_type);
                    else if (propertyType.GetTypeInfo().IsEnum)
                        setter = property.CreateSetter(_type, propertyType);
                    else if (propertyType == typeof(Guid))
                        setter = property.CreateSetter<Guid>(_type);
                    else if (propertyType == typeof(byte))
                        setter = property.CreateSetter<byte>(_type);
                    else if (propertyType == typeof(byte[]))
                        setter = property.CreateSetter<byte[]>(_type);
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
                    if (!property.CanRead)
                        continue;
                    var propertyType = property.PropertyType;
                    Delegate getter = null;
                    if (propertyType == typeof(int))
                        getter = property.CreateGetter<int>(_type);
                    if (propertyType == typeof(int?))
                        getter = property.CreateGetter<int?>(_type);
                    else if (propertyType == typeof(string))
                        getter = property.CreateGetter<string>(_type);
                    else if (propertyType == typeof(DateTime))
                        getter = property.CreateGetter<DateTime>(_type);
                    else if (propertyType == typeof(DateTime?))
                        getter = property.CreateGetter<DateTime?>(_type);
                    else if (propertyType == typeof(decimal))
                        getter = property.CreateGetter<decimal>(_type);
                    else if (propertyType == typeof(decimal?))
                        getter = property.CreateGetter<decimal?>(_type);
                    else if (propertyType == typeof(float))
                        getter = property.CreateGetter<float>(_type);
                    else if (propertyType == typeof(float?))
                        getter = property.CreateGetter<float?>(_type);
                    else if (propertyType == typeof(double))
                        getter = property.CreateGetter<double>(_type);
                    else if (propertyType == typeof(double?))
                        getter = property.CreateGetter<double?>(_type);
                    else if (propertyType == typeof(bool))
                        getter = property.CreateGetter<bool>(_type);
                    else if (propertyType == typeof(bool?))
                        getter = property.CreateGetter<bool?>(_type);
                    else if (propertyType.GetTypeInfo().IsEnum)
                        getter = property.CreateGetter(_type, propertyType);
                    else if (propertyType == typeof(Guid))
                        getter = property.CreateGetter<Guid>(_type);
                    else if (propertyType == typeof(byte))
                        getter = property.CreateGetter<byte>(_type);
                    else if (propertyType == typeof(byte[]))
                        getter = property.CreateGetter<byte[]>(_type);
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
            var minfo = callback.GetMethodInfo();
            if (propertyValue is DBNull)
                propertyValue = null;
            var paramterType = minfo.GetParameters().First().ParameterType;
            if (paramterType.GetTypeInfo().IsEnum && propertyValue != null)
            {
                propertyValue = Enum.Parse(paramterType, propertyValue.ToString());
            }
            try
            {
                minfo.Invoke(instance, new[] { propertyValue });
            }
            catch
            {
                
                var convertedValue = Convert.ChangeType(propertyValue, paramterType);
                minfo.Invoke(instance, new[] { convertedValue });
            }
        }
    }

}