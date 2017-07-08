// #region Author Information
// // IEntitySet.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion
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