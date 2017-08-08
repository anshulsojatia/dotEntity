/**
 * Copyright(C) 2017  Apexol Technologies
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

        #endregion

        #region implementations

        IEntitySet<T> IEntitySet<T>.Where(Expression<Func<T, bool>> where)
        {
            _whereList.Add(where);
            return this;
        }

        IEntitySet<T> IEntitySet<T>.OrderBy(Expression<Func<T, object>> orderBy, RowOrder rowOrder)
        {
            _orderBy.Add(orderBy, rowOrder);
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

        int IEntitySet<T>.Count()
        {
            using (var manager = new DotEntityQueryManager())
            {
                return manager.DoCount<T>(_whereList);
            }
        }

        T IEntitySet<T>.SelectSingle()
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

        #endregion

        internal static IEntitySet<T> Instance => new EntitySet<T>();
    }
}