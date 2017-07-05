// #region Author Information
// // MultiResult.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using System.Collections.Generic;
using System.Linq;
using SpruceFramework.Extensions;

namespace SpruceFramework
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
            try
            {
                return (TType) _resultSet[_resultIndex].First()[0];
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