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

 * You can release yourself from the requirements of the license by purchasing
 * a commercial license.Buying such a license is mandatory as soon as you
 * develop commercial software involving the dotEntity software without
 * disclosing the source code of your own applications.
 * To know more about our commercial license email us at support@roastedbytes.com or
 * visit http://dotentity.net/legal/commercial
 */
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using DotEntity.Enumerations;

namespace DotEntity
{
    public interface IEntitySet<T> where T : class
    {
        IEntitySet<T> OrderBy(Expression<Func<T, object>> orderBy, RowOrder rowOrder = RowOrder.Ascending);

        IEntitySet<T> OrderBy(LambdaExpression orderBy, RowOrder rowOrder = RowOrder.Ascending);

        IEntitySet<T> Where(Expression<Func<T, bool>> where);

        IEntitySet<T> Where(LambdaExpression where);
        
        IEntitySet<T> Join<T1>(string sourceColumnName, string destinationColumnName, SourceColumn sourceColumn = SourceColumn.Chained, JoinType joinType = JoinType.Inner) where T1 : class;

        IEntitySet<T> Relate<T1>(Action<T, T1> relateAction) where T1 : class;

        IEnumerable<T> Select(int page = 1, int count = int.MaxValue, IDotEntityTransaction transaction = null);

        IEnumerable<T> SelectWithTotalMatches(out int totalMatches, int page = 1, int count = int.MaxValue, IDotEntityTransaction transaction = null);

        IEnumerable<T> SelectNested(int page = 1, int count = int.MaxValue, IDotEntityTransaction transaction = null);

        int Count(IDotEntityTransaction transaction = null);

        T SelectSingle(IDotEntityTransaction transaction = null);
    }
}