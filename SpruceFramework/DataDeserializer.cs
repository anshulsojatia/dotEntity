// #region Author Information
// // DataDeserializer.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using System;
using System.Collections.Concurrent;
using System.Data;
using System.Globalization;
using System.Linq;
using SpruceFramework.Extensions;
using SpruceFramework.Reflection;

namespace SpruceFramework
{
    internal class DataDeserializer<T> : IDataDeserializer<T> where T : class
    {
        public ConcurrentDictionary<string, object> TypeMap = null;

        private readonly Type _typeofT;

        public DataDeserializer()
        {
            _typeofT = typeof(T);
            CreateMapIfNotDone();
        }

        private void CreateMapIfNotDone()
        {
            if (TypeMap != null && TypeMap.Count > 0)
                return;

            TypeMap = new ConcurrentDictionary<string, object>();

            var typeProperties = _typeofT.GetProperties();
            foreach (var property in typeProperties)
            {

                var propertyType = property.PropertyType;
                object setter = null;
                if (propertyType == typeof(int))
                    setter = property.CreateSetter<T, int>();
                else if (propertyType == typeof(string))
                    setter = property.CreateSetter<T, string>();
                else if (propertyType == typeof(DateTime))
                    setter = property.CreateSetter<T, DateTime>();
                else if (propertyType == typeof(decimal))
                    setter = property.CreateSetter<T, decimal>();
                else if (propertyType == typeof(bool))
                    setter = property.CreateSetter<T, bool>();

                TypeMap.TryAdd(property.Name, setter);
            }
        }
      

        
        public T DeserializeSingle(IDataReader reader)
        {
            return DeserializeMany(reader).FirstOrDefault();
        }

        public T[] DeserializeMany(IDataReader reader)
        {
            var tArray = FurnishInstances(reader);
            return tArray;
        }

        private T[] FurnishInstances(IDataReader reader)
        {
            var columnNames = TypeMap.Keys.ToArray();
            var rows = reader.GetDataReaderRows(columnNames);
            var tInstances = Instantiator<T>.Instances(rows.Length);
            var index = 0;

            foreach (var row in rows)
            {
                var instance = tInstances[index++];
                foreach (var column in columnNames)
                {
                    var fieldName = column;
                    if (!TypeMap.ContainsKey(fieldName)) continue;
                    var fieldValue = row[fieldName];
                    var fieldType = fieldValue.GetType();

                    if (fieldType == typeof(int))
                        SetPropertyAs<int>(instance, fieldName, fieldValue);
                    else if (fieldType == typeof(string))
                        SetPropertyAs<string>(instance, fieldName, fieldValue);
                    else if (fieldType == typeof(DateTime))
                        SetPropertyAs<DateTime>(instance, fieldName, fieldValue);
                    else if (fieldType == typeof(decimal))
                        SetPropertyAs<decimal>(instance, fieldName, fieldValue);
                    else if (fieldType == typeof(bool))
                        SetPropertyAs<bool>(instance, fieldName, fieldValue);
                }
            }

            return tInstances;
        }

        private void SetPropertyAs<TType>(T instance, string fieldName, object value)
        {
            ((Action<T, TType>)TypeMap[fieldName]).Invoke(instance, Parse<TType>(value));
        }
        private static TType Parse<TType>(object value)
        {
            if (value == null || value is DBNull) return default(TType);
            if (value is TType) return (TType)value;
            var type = typeof(TType);
            type = Nullable.GetUnderlyingType(type) ?? type;
            if (type.IsEnum)
            {
                if (value is float || value is double || value is decimal)
                {
                    value = Convert.ChangeType(value, Enum.GetUnderlyingType(type), CultureInfo.InvariantCulture);
                }
                return (TType)Enum.ToObject(type, value);
            }

            return (TType)Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
        }


        public static DataDeserializer<T> Instance => Singleton<DataDeserializer<T>>.Instance;
    }
}