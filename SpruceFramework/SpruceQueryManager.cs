// #region Author Information
// // SpruceQueryManager.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using SpruceFramework.Enumerations;

namespace SpruceFramework
{
    internal class SpruceQueryManager : ISpruceQueryManager
    {
        private readonly IQueryGenerator _queryGenerator;
        internal SpruceQueryManager()
        {
            _queryGenerator = Spruce.QueryGenerator;
        }

        private readonly bool _withTransaction;
        private readonly IList<SpruceDbCommand> _transactionCommands;
        internal SpruceQueryManager(bool withTransaction) : this()
        {
            _withTransaction = withTransaction;
            _transactionCommands = new List<SpruceDbCommand>();
        }

        public virtual TType DoScaler<TType>(string query, dynamic parameters, Func<TType, bool> resultAction = null)
        {
            query = _queryGenerator.Query(query, parameters, out IList<QueryInfo> queryParameters);
            var cmd = new SpruceDbCommand(DbOperationType.SelectSingle, query, queryParameters);
            cmd.ProcessResult(o =>
            {
                if (_withTransaction)
                   cmd.ContinueNextCommand = resultAction?.Invoke((TType) o) ?? true;
            });

            if (_withTransaction)
            {
                _transactionCommands.Add(cmd);
                return default(TType);
            }

            SpruceDbConnector.ExecuteCommand(cmd);
            return cmd.GetResultAs<TType>();
        }

        public virtual IEnumerable<T> Do<T>(string query, dynamic parameters, Func<IEnumerable<T>, bool> resultAction = null) where T : class
        {
            query = _queryGenerator.Query(query, parameters, out IList<QueryInfo> queryParameters);
            var cmd = new SpruceDbCommand(DbOperationType.Select, query, queryParameters);
            cmd.ProcessReader(reader =>
            {
                if(!_withTransaction)
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
            SpruceDbConnector.ExecuteCommand(cmd);
            return cmd.GetResultAs<IEnumerable<T>>();
        }

        public virtual void Do(string query, dynamic parameters)
        {
            query = _queryGenerator.Query(query, parameters, out IList<QueryInfo> queryParameters);
            var cmd = new SpruceDbCommand(DbOperationType.Select, query, queryParameters);
            if (_withTransaction)
            {
                _transactionCommands.Add(cmd);
                return;
            }
            SpruceDbConnector.ExecuteCommand(cmd);
        }

        public virtual int DoInsert<T>(T entity, Func<T, bool> resultAction = null) where T : class
        {
            var keyColumn = DataDeserializer<T>.Instance.GetKeyColumn();
            var query = _queryGenerator.GenerateInsert(entity, out IList<QueryInfo> queryParameters);
            var cmd = new SpruceDbCommand(DbOperationType.Insert, query, queryParameters, keyColumn);
            cmd.ProcessResult(result =>
            {
                var id = (int) result;
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
            SpruceDbConnector.ExecuteCommand(cmd);
            return 1;
        }

        public virtual int DoInsert<T>(T[] entities, Func<T, bool> resultAction = null) where T : class
        {
            var keyColumn = DataDeserializer<T>.Instance.GetKeyColumn();
            var commands = new List<SpruceDbCommand>();
            foreach (var t in entities)
            {
                var query = _queryGenerator.GenerateInsert(t, out IList<QueryInfo> queryParameters);
                var cmd = new SpruceDbCommand(DbOperationType.Insert, query, queryParameters, keyColumn);
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

            SpruceDbConnector.ExecuteCommands(commands.ToArray());
            return 1;
        }

        public virtual int DoUpdate<T>(T entity, Func<T, bool> resultAction = null) where T : class
        {
            var query = _queryGenerator.GenerateUpdate(entity, out IList<QueryInfo> queryParameters);
            var cmd = new SpruceDbCommand(DbOperationType.Update, query, queryParameters);
            if (_withTransaction)
            {
                cmd.ProcessResult(o => cmd.ContinueNextCommand = resultAction?.Invoke(entity) ?? true);
                _transactionCommands.Add(cmd);
                return default(int);
            }
            SpruceDbConnector.ExecuteCommand(cmd);
            return cmd.GetResultAs<int>();
        }

        public virtual int DoUpdate<T>(dynamic entity, Expression<Func<T, bool>> where, Func<T, bool> resultAction = null) where T : class
        {
            var query = _queryGenerator.GenerateUpdate(entity, where, out IList<QueryInfo> queryParameters);
            var cmd = new SpruceDbCommand(DbOperationType.Update, query, queryParameters);
            if (_withTransaction)
            {
                cmd.ProcessResult(o => cmd.ContinueNextCommand = resultAction?.Invoke(entity) ?? true);
                _transactionCommands.Add(cmd);
                return default(int);
            }
            SpruceDbConnector.ExecuteCommand(cmd);
            return cmd.GetResultAs<int>();
        }

        public virtual int DoDelete<T>(T entity, Func<T, bool> resultAction = null) where T : class
        {
            var query = _queryGenerator.GenerateDelete<T>(x => x == entity, out IList<QueryInfo> queryParameters);
            var cmd = new SpruceDbCommand(DbOperationType.Delete, query, queryParameters);
            if (_withTransaction)
            {
                cmd.ProcessResult(o => cmd.ContinueNextCommand = resultAction?.Invoke(entity) ?? true);
                _transactionCommands.Add(cmd);
                return default(int);
            }
            SpruceDbConnector.ExecuteCommand(cmd);
            return cmd.GetResultAs<int>();
        }

        public virtual int DoDelete<T>(Expression<Func<T, bool>> where, Func<T, bool> resultAction = null) where T : class
        {
            var query = _queryGenerator.GenerateDelete<T>(where, out IList<QueryInfo> queryParameters);
            var cmd = new SpruceDbCommand(DbOperationType.Delete, query, queryParameters);
            if (_withTransaction)
            {
                cmd.ProcessResult(o => cmd.ContinueNextCommand = resultAction?.Invoke(null) ?? true);
                _transactionCommands.Add(cmd);
                return default(int);
            }
            SpruceDbConnector.ExecuteCommand(cmd);
            return cmd.GetResultAs<int>();
        }

        public virtual IEnumerable<T> DoSelect<T>(List<Expression<Func<T, bool>>> where = null, Dictionary<Expression<Func<T, object>>, RowOrder> orderBy = null, int page = 1, int count = int.MaxValue) where T : class
        {
            ThrowIfInvalidPage(orderBy, page, count);

            var query = _queryGenerator.GenerateSelect(out IList<QueryInfo> queryParameters, where, orderBy, page, count);
            var cmd = new SpruceDbCommand(DbOperationType.Select, query, queryParameters);
            cmd.ProcessReader(reader =>
            {
                return DataDeserializer<T>.Instance.DeserializeMany(reader);
            });
            SpruceDbConnector.ExecuteCommand(cmd);

            return cmd.GetResultAs<IEnumerable<T>>();
        }

        public virtual Tuple<int, IEnumerable<T>> DoSelectWithTotalMatches<T>(List<Expression<Func<T, bool>>> where = null, Dictionary<Expression<Func<T, object>>, RowOrder> orderBy = null, int page = 1, int count = int.MaxValue) where T : class
        {
            ThrowIfInvalidPage(orderBy, page, count);

            var query = _queryGenerator.GenerateSelectWithTotalMatchingCount(out IList<QueryInfo> queryParameters, where, orderBy, page, count);

            var cmd = new SpruceDbCommand(DbOperationType.Select, query, queryParameters);
            cmd.ProcessReader(reader =>
            {
                var ts = DataDeserializer<T>.Instance.DeserializeMany(reader);
                reader.NextResult();
                reader.Read();
                var total = (int) reader[0];
                return Tuple.Create(total, ts);
            });
            SpruceDbConnector.ExecuteCommand(cmd);

            return cmd.GetResultAs<Tuple<int, IEnumerable<T>>>();
        }

        public virtual IEnumerable<T> DoSelect<T>(List<IJoinMeta> joinMetas, Dictionary<Type, Delegate> relationActions, List<LambdaExpression> where = null, Dictionary<LambdaExpression, RowOrder> orderBy = null, int page = 1, int count = int.MaxValue) where T : class
        {
            ThrowIfInvalidPage(orderBy, page, count);

            var query = _queryGenerator.GenerateJoin<T>(out IList<QueryInfo> queryParameters, joinMetas, where, orderBy, page, count);
            var cmd = new SpruceDbCommand(DbOperationType.Select, query, queryParameters);
            cmd.ProcessReader(reader => DataDeserializer<T>.Instance.DeserializeManyNested(reader, joinMetas, relationActions));
            SpruceDbConnector.ExecuteCommand(cmd);
            return cmd.GetResultAs<IEnumerable<T>>();
        }

        public virtual T DoSelectSingle<T>(List<Expression<Func<T, bool>>> where = null, Dictionary<Expression<Func<T, object>>, RowOrder> orderBy = null) where T : class
        {
            var query = _queryGenerator.GenerateSelect(out IList<QueryInfo> queryParameters, where, orderBy);
            var cmd = new SpruceDbCommand(DbOperationType.Select, query, queryParameters);
            cmd.ProcessReader(reader => DataDeserializer<T>.Instance.DeserializeSingle(reader));
            SpruceDbConnector.ExecuteCommand(cmd);
            return cmd.GetResultAs<T>();
        }

        public virtual int DoCount<T>(List<Expression<Func<T, bool>>> where, Func<int, bool> resultAction = null) where T : class
        {
            var query = _queryGenerator.GenerateCount(where, out IList<QueryInfo> queryParameters);
            var cmd = new SpruceDbCommand(DbOperationType.SelectSingle, query, queryParameters);
            if (_withTransaction)
            {
                cmd.ProcessResult(o => cmd.ContinueNextCommand = resultAction?.Invoke((int) o) ?? true);
                _transactionCommands.Add(cmd);
                return default(int);
            }
            SpruceDbConnector.ExecuteCommand(cmd);
            return cmd.GetResultAs<int>();
        }

        public virtual bool CommitTransaction()
        {
            if(!_withTransaction)
                throw new Exception("Can not call Commit on a non-transactional execution");

            SpruceDbConnector.ExecuteCommands(_transactionCommands.ToArray(), true);
            return _transactionCommands.All(x => x.ContinueNextCommand);
        }

        private static void ThrowIfInvalidPage(ICollection orderBy, int page, int count)
        {
            if (page < 1 || count < 0 || ((page > 1 || count < int.MaxValue) && (orderBy == null || orderBy.Count == 0)))
            {
                throw new Exception("Invalid pagination");
            }
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