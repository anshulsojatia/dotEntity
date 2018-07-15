/**
 * Copyright(C) 2017  Apexol Technologies
 * 
 * This file (DataDeserializer.cs) is part of dotEntity(https://github.com/RoastedBytes/dotentity).
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
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using DotEntity.Caching;
using DotEntity.Extensions;
using DotEntity.Reflection;

namespace DotEntity
{
    internal class DataDeserializer<T> : IDataDeserializer<T> where T : class
    {
        private PropertyWriter _setter = null;
        private PropertyReader _getter = null;
        private readonly Type _typeofT;

        public DataDeserializer()
        {
            _typeofT = typeof(T);
            CreateMapIfNotDone();
        }

        private void CreateMapIfNotDone()
        {
            _setter = PropertyCallerCache.SetterOfType(_typeofT);
        }
      

        
        public T DeserializeSingle(IDataReader reader, DotEntityDbCommand command)
        {
            return DeserializeMany(reader, command).FirstOrDefault();
        }

        public T DeserializeSingle(List<DataReaderRow> rows)
        {
            return DeserializeMany(rows).FirstOrDefault();
        }

        public IEnumerable<T> DeserializeMany(IDataReader reader, DotEntityDbCommand command)
        {
            var columnNames = GetColumns();
            var rows = reader.GetDataReaderRows(columnNames, _typeofT.Name, command);
            return DeserializeMany(rows);
        }

        public IEnumerable<T> DeserializeMany(List<DataReaderRow> rows)
        {
            var tArray = FurnishInstances(rows);
            return tArray;
        }

        public void SetProperties(T instance, DataReaderRow row, string[] columnNames)
        {
            for(var i = 0; i < columnNames.Length; i++)
            {
                var fieldName = columnNames[i];
                //if (!_setterMap.ContainsKey(fieldName)) continue;
                var fieldValue = row[_typeofT.Name + "." + fieldName];
                _setter.Set(instance, fieldName, fieldValue);
            }
        }
       
        private IEnumerable<T> FurnishInstances(List<DataReaderRow> rows)
        {
            var columnNames = GetColumns();
            var tInstances = Instantiator<T>.Instances(rows.Count);
            var index = 0;

            foreach (var row in rows)
            {
                var instance = tInstances[index++];
                SetProperties(instance, row, columnNames);
                yield return instance;
            }
        }

        public IEnumerable<T> DeserializeManyNested(IDataReader reader, IList<IJoinMeta> joinMetas, Dictionary<Type, Delegate> relationActions)
        {
            
            var tInstances = new List<T>();
            //make deserializers for each of relation types
            var deserializers = new Dictionary<Type, IDataDeserializer>();
            var columnsToSkip = new Dictionary<Type, int> {{_typeofT, -1}};
            var localObjectCache = new Dictionary<string, object>();

            foreach (var jm in joinMetas)
            {
                var serializerObject = (IDataDeserializer) GenericInvoker.InvokeProperty(null, typeof(DataDeserializer<>), jm.OnType, "Instance");
                deserializers.Add(jm.OnType, serializerObject);
                columnsToSkip.Add(jm.OnType, -1);
            }

            var rowIndex = 0;
            DataReaderRow prevRow = null;
            var rows = reader.GetDataReaderRows(columnsToSkip); //all rows
            var lastProcessedObjects = new Dictionary<Type, object>();

            void AddOrUpdateLastProcessObject(Type type, object obj)
            {
                if (lastProcessedObjects.ContainsKey(type))
                    lastProcessedObjects[type] = obj;
                else
                {
                    lastProcessedObjects.Add(type, obj);
                }
            }

            while (rowIndex < rows.Count)
            {
                var row = rows[rowIndex];
                var tInstance = GetAppropriateInstance(_typeofT, prevRow, row, this, ref localObjectCache);
                if (tInstance == null)
                    continue;
                if(!tInstances.Contains(tInstance))
                    tInstances.Add((T) tInstance);

                //then for all the child instances
                foreach (var ds in deserializers)
                {
                    if (!relationActions.ContainsKey(ds.Key))
                        continue;

                    var childInstance = GetAppropriateInstance(ds.Key, prevRow, row, ds.Value, ref localObjectCache);
                    if (childInstance == null)
                        continue;

                    //invoke the relation to bind the instances if required
                    relationActions[ds.Key].DynamicInvoke(tInstance, childInstance);
                }
                prevRow = row;
                rowIndex++;
            }

            return tInstances.AsEnumerable();
        }

        private object GetAppropriateInstance(Type instanceType, DataReaderRow prevDataRow,
            DataReaderRow currentDataRow, IDataDeserializer deserializer, ref Dictionary<string, object> localCache)
        {
            const string localObjectKey = "{0}.{1}.{2}"; //<Type>.<Key>.<NUM>

            var columns = deserializer.GetColumns();
            var typedColumns = deserializer.GetTypedColumnNames(columns, instanceType);

            //are all columns of current row null
            if (DataReaderRow.AreAllColumnsNull(currentDataRow, typedColumns, 0))
                return null;

            //let's check if have this object in cache
            var keyColumn = deserializer.GetKeyColumn();
            var cacheKey = string.Format(localObjectKey, instanceType.Name, keyColumn,
                currentDataRow[instanceType.Name + "." + keyColumn]);

            if (!localCache.TryGetValue(cacheKey, out object newInstance))
            {
                //we can create instance
                newInstance = Instantiator.GetInstance(instanceType);

                //assign properties
                GenericInvoker.Invoke(deserializer, "SetProperties", newInstance, currentDataRow, columns);
                localCache.Add(cacheKey, newInstance);
            }
            return newInstance;
        }

        private bool CreateInstanceIfRequired(Type instanceType, DataReaderRow prevDataRow, DataReaderRow currentDataRow, IDataDeserializer deserializer, int skipColumns, out object newInstance, ref Dictionary<string, object> localCache)
        {
            const string localObjectKey = "{0}.{1}.{2}"; //<Type>.<Key>.<NUM>

            newInstance = null;
            var columns = deserializer.GetColumns();
            var typedColumns = deserializer.GetTypedColumnNames(columns, instanceType);
            if (DataReaderRow.AreSameRowsForColumns(prevDataRow, currentDataRow, typedColumns, skipColumns))
                return false;

            //are all columns of current row null
            if (DataReaderRow.AreAllColumnsNull(currentDataRow, typedColumns, skipColumns))
                return false;

            //let's check if have this object in cache
            var keyColumn = deserializer.GetKeyColumn();
            var cacheKey = string.Format(localObjectKey, instanceType.Name, keyColumn,
                currentDataRow[_typeofT.Name + "." + keyColumn]);

            if (localCache.TryGetValue(cacheKey, out newInstance))
                return true;

            //we can create instance
            newInstance = Instantiator.GetInstance(instanceType);
            
            //assign properties
            GenericInvoker.Invoke(deserializer, "SetProperties", newInstance, currentDataRow, columns);
            localCache.Add(cacheKey, newInstance);
            return true;
        }

        internal void SetPropertyAs<TType>(T instance, string fieldName, object value)
        {
            _setter.Set(instance, fieldName, value);
        }

        internal TType GetPropertyAs<TType>(T instance, string fieldName)
        {
            if (_getter == null)
                _getter = PropertyCallerCache.GetterOfType(_typeofT);

            return _getter.Get<TType>(instance, fieldName);
        }

        public static DataDeserializer<T> Instance => Singleton<DataDeserializer<T>>.Instance;

        private string[] _columnsAsArray;
        public string[] GetColumns()
        {
            return _columnsAsArray ?? (_columnsAsArray = _setter.Keys.ToArray());
        }

        private string _keyColumnName = null;
        public string GetKeyColumn()
        {
            if (_keyColumnName != null)
                return _keyColumnName;
            _keyColumnName = _typeofT.GetKeyColumnName();
            return _keyColumnName;
        }
    }
}