// #region Author Information
// // ISpruceTable.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion
using System;
using System.Linq.Expressions;
using SpruceFramework.Enumerations;

namespace SpruceFramework
{
    public interface ISpruceTable<T> where T : class
    {
        ISpruceTable<T> OrderBy(Expression<Func<T, object>> orderBy, RowOrder rowOrder = RowOrder.Ascending);

        ISpruceTable<T> OrderBy(LambdaExpression orderBy, RowOrder rowOrder = RowOrder.Ascending);

        ISpruceTable<T> Where(Expression<Func<T, bool>> where);

        ISpruceTable<T> Where(LambdaExpression where);
        
        ISpruceTable<T> Join<T1>(string sourceColumnName, string destinationColumnName) where T1 : class;

        ISpruceTable<T> Relate<T1>(Action<T, T1> relateAction) where T1 : class;

        T[] Select(int page = 1, int count = int.MaxValue);

        T[] SelectNested(int page = 1, int count = int.MaxValue);

        T SelectSingle();
    }
}