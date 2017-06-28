// #region Author Information
// // SpruceDbCommand.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using System;
using System.Collections.Generic;
using System.Data;
using SpruceFramework.Enumerations;

namespace SpruceFramework
{
    internal class SpruceDbCommand
    {
        public DbOperationType OperationType { get; set; }

        public string Query { get; set; }

        public IList<QueryParameter> QueryParameters { get; set; }

        public string KeyColumn { get; set; }

        public bool ContinueNextCommand { get; set; }

        public SpruceDbCommand(DbOperationType operationType, string query, IList<QueryParameter> queryParameters, string keyColumn = "Id")
        {
            OperationType = operationType;
            Query = query;
            QueryParameters = queryParameters;
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