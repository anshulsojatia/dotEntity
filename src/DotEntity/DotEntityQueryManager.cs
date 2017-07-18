/**
 * Copyright(C) 2017  Apexol Technologies
 * 
 * This file (DotEntityQueryManager.cs) is part of dotEntity(https://github.com/RoastedBytes/dotentity).
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

 * You can release yourself from the requirements of the license by purchasing
 * a commercial license.Buying such a license is mandatory as soon as you
 * develop commercial software involving the dotEntity software without
 * disclosing the source code of your own applications.
 * To know more about our commercial license email us at support@roastedbytes.com or
 * visit http://dotentity.net/legal/commercial
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using DotEntity.Enumerations;
using DotEntity.Extensions;

namespace DotEntity
{
    internal class DotEntityQueryManager : IDotEntityQueryManager
    {
        private readonly IQueryGenerator _queryGenerator;
        internal DotEntityQueryManager()
        {
            _queryGenerator = DotEntityDb.Provider.QueryGenerator;
        }

        private readonly bool _withTransaction;
        private readonly IList<DotEntityDbCommand> _transactionCommands;
        internal DotEntityQueryManager(bool withTransaction) : this()
        {
            _withTransaction = withTransaction;
            _transactionCommands = new List<DotEntityDbCommand>();
        }

        public virtual TType DoScaler<TType>(string query, dynamic parameters, Func<TType, bool> resultAction = null)
        {
            query = _queryGenerator.Query(query, parameters, out IList<QueryInfo> queryParameters);
            var cmd = new DotEntityDbCommand(DbOperationType.SelectScaler, query, queryParameters);
            cmd.ProcessResult(o =>
            {
                if (_withTransaction)
                    cmd.ContinueNextCommand = resultAction?.Invoke((TType)o) ?? true;
            });

            if (_withTransaction)
            {
                _transactionCommands.Add(cmd);
                return default(TType);
            }

            DotEntityDbConnector.ExecuteCommand(cmd);
            return cmd.GetResultAs<TType>();
        }

        public virtual IEnumerable<T> Do<T>(string query, dynamic parameters, Func<IEnumerable<T>, bool> resultAction = null) where T : class
        {
            query = _queryGenerator.Query(query, parameters, out IList<QueryInfo> queryParameters);
            var cmd = new DotEntityDbCommand(DbOperationType.Select, query, queryParameters);
            cmd.ProcessReader(reader =>
            {
                if (!_withTransaction)
                    return DataDeserializer<T>.Instance.DeserializeMany(reader);
                if (resultAction != null)
                {
                    var ts = DataDeserializer<T>.Instance.DeserializeMany(reader);
                    cmd.ContinueNextCommand = resultAction.Invoke(ts);
                }
                return null;
            });
            if (_withTransaction)
            {
                _transactionCommands.Add(cmd);
                return default(IEnumerable<T>);
            }
            DotEntityDbConnector.ExecuteCommand(cmd);
            return cmd.GetResultAs<IEnumerable<T>>();
        }

        public virtual void Do(string query, dynamic parameters)
        {
            query = _queryGenerator.Query(query, parameters, out IList<QueryInfo> queryParameters);
            var cmd = new DotEntityDbCommand(DbOperationType.Select, query, queryParameters);
            if (_withTransaction)
            {
                _transactionCommands.Add(cmd);
                return;
            }
            DotEntityDbConnector.ExecuteCommand(cmd);
        }

        public virtual IMultiResult DoMultiResult(string query, dynamic parameters)
        {
            query = _queryGenerator.Query(query, parameters, out IList<QueryInfo> queryParameters);
            var cmd = new DotEntityDbCommand(DbOperationType.MultiQuery, query, queryParameters);
            cmd.ProcessReader(reader =>
            {
                var multiResultRows = reader.GetRawDataReaderRows();
                return new MultiResult(multiResultRows);
            });
            DotEntityDbConnector.ExecuteCommand(cmd);
            return cmd.GetResultAs<IMultiResult>();
        }

        public virtual int DoInsert<T>(T entity, Func<T, bool> resultAction = null) where T : class
        {
            var keyColumn = DataDeserializer<T>.Instance.GetKeyColumn();
            var query = _queryGenerator.GenerateInsert(entity, out IList<QueryInfo> queryParameters);
            var cmd = new DotEntityDbCommand(DbOperationType.Insert, query, queryParameters, keyColumn);
            cmd.ProcessResult(result =>
            {
                var id = (int)result;
                DataDeserializer<T>.Instance.SetPropertyAs<int>(entity, keyColumn, id);
                if (_withTransaction)
                    //has somebody called for a rollback?
                    cmd.ContinueNextCommand = !resultAction?.Invoke(entity) ?? true;
            });
            if (_withTransaction)
            {
                _transactionCommands.Add(cmd);
                return default(int);
            }
            DotEntityDbConnector.ExecuteCommand(cmd);
            return 1;
        }

        public virtual int DoInsert<T>(T[] entities, Func<T, bool> resultAction = null) where T : class
        {
            var keyColumn = DataDeserializer<T>.Instance.GetKeyColumn();
            var commands = new List<DotEntityDbCommand>();
            foreach (var t in entities)
            {
                var query = _queryGenerator.GenerateInsert(t, out IList<QueryInfo> queryParameters);
                var cmd = new DotEntityDbCommand(DbOperationType.Insert, query, queryParameters, keyColumn);
                cmd.ProcessResult(result =>
                {
                    var id = (int)result;
                    DataDeserializer<T>.Instance.SetPropertyAs<int>(t, keyColumn, id);
                    if (_withTransaction)
                        //has somebody called for a rollback?
                        cmd.ContinueNextCommand = !resultAction?.Invoke(t) ?? true;
                });
                commands.Add(cmd);
                if (_withTransaction)
                {
                    _transactionCommands.Add(cmd);
                }
            }

            if (_withTransaction)
                return default(int);

            DotEntityDbConnector.ExecuteCommands(commands.ToArray());
            return 1;
        }

        [Obsolete("Doesn't give good performance", true)]
        public virtual int DoInsert2<T>(T[] entities, Func<T, bool> resultAction = null) where T : class
        {
            /*In batch inserts, the parameters may exceed the maximum number of command parameters supported by the server.
             e.g. SqlServer supports 2100 command parameters at max. We'll therefore check if the parameter counts don't exceed that limit*/
            var usablePropertiesCount = typeof(T).GetDatabaseUsableProperties().Count();
            var parameterCount = usablePropertiesCount * entities.Length;
            if (parameterCount > DotEntityDb.Provider.MaximumParametersPerQuery)
                throw new Exception(
                    "The batch insert can't continue because the resultant query will have more parameters that allowed by the provider");

            //safe to proceed
            var keyColumn = DataDeserializer<T>.Instance.GetKeyColumn();
            var query = _queryGenerator.GenerateBatchInsert(entities, out IList<QueryInfo> queryParameters);
            var cmd = new DotEntityDbCommand(DbOperationType.MultiQuery, query, queryParameters, keyColumn);

            cmd.ProcessReader(reader =>
            {
                var entityIndex = 0;
                var multiResultRows = reader.GetRawDataReaderRows();
                foreach (var rowList in multiResultRows)
                {
                    var id = (int)rowList[0][0];
                    DataDeserializer<T>.Instance.SetPropertyAs<int>(entities[entityIndex++], keyColumn, id);
                }
                return multiResultRows.Count;
            });
            if (_withTransaction)
            {
                _transactionCommands.Add(cmd);
            }

            if (_withTransaction)
                return default(int);

            DotEntityDbConnector.ExecuteCommand(cmd, useTransaction: true);
            return cmd.GetResultAs<int>();
        }

        public virtual int DoUpdate<T>(T entity, Func<T, bool> resultAction = null) where T : class
        {
            var query = _queryGenerator.GenerateUpdate(entity, out IList<QueryInfo> queryParameters);
            var cmd = new DotEntityDbCommand(DbOperationType.Update, query, queryParameters);
            if (_withTransaction)
            {
                cmd.ProcessResult(o => cmd.ContinueNextCommand = resultAction?.Invoke(entity) ?? true);
                _transactionCommands.Add(cmd);
                return default(int);
            }
            DotEntityDbConnector.ExecuteCommand(cmd);
            return cmd.GetResultAs<int>();
        }

        public virtual int DoUpdate<T>(dynamic entity, Expression<Func<T, bool>> where, Func<T, bool> resultAction = null) where T : class
        {
            var query = _queryGenerator.GenerateUpdate(entity, where, out IList<QueryInfo> queryParameters);
            var cmd = new DotEntityDbCommand(DbOperationType.Update, query, queryParameters);
            if (_withTransaction)
            {
                cmd.ProcessResult(o => cmd.ContinueNextCommand = resultAction?.Invoke(entity) ?? true);
                _transactionCommands.Add(cmd);
                return default(int);
            }
            DotEntityDbConnector.ExecuteCommand(cmd);
            return cmd.GetResultAs<int>();
        }

        public virtual int DoDelete<T>(T entity, Func<T, bool> resultAction = null) where T : class
        {
            var query = _queryGenerator.GenerateDelete(entity, out IList<QueryInfo> queryParameters);
            var cmd = new DotEntityDbCommand(DbOperationType.Delete, query, queryParameters);
            if (_withTransaction)
            {
                cmd.ProcessResult(o => cmd.ContinueNextCommand = resultAction?.Invoke(entity) ?? true);
                _transactionCommands.Add(cmd);
                return default(int);
            }
            DotEntityDbConnector.ExecuteCommand(cmd);
            return cmd.GetResultAs<int>();
        }

        public virtual int DoDelete<T>(Expression<Func<T, bool>> where, Func<T, bool> resultAction = null) where T : class
        {
            var query = _queryGenerator.GenerateDelete<T>(where, out IList<QueryInfo> queryParameters);
            var cmd = new DotEntityDbCommand(DbOperationType.Delete, query, queryParameters);
            if (_withTransaction)
            {
                cmd.ProcessResult(o => cmd.ContinueNextCommand = resultAction?.Invoke(null) ?? true);
                _transactionCommands.Add(cmd);
                return default(int);
            }
            DotEntityDbConnector.ExecuteCommand(cmd);
            return cmd.GetResultAs<int>();
        }

        public virtual IEnumerable<T> DoSelect<T>(List<Expression<Func<T, bool>>> where = null, Dictionary<Expression<Func<T, object>>, RowOrder> orderBy = null, int page = 1, int count = int.MaxValue) where T : class
        {
            ThrowIfInvalidPage(orderBy, page, count);

            var query = _queryGenerator.GenerateSelect(out IList<QueryInfo> queryParameters, where, orderBy, page, count);
            var cmd = new DotEntityDbCommand(DbOperationType.Select, query, queryParameters);
            cmd.ProcessReader(reader =>
            {
                return DataDeserializer<T>.Instance.DeserializeMany(reader);
            });
            DotEntityDbConnector.ExecuteCommand(cmd);

            return cmd.GetResultAs<IEnumerable<T>>();
        }

        public virtual Tuple<int, IEnumerable<T>> DoSelectWithTotalMatches<T>(List<Expression<Func<T, bool>>> where = null, Dictionary<Expression<Func<T, object>>, RowOrder> orderBy = null, int page = 1, int count = int.MaxValue) where T : class
        {
            ThrowIfInvalidPage(orderBy, page, count);

            var query = _queryGenerator.GenerateSelectWithTotalMatchingCount(out IList<QueryInfo> queryParameters, where, orderBy, page, count);

            var cmd = new DotEntityDbCommand(DbOperationType.Select, query, queryParameters);
            cmd.ProcessReader(reader =>
            {
                var ts = DataDeserializer<T>.Instance.DeserializeMany(reader);
                reader.NextResult();
                reader.Read();
                var total = (int)reader[0];
                return Tuple.Create(total, ts);
            });
            DotEntityDbConnector.ExecuteCommand(cmd);

            return cmd.GetResultAs<Tuple<int, IEnumerable<T>>>();
        }

        public virtual IEnumerable<T> DoSelect<T>(List<IJoinMeta> joinMetas, Dictionary<Type, Delegate> relationActions, List<LambdaExpression> where = null, Dictionary<LambdaExpression, RowOrder> orderBy = null, int page = 1, int count = int.MaxValue) where T : class
        {
            ThrowIfInvalidPage(orderBy, page, count);

            var query = _queryGenerator.GenerateJoin<T>(out IList<QueryInfo> queryParameters, joinMetas, where, orderBy, page, count);
            var cmd = new DotEntityDbCommand(DbOperationType.Select, query, queryParameters);
            cmd.ProcessReader(reader => DataDeserializer<T>.Instance.DeserializeManyNested(reader, joinMetas, relationActions));
            DotEntityDbConnector.ExecuteCommand(cmd);
            return cmd.GetResultAs<IEnumerable<T>>();
        }

        public virtual T DoSelectSingle<T>(List<Expression<Func<T, bool>>> where = null, Dictionary<Expression<Func<T, object>>, RowOrder> orderBy = null) where T : class
        {
            var query = _queryGenerator.GenerateSelect(out IList<QueryInfo> queryParameters, where, orderBy);
            var cmd = new DotEntityDbCommand(DbOperationType.Select, query, queryParameters);
            cmd.ProcessReader(reader => DataDeserializer<T>.Instance.DeserializeSingle(reader));
            DotEntityDbConnector.ExecuteCommand(cmd);
            return cmd.GetResultAs<T>();
        }

        public virtual int DoCount<T>(List<Expression<Func<T, bool>>> where, Func<int, bool> resultAction = null) where T : class
        {
            var query = _queryGenerator.GenerateCount(where, out IList<QueryInfo> queryParameters);
            var cmd = new DotEntityDbCommand(DbOperationType.SelectScaler, query, queryParameters);
            if (_withTransaction)
            {
                cmd.ProcessResult(o => cmd.ContinueNextCommand = resultAction?.Invoke((int)o) ?? true);
                _transactionCommands.Add(cmd);
                return default(int);
            }
            DotEntityDbConnector.ExecuteCommand(cmd);
            return cmd.GetResultAs<int>();
        }

        public virtual bool CommitTransaction()
        {
            Throw.IfCommitCalledOnNonTransactionalExecution(_withTransaction);

            DotEntityDbConnector.ExecuteCommands(_transactionCommands.ToArray(), true);
            return _transactionCommands.All(x => x.ContinueNextCommand);
        }

        private static void ThrowIfInvalidPage(ICollection orderBy, int page, int count)
        {
            Throw.IfInvalidPagination(orderBy, page, count);
        }

        private bool _disposed;
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