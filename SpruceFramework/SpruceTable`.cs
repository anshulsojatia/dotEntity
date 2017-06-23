// #region Author Information
// // SpruceTable`.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using SpruceFramework.Enumerations;

namespace SpruceFramework
{
    public sealed class SpruceTable<T> : ISpruceTable<T> where T : class
    {
        private readonly List<Expression<Func<T, bool>>> _whereList;
        private readonly Dictionary<Expression<Func<T, object>>, RowOrder> _orderBy;
        
        private IEnumerable<T> _entities;

        internal SpruceTable()
        {
            _whereList = new List<Expression<Func<T, bool>>>();
            _orderBy = new Dictionary<Expression<Func<T, object>>, RowOrder>();
            _entities = new List<T>();
        }

        #region static wrappers
        public static void Insert(T entity)
        {
            using (var manager = new SpruceQueryManager<T>())
            {
                manager.DoInsert(entity);
            }
        }

        public static void Delete(T entity)
        {
            using (var manager = new SpruceQueryManager<T>())
            {
                manager.DoDelete(entity);
            }
        }

        public static void Delete(Expression<Func<T, bool>> where)
        {

        }

        public static void Update(T entity)
        {

        }

        public static T[] Query(string query, dynamic parameters)
        {
            using (var manager = new SpruceQueryManager<T>())
            {
                return manager.Do(query, parameters);
            }
        }

        public static ISpruceTable<T> Where(Expression<Func<T, bool>> where)
        {
            return Instance.Where(where);
        }


        public static ISpruceTable<T> Join<TType>(Expression<Func<T, object>> sourceColumn, Expression<Func<TType, object>> targetColumn, Action<T, TType> mapperFunction)
        {
            return Instance.Join(sourceColumn, targetColumn, mapperFunction);
        }
        #endregion

        #region implementations

        ISpruceTable<T> ISpruceTable<T>.Where(Expression<Func<T, bool>> where)
        {
            _whereList.Add(where);
            return this;
        }

        ISpruceTable<T> ISpruceTable<T>.Join<TType>(Expression<Func<T, object>> sourceColumn, Expression<Func<TType, object>> targetColumn, Action<T, TType> mapperFunction)
        {
            return this;
        }

        ISpruceTable<T> ISpruceTable<T>.OrderBy(Expression<Func<T, object>> orderBy, RowOrder rowOrder)
        {
            _orderBy.Add(orderBy, rowOrder);
            return this;
        }

        T[] ISpruceTable<T>.Select()
        {
            using (var manager = new SpruceQueryManager<T>())
            {
                return manager.DoSelect(_whereList, _orderBy, 1, 30);
            }
        }

        T ISpruceTable<T>.SelectSingle()
        {
            using (var manager = new SpruceQueryManager<T>())
            {
                return manager.DoSelectSingle(_whereList, _orderBy, 1, 30);
            }
        }

        #endregion
        
        internal static ISpruceTable<T> Instance
        {
            get
            {
                return new SpruceTable<T>();
            }
        }
    }
}