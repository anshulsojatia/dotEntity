/**
 * Copyright(C) 2017  Apexol Technologies
 * 
 * This file (MultiResult.cs) is part of dotEntity(https://github.com/RoastedBytes/dotentity).
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
 * a commercial license. Buying such a license is mandatory as soon as you
 * develop commercial activities involving the dotEntity software without
 * disclosing the source code of your own applications. The activites include:
 * shipping dotEntity with a closed source product, offering paid services to customers
 * as an Application Service Provider.
 * To know more about our commercial license email us at support@roastedbytes.com or
 * visit http://dotentity.net/legal/commercial
 */
using System;
using System.Collections.Generic;
using System.Linq;
using DotEntity.Extensions;

namespace DotEntity
{
    internal class MultiResult : IMultiResult
    {
        private readonly List<List<DataReaderRow>> _resultSet;
        private int _resultIndex = 0;

        public MultiResult(List<List<DataReaderRow>> resultSet)
        {
            _resultSet = resultSet;
        }

        private void NextResult()
        {
            _resultIndex++;
        }

        public T SelectAs<T>() where T : class
        {
            try
            {
                _resultSet[_resultIndex].PrefixTypeName(typeof(T).Name);
                return DataDeserializer<T>.Instance.DeserializeSingle(_resultSet[_resultIndex]);
            }
            finally
            {
                NextResult();
            }
        }

        public IEnumerable<T> SelectAllAs<T>() where T : class
        {
            try
            {
                _resultSet[_resultIndex].PrefixTypeName(typeof(T).Name);
                return DataDeserializer<T>.Instance.DeserializeMany(_resultSet[_resultIndex]);
            }
            finally
            {
                NextResult();
            }
        }

        public TType SelectScalerAs<TType>()
        {
            var value = _resultSet[_resultIndex].First()[0];
            try
            {
                return (TType) value;
            }
            catch
            {
                return (TType) Convert.ChangeType(value, typeof(TType));
            }
            finally
            {
                NextResult();
            }
        }

        private bool _disposed = false;
        public void Dispose()
        {
            _disposed = true;
        }

        public bool IsDisposed()
        {
            return _disposed;
        }
    }
}