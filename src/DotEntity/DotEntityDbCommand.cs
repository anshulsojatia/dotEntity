/**
 * Copyright(C) 2017  Apexol Technologies
 * 
 * This file (DotEntityDbCommand.cs) is part of dotEntity(https://github.com/RoastedBytes/dotentity).
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
 * a commercial license (dotEntity or dotEntity Pro). Buying such a license is mandatory as soon as you
 * develop commercial activities involving the dotEntity software without
 * disclosing the source code of your own applications. The activites include:
 * shipping dotEntity with a closed source product, offering paid services to customers
 * as an Application Service Provider.
 * To know more about our commercial license email us at support@roastedbytes.com or
 * visit http://dotentity.net/licensing
 */
using System;
using System.Collections.Generic;
using System.Data;
using DotEntity.Enumerations;

namespace DotEntity
{
    internal class DotEntityDbCommand
    {
        public DbOperationType OperationType { get; set; }

        public string Query { get; set; }

        public IList<QueryInfo> QueryInfos { get; set; }

        public string KeyColumn { get; set; }

        public DotEntityDbCommand(DbOperationType operationType, string query, IList<QueryInfo> queryParameters, string keyColumn = "Id")
        {
            OperationType = operationType;
            Query = query;
            QueryInfos = queryParameters;
            KeyColumn = keyColumn;
        }

        private Func<IDataReader, object> _readerAction;
        public void ProcessReader(Func<IDataReader, object> action)
        {
            _readerAction = action;
        }

        private Action<object> _processAction;
        public void ProcessResult(Action<object> action)
        {
            _processAction = action;
        }


        internal void SetDataReader(IDataReader reader)
        {
            RawResult = _readerAction?.Invoke(reader);
        }

        internal void SetRawResult(object value)
        {
            RawResult = value;
            _processAction?.Invoke(RawResult);
        }
        public object RawResult { get; private set; }

        public T GetResultAs<T>()
        {
            try
            {
                return (T)RawResult;
            }
            catch
            {
                return (T)Convert.ChangeType(RawResult, typeof(T));
            }
        }
    }
}