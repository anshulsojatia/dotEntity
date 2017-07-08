// #region Author Information
// // DotEntityDbCommand.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

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

        public bool ContinueNextCommand { get; set; }

        public DotEntityDbCommand(DbOperationType operationType, string query, IList<QueryInfo> queryParameters, string keyColumn = "Id")
        {
            OperationType = operationType;
            Query = query;
            QueryInfos = queryParameters;
            ContinueNextCommand = true;
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
            return (T) RawResult;
        }
    }
}