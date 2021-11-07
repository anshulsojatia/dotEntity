/**
 * Copyright(C) 2017-2021  Sojatia Infocrafts Private Limited
 * 
 * This file (EntitySet`.cs) is part of dotEntity(https://github.com/RoastedBytes/dotentity).
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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DotEntity.Enumerations;
using DotEntity.Extensions;

namespace DotEntity
{
    /// <summary>
    /// Provides a standard way to perform various database operations for the entity of type <typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T">The entity class that maps to a specific database table</typeparam>
    public sealed partial class EntitySet<T> : IEntitySet<T> where T : class
    {
        private readonly List<Expression<Func<T, bool>>> _whereList;
        private readonly Dictionary<Expression<Func<T, object>>, RowOrder> _orderBy;

        private List<IJoinMeta> _joinList;
        private List<LambdaExpression> _joinWhereList;
        private Dictionary<LambdaExpression, RowOrder> _joinOrderBy;
        private Dictionary<Type, IList<string>> _skip;

        internal EntitySet()
        {
            _whereList = new List<Expression<Func<T, bool>>>();
            _orderBy = new Dictionary<Expression<Func<T, object>>, RowOrder>();
        }

        #region static wrappers
        /// <summary>
        /// Inserts a new entity of specified type into database table
        /// </summary>
        /// <param name="entity">The entity to be inserted</param>
        public static void Insert(T entity)
        {
            using (var manager = new DotEntityQueryManager())
            {
                manager.DoInsert(entity);
            }
        }
        /// <summary>
        /// Inserts multiple entities of specified type into database table
        /// </summary>
        /// <param name="entities">An array of entities to be inserted</param>
        public static void Insert(T[] entities)
        {
            using (var manager = new DotEntityQueryManager())
            {
                manager.DoInsert(entities);
            }
        }
        /// <summary>
        /// Inserts a new entity of type <typeparam name="T"></typeparam> into database within a transaction
        /// </summary>
        /// <param name="entity">The entity to be inserted. The primary key of the entity will be set if Insert succeeds</param>
        /// <param name="transaction">The <see cref="IDotEntityTransaction"/> transaction under which the operation executes</param>
        /// <param name="action">(optional) The function to capture the inserted entity. The return value of this function determines if the next operation in the transaction executes or not. Default return value is True</param>
        public static void Insert(T entity, IDotEntityTransaction transaction, Func<T, bool> action = null)
        {
            Throw.IfTransactionIsNullOrDisposed(transaction.IsNullOrDisposed(), nameof(transaction));
            transaction.Manager.AsDotEntityQueryManager().DoInsert(entity, action);
        }

        /// <summary>
        /// Deletes the entity of type <typeparamref name="T"/> from database
        /// </summary>
        /// <param name="entity">The entity to be deleted</param>
        public static void Delete(T entity)
        {
            using (var manager = new DotEntityQueryManager())
            {
                manager.DoDelete(entity);
            }
        }

        /// <summary>
        /// Deletes the entity of type <typeparamref name="T"/> from database within a transaction
        /// </summary>
        /// <param name="entity">The entity to be deleted</param>
        /// <param name="transaction">The <see cref="IDotEntityTransaction"/> transaction under which the operation executes</param>
        /// <param name="action">(optional) The function to capture the inserted entity. The return value of this function determines if the next operation in the transaction executes or not. Default return value is True</param>
        public static void Delete(T entity, IDotEntityTransaction transaction, Func<T, bool> action = null)
        {
            Throw.IfTransactionIsNullOrDisposed(transaction.IsNullOrDisposed(), nameof(transaction));
            transaction.Manager.AsDotEntityQueryManager().DoDelete(entity, action);
        }

        /// <summary>
        /// Deletes the matching entities from the database table
        /// </summary>
        /// <param name="where">The expression to filter entities. This translates to WHERE clause in SQL</param>
        public static void Delete(Expression<Func<T, bool>> where)
        {
            using (var manager = new DotEntityQueryManager())
            {
                manager.DoDelete(where);
            }
        }
        /// <summary>
        /// Deletes the matching entities from the database within a transaction
        /// </summary>
        /// <param name="where">The expression to filter entities</param>
        /// <param name="transaction">The <see cref="IDotEntityTransaction"/> transaction under which the operation executes</param>
        public static void Delete(Expression<Func<T, bool>> where, IDotEntityTransaction transaction)
        {
            Throw.IfTransactionIsNullOrDisposed(transaction.IsNullOrDisposed(), nameof(transaction));
            transaction.Manager.AsDotEntityQueryManager().DoDelete<T>(where);
        }

        /// <summary>
        /// Updates the database table row that matches the primary key of the provided entity with the entity property values
        /// </summary>
        /// <param name="entity">The entity object for performing the update</param>
        public static void Update(T entity)
        {
            using (var manager = new DotEntityQueryManager())
            {
                manager.DoUpdate(entity);
            }
        }

        /// <summary>
        /// Updates the database table row that matches the provided <paramref name="where"/> expression
        /// </summary>
        /// <param name="entity">A dynamic object containing the fields to be updated with their new values</param>
        /// <param name="where">The filter expression to select appropriate database rows to update. This translates to WHERE clause in SQL</param>
        public static void Update(object entity, Expression<Func<T, bool>> where)
        {
            using (var manager = new DotEntityQueryManager())
            {
                manager.DoUpdate(entity, where);
            }
        }

        /// <summary>
        /// Updates the database table row that matches the primary key of the provided entity with the entity property values within a transaction
        /// </summary>
        /// <param name="entity">The entity object for performing the update</param>
        /// <param name="transaction">The <see cref="IDotEntityTransaction"/> transaction under which the operation executes</param>
        /// <param name="action">(optional) The function to capture the updated entity. The return value of this function determines if the next operation in the transaction executes or not. Default return value is True</param>
        public static void Update(T entity, IDotEntityTransaction transaction, Func<T, bool> action = null)
        {
            Throw.IfTransactionIsNullOrDisposed(transaction.IsNullOrDisposed(), nameof(transaction));
            transaction.Manager.AsDotEntityQueryManager().DoUpdate(entity, action);
        }

        /// <summary>
        /// Updates the database table row that matches the provided <paramref name="where"/> expression
        /// </summary>
        /// <param name="entity">A dynamic object containing the fields to be updated with their new values</param>
        /// <param name="where">The filter expression to select appropriate database rows to update. This translates to WHERE clause in SQL</param>
        /// <param name="transaction">The <see cref="IDotEntityTransaction"/> transaction under which the operation executes</param>
        /// <param name="action">(optional) The function to capture the updated entity. The return value of this function determines if the next operation in the transaction executes or not. Default return value is True</param>
        public static void Update(object entity, Expression<Func<T, bool>> where, IDotEntityTransaction transaction, Func<T, bool> action = null)
        {
            Throw.IfTransactionIsNullOrDisposed(transaction.IsNullOrDisposed(), nameof(transaction));
            transaction.Manager.AsDotEntityQueryManager().DoUpdate(entity, where, action);
        }

        /// <summary>
        /// Queries the database for the requested entities
        /// </summary>
        /// <returns>An enumeration of <typeparamref name="T"/></returns>
        public static IEnumerable<T> Select()
        {
            using (var manager = new DotEntityQueryManager())
            {
                return manager.DoSelect<T>();
            }
        }

        /// <summary>
        /// Executes the provided <paramref name="query"/> against the data provider
        /// </summary>
        /// <param name="query">The query to be executed against the provider. The query parameters references should be named with '@' prefix</param>
        /// <param name="parameters">(optional) A dynamic object containing the parameters used in the query</param>
        /// <param name="queryCache">(optional) The query cache to be used for the query</param>
        /// <param name="cacheParameters">(optional) The cache parameters that'll be replaced in the query</param>
        /// <returns>An enumeration of <typeparamref name="T"/></returns>
        public static IEnumerable<T> Query(string query, object parameters = null, QueryCache queryCache = null, object[] cacheParameters = null)
        {
            if (queryCache != null)
                queryCache.ParameterValues = cacheParameters;
            using (var manager = new DotEntityQueryManager(queryCache))
            {
                return manager.Do<T>(query, parameters);
            }
        }

        /// <summary>
        /// Executes the provided <paramref name="query"/> against the data provider and returns the first result of the set
        /// </summary>
        /// <param name="query">The query to be executed against the provider. The query parameters references should be named with '@' prefix</param>
        /// <param name="parameters">(optional) A dynamic object containing the parameters used in the query</param>
        /// <param name="queryCache">(optional) The query cache to be used for the query</param>
        /// <param name="cacheParameters">(optional) The cache parameters that'll be replaced in the query</param>
        /// <returns>An enumeration of <typeparamref name="T"/></returns>
        public static T QuerySingle(string query, object parameters = null, QueryCache queryCache = null, object[] cacheParameters = null)
        {
            if (queryCache != null)
                queryCache.ParameterValues = cacheParameters;
            using (var manager = new DotEntityQueryManager(queryCache))
            {
                return manager.DoSingle<T>(query, parameters);
            }
        }

        /// <summary>
        /// Executes the provided <paramref name="query"/> against the data provider within a transaction
        /// </summary>
        /// <param name="query">The query to be executed against the provider. The query parameters references should be named with '@' prefix</param>
        /// <param name="parameters">A dynamic object containing the parameters used in the query</param>
        /// <param name="transaction">The <see cref="IDotEntityTransaction"/> transaction under which the operation executes</param>
        /// <param name="action">(optional) The function to capture the updated entities. The entities are provided as parameter to the function. The return value of this function determines if the next operation in the transaction executes or not. Default return value is True</param>
        public static void Query(string query, object parameters, IDotEntityTransaction transaction, Func<IEnumerable<T>, bool> action = null)
        {
            Throw.IfTransactionIsNullOrDisposed(transaction.IsNullOrDisposed(), nameof(transaction));
            transaction.Manager.AsDotEntityQueryManager().Do<T>(query, parameters, action);
        }

        /// <summary>
        /// Executes the provided <paramref name="query"/> against the data provider and returns value of first row and first column of the result.
        /// </summary>
        /// <typeparam name="TType">The type of the return value</typeparam> 
        /// <param name="query">The query to be executed against the provider. The query parameters references should be named with '@' prefix</param>
        /// <param name="parameters">(optional) A dynamic object containing the parameters used in the query</param>
        /// <returns>A value of type <typeparamref name="TType"/></returns>
        public static TType QueryScaler<TType>(string query, object parameters = null)
        {
            using (var manager = new DotEntityQueryManager())
            {
                return manager.DoScaler<TType>(query, parameters);
            }
        }

        /// <summary>
        /// Executes the provided <paramref name="query"/> against the data provider within a transaction and returns value of first row and first column of the result.
        /// </summary>
        /// <typeparam name="TType">The type of the return value</typeparam> 
        /// <param name="query">The query to be executed against the provider. The query parameters references should be named with '@' prefix</param>
        /// <param name="parameters">A dynamic object containing the parameters used in the query</param>
        /// <param name="transaction">The <see cref="IDotEntityTransaction"/> transaction under which the operation executes</param>
        /// <param name="action">(optional) The function to capture the updated entities. The entities are provided as parameter to the function. The return value of this function determines if the next operation in the transaction executes or not. Default return value is True</param>
        public static void QueryScaler<TType>(string query, object parameters, IDotEntityTransaction transaction, Func<TType, bool> action = null)
        {
            Throw.IfTransactionIsNullOrDisposed(transaction.IsNullOrDisposed(), nameof(transaction));
            transaction.Manager.AsDotEntityQueryManager().DoScaler<TType>(query, parameters, action);
        }

        /// <summary>
        /// Specifies the where expression to filter the repository for subsequent operation
        /// </summary>
        /// <param name="where">The filter expression to select appropriate database rows to update. This translates to WHERE clause in SQL</param>
        /// <returns></returns>
        public static IEntitySet<T> Where(Expression<Func<T, bool>> where)
        {
            return Instance.Where(where);
        }

        /// <summary>
        /// Counts the total number of matching entities
        /// </summary>
        /// <returns>Total number of matching entities</returns>
        public static int Count()
        {
            return Instance.Count();
        }

        /// <summary>
        /// Queries the database for the requested entities, additionally finding total number of matching records
        /// </summary>
        /// <param name="totalMatches">The total number of matching entities</param>
        /// <param name="page">(optional) The page number of the result set. Default is 1</param>
        /// <param name="count">(optional) The number of entities to return. Defaults to all entities</param>
        /// <returns>An enumeration of <typeparamref name="T"/></returns>
        public static IEnumerable<T> SelectWithTotalMatches(out int totalMatches, int page = 1, int count = Int32.MaxValue)
        {
            return Instance.SelectWithTotalMatches(out totalMatches, page, count);
        }

        /// <summary>
        /// Specifies that the query should be picked up from the cache provided.  This is useful to execute multiple queries with same signature. All the expressions in such queries are evaluated only once
        /// </summary>
        /// <param name="cache">The query cache to use</param>
        /// <param name="parameterValues">The parameter values to be replaced in the cached query</param>
        /// <returns>An implementation object of type <see cref="IEntitySet{T}"/></returns>
        public static IEntitySet<T> WithQueryCache(QueryCache cache, params object[] parameterValues)
        {
            return Instance.WithQueryCache(cache, parameterValues);
        }

        public IList<object[]> CustomSelect(string rawSelection, int page = 1, int count = Int32.MaxValue)
        {
            using (var manager = new DotEntityQueryManager(_cache))
            {
                return manager.DoCustomSelect(rawSelection, _whereList, _orderBy, page, count);
            }
        }

        public IList<object[]> CustomSelectNested(string rawSelection, int page = 1, int count = Int32.MaxValue)
        {
            using (var manager = new DotEntityQueryManager())
            {
                if (_joinWhereList == null)
                    _joinWhereList = new List<LambdaExpression>();

                if (_joinOrderBy == null)
                    _joinOrderBy = new Dictionary<LambdaExpression, RowOrder>();

                //move all order by and where to these list
                foreach (var w in _whereList)
                    _joinWhereList.Add(w);

                foreach (var o in _orderBy)
                    _joinOrderBy.Add(o.Key, o.Value);

                //always use explicit select mode for joins
                var selectMode = DotEntityDb.SelectQueryMode;
                DotEntityDb.SelectQueryMode = SelectQueryMode.Explicit;
                var entities = manager.DoCustomSelect<T>(rawSelection, _joinList, _relationActions, _joinWhereList, _joinOrderBy, page, count);
                //reset select mode
                DotEntityDb.SelectQueryMode = selectMode;
                return entities;
            }
        }

        public IEntitySet<T> SkipColumns<T1>(params string[] columns)
        {
            _skip = _skip ?? new Dictionary<Type, IList<string>>();
            if (!_skip.TryGetValue(typeof(T1), out var columnsList))
            {
                columnsList = new List<string>();
                _skip.Add(typeof(T1), columnsList);
            }

            foreach (var column in columns)
            {
                if (!columnsList.Contains(column))
                    columnsList.Add(column);
            }

            return this;
        }

        public bool ContainJoins()
        {
            return _joinList != null && _joinList.Count > 0;
        }

        public static IEntitySet<T> Just()
        {
            return Instance;
        }
        #endregion

        #region implementations

        IEntitySet<T> IEntitySet<T>.Where(Expression<Func<T, bool>> where)
        {
            _whereList.Add(where);
            return this;
        }

        IEntitySet<T> IEntitySet<T>.Where(LambdaExpression where)
        {
            if (_joinWhereList == null)
                _joinWhereList = new List<LambdaExpression>();

            _joinWhereList.Add(where);
            return this;
        }

        public IEntitySet<T> Join<T1>(string sourceColumnName, string destinationColumnName, Type sourceColumnType, JoinType joinType = JoinType.Inner, int sourceColumnAppearanceOrder = 0, Expression<Func<T, T1, bool>> additionalExpression = null) where T1 : class
        {
            if (_joinList == null)
                _joinList = new List<IJoinMeta>();

            _joinList.Add(new JoinMeta<T1>(sourceColumnName, destinationColumnName, sourceColumnType, joinType, sourceColumnAppearanceOrder, additionalExpression));
            return this;
        }

        IEntitySet<T> IEntitySet<T>.OrderBy(Expression<Func<T, object>> orderBy, RowOrder rowOrder)
        {
            _orderBy.Add(orderBy, rowOrder);
            return this;
        }

        IEntitySet<T> IEntitySet<T>.OrderBy(LambdaExpression orderBy, RowOrder rowOrder)
        {
            if (_joinOrderBy == null)
                _joinOrderBy = new Dictionary<LambdaExpression, RowOrder>();
            _joinOrderBy.Add(orderBy, rowOrder);
            return this;
        }

        IEnumerable<T> IEntitySet<T>.Select(int page, int count)
        {
            using (var manager = new DotEntityQueryManager(_cache))
            {
                return manager.DoSelect(_whereList, _orderBy, page, count);
            }
        }

        IEnumerable<T> IEntitySet<T>.SelectWithTotalMatches(out int totalMatches, int page, int count)
        {
            using (var manager = new DotEntityQueryManager())
            {
                var selectWithCount = manager.DoSelectWithTotalMatches(_whereList, _orderBy, page, count);
                totalMatches = selectWithCount.Item1;
                return selectWithCount.Item2;
            }
        }

        IEnumerable<T> IEntitySet<T>.SelectNested(int page, int count)
        {
            using (var manager = new DotEntityQueryManager())
            {
                if (_joinWhereList == null)
                    _joinWhereList = new List<LambdaExpression>();

                if (_joinOrderBy == null)
                    _joinOrderBy = new Dictionary<LambdaExpression, RowOrder>();

                //move all order by and where to these list
                foreach (var w in _whereList)
                    _joinWhereList.Add(w);

                foreach (var o in _orderBy)
                    _joinOrderBy.Add(o.Key, o.Value);

                //always use explicit select mode for joins
                var selectMode = DotEntityDb.SelectQueryMode;
                DotEntityDb.SelectQueryMode = SelectQueryMode.Explicit;
                var entities = manager.DoSelect<T>(_joinList, _relationActions, _joinWhereList, _joinOrderBy, page, count);
                //reset select mode
                DotEntityDb.SelectQueryMode = selectMode;
                return entities;
            }
        }

        public IEnumerable<T> SelectNestedWithTotalMatches(out int totalMatches, int page = 1, int count = Int32.MaxValue)
        {
            using (var manager = new DotEntityQueryManager())
            {
                if (_joinWhereList == null)
                    _joinWhereList = new List<LambdaExpression>();

                if (_joinOrderBy == null)
                    _joinOrderBy = new Dictionary<LambdaExpression, RowOrder>();

                //move all order by and where to these list
                foreach (var w in _whereList)
                    _joinWhereList.Add(w);

                foreach (var o in _orderBy)
                    _joinOrderBy.Add(o.Key, o.Value);

                //always use explicit select mode for joins
                var selectMode = DotEntityDb.SelectQueryMode;
                DotEntityDb.SelectQueryMode = SelectQueryMode.Explicit;
                var selectWithCount = manager.DoJoinWithTotalMatches<T>(_joinList, _relationActions, _joinWhereList, _joinOrderBy, page, count);
                //reset select mode
                DotEntityDb.SelectQueryMode = selectMode;
                totalMatches = selectWithCount.Item1;
                return selectWithCount.Item2;
            }
        }

        int IEntitySet<T>.Count(IDotEntityTransaction transaction)
        {
            if (transaction != null)
            {
                Throw.IfTransactionIsNullOrDisposed(transaction.IsNullOrDisposed(), nameof(transaction));
                return transaction.Manager.AsDotEntityQueryManager().DoCount(_whereList);
            }
            else
            {
                using (var manager = new DotEntityQueryManager())
                {
                    return manager.DoCount<T>(_whereList);
                }
            }

        }

        IEntitySet<T> IEntitySet<T>.Join<T1>(string sourceColumnName, string destinationColumnName, SourceColumn sourceColumn, JoinType joinType, Expression<Func<T, T1, bool>> additionalExpression = null)
        {
            if (_joinList == null)
                _joinList = new List<IJoinMeta>();

            _joinList.Add(new JoinMeta<T1>(sourceColumnName, destinationColumnName, sourceColumn, joinType, additionalExpression));
            return this;
        }

        IEntitySet<T> IEntitySet<T>.PlaceholderJoin<T1>()
        {
            if (_joinList == null)
                _joinList = new List<IJoinMeta>();

            _joinList.Add(new JoinMeta<T1>("", ""));
            return this;
        }

        private Dictionary<Type, Delegate> _relationActions;
        IEntitySet<T> IEntitySet<T>.Relate<T1>(Action<T, T1> relateAction)
        {
            if (_relationActions == null)
                _relationActions = new Dictionary<Type, Delegate>();

            var typeOfT1 = typeof(T1);
            if (!_relationActions.ContainsKey(typeOfT1))
                _relationActions.Add(typeOfT1, relateAction);

            return this;
        }

        T IEntitySet<T>.SelectSingle(IDotEntityTransaction transaction)
        {
            using (var manager = new DotEntityQueryManager())
            {
                return manager.DoSelectSingle(_whereList, _orderBy);
            }
        }

        private QueryCache _cache;
        IEntitySet<T> IEntitySet<T>.WithQueryCache(QueryCache cache, params object[] parameterValues)
        {
            _cache = cache;
            _cache.ParameterValues = parameterValues;
            return this;
        }

        /// <summary>
        /// Executes the provided <paramref name="query"/> against the data provider
        /// </summary>
        /// <param name="query">The query to be executed against the provider. The query parameters references should be named with '@' prefix</param>
        /// <param name="parameters">(optional) A dynamic object containing the parameters used in the query</param>
        /// <param name="isProcedure">(optional) Is the query provided a stored procedure?</param>
        /// <returns>An enumeration of <typeparamref name="T"/></returns>
        IEnumerable<T> IEntitySet<T>.QueryNested(string query, object parameters, bool isProcedure)
        {
            using (var manager = new DotEntityQueryManager())
            {
                //always use explicit select mode for joins
                var selectMode = DotEntityDb.SelectQueryMode;
                DotEntityDb.SelectQueryMode = SelectQueryMode.Explicit;
                var entities = manager.DoQuery<T>(query, parameters, _joinList, _relationActions, isProcedure);
                //reset select mode
                DotEntityDb.SelectQueryMode = selectMode;
                return entities;
            }
        }

       
        #endregion

        internal static IEntitySet<T> Instance => new EntitySet<T>();
    }
}