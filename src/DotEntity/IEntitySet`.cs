/**
 * Copyright(C) 2017  Apexol Technologies
 * 
 * This file (IEntitySet`.cs) is part of dotEntity(https://github.com/RoastedBytes/dotentity).
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
using System.Linq.Expressions;
using DotEntity.Enumerations;

namespace DotEntity
{
    /// <summary>
    /// Represents a database table of type <typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T">The entity class that maps to a specific database table</typeparam>
    public interface IEntitySet<T> where T : class
    {
        /// <summary>
        /// Orders the result set according to the <paramref name="orderBy"/> expression field
        /// </summary>
        /// <param name="orderBy">The expression specifying the field to use in ORDER BY clause</param>
        /// <param name="rowOrder">(optional) Specifies the <see cref="RowOrder"/>. Default is <see cref="RowOrder.Ascending"/></param>
        /// <returns>An implementation object of type <see cref="IEntitySet{T}"/></returns>
        IEntitySet<T> OrderBy(Expression<Func<T, object>> orderBy, RowOrder rowOrder = RowOrder.Ascending);

        /// <summary>
        /// Specifies the where expression to filter the repository for subsequent operations
        /// </summary>
        /// <param name="where">The filter expression to select appropriate database rows to update. This translates to WHERE clause in SQL</param>
        /// <returns>An implementation object of type <see cref="IEntitySet{T}"/></returns>
        IEntitySet<T> Where(Expression<Func<T, bool>> where);

        /// <summary>
        /// Queries the database for the requested entities
        /// </summary>
        /// <param name="page">(optional) The page number of the result set. Default is 1</param>
        /// <param name="count">(optional) The number of entities to return. Defaults to all entities</param>
        /// <returns>An enumeration of <typeparamref name="T"/></returns>
        IEnumerable<T> Select(int page = 1, int count = int.MaxValue);

        /// <summary>
        /// Queries the database for the requested entities, additionally finding total number of matching records
        /// </summary>
        /// <param name="totalMatches">The total number of matching entities</param>
        /// <param name="page">(optional) The page number of the result set. Default is 1</param>
        /// <param name="count">(optional) The number of entities to return. Defaults to all entities</param>
        /// <returns>An enumeration of <typeparamref name="T"/></returns>
        IEnumerable<T> SelectWithTotalMatches(out int totalMatches, int page = 1, int count = int.MaxValue);

        /// <summary>
        /// Counts the total number of matching entities within or without a transaction
        /// </summary>
        /// <returns>Total number of matching entities</returns>
        int Count();

        /// <summary>
        /// Queries the database for the requested entities within or without a transaction and returns the first entity of the result set
        /// </summary>
        /// <returns>The entity of type <typeparamref name="T"/> or null in case of empty result set</returns>
        T SelectSingle();
    }
}