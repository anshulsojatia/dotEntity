// #region Author Information
// // ISpruceTable.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion
using SpruceFramework.Enumerations;
using System;
using System.Linq.Expressions;

namespace SpruceFramework
{
    public interface ISpruceTable<T> where T : class
    {
        ISpruceTable<T> Join<TType>(Expression<Func<T, object>> sourceColumn, Expression<Func<TType, object>> targetColumn, Action<T, TType> mapperFunction);

        ISpruceTable<T> OrderBy(Expression<Func<T, object>> orderBy, RowOrder rowOrder = RowOrder.Ascending);

        ISpruceTable<T> Where(Expression<Func<T, bool>> where);

        T[] Select();

        T SelectSingle();
    }
}