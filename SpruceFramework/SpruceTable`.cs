// #region Author Information
// // SpruceTable`.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using SpruceFramework.Enumerations;

namespace SpruceFramework
{
    public sealed class SpruceTable<T> : ISpruceTable<T> where T : class
    {
        private readonly List<Expression<Func<T, bool>>> _whereList;
        private readonly Dictionary<Expression<Func<T, object>>, RowOrder> _orderBy;

        private List<IJoinMeta> _joinList;
        private List<LambdaExpression> _joinWhereList;
        private Dictionary<LambdaExpression, RowOrder> _joinOrderBy;

        internal SpruceTable()
        {
            _whereList = new List<Expression<Func<T, bool>>>();
            _orderBy = new Dictionary<Expression<Func<T, object>>, RowOrder>();
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

        public static TType QueryScaler<TType>(string query, dynamic parameters)
        {
            using (var manager = new SpruceQueryManager<T>())
            {
                return manager.DoScaler<TType>(query, parameters);
            }
        }
        public static ISpruceTable<T> Where(Expression<Func<T, bool>> where)
        {
            return Instance.Where(where);
        }

        #endregion

        #region implementations

        ISpruceTable<T> ISpruceTable<T>.Where(Expression<Func<T, bool>> where)
        {
            _whereList.Add(where);
            return this;
        }

        ISpruceTable<T> ISpruceTable<T>.Where(LambdaExpression where)
        {
            if(_joinWhereList == null)
                _joinWhereList = new List<LambdaExpression>();

            _joinWhereList.Add(where);
            return this;
        }

        ISpruceTable<T> ISpruceTable<T>.OrderBy(Expression<Func<T, object>> orderBy, RowOrder rowOrder)
        {
            _orderBy.Add(orderBy, rowOrder);
            return this;
        }

        ISpruceTable<T> ISpruceTable<T>.OrderBy(LambdaExpression orderBy, RowOrder rowOrder)
        {
            if(_joinOrderBy == null)
                _joinOrderBy = new Dictionary<LambdaExpression, RowOrder>();
            _joinOrderBy.Add(orderBy, rowOrder);
            return this;
        }

        T[] ISpruceTable<T>.Select(int page, int count)
        {
            using (var manager = new SpruceQueryManager<T>())
            {
                return manager.DoSelect(_whereList, _orderBy, page, count);
            }
        }

        T[] ISpruceTable<T>.SelectNested(int page, int count)
        {
            using (var manager = new SpruceQueryManager<T>())
            {
                if(_joinWhereList == null)
                    _joinWhereList = new List<LambdaExpression>();

                if(_joinOrderBy == null)
                    _joinOrderBy = new Dictionary<LambdaExpression, RowOrder>();

                //move all order by and where to these list
                foreach(var w in _whereList)
                    _joinWhereList.Add(w);

                foreach(var o in _orderBy)
                    _joinOrderBy.Add(o.Key, o.Value);

                return manager.DoSelect(_joinList, _relationActions, _joinWhereList, _joinOrderBy, page, count);
            }
        }

        ISpruceTable<T> ISpruceTable<T>.Join<T1>(string sourceColumnName, string destinationColumnName)
        {
            if(_joinList == null)
                _joinList = new List<IJoinMeta>();

            _joinList.Add(new JoinMeta<T1>(sourceColumnName, destinationColumnName));
            return this;
        }

        private Dictionary<Type, Delegate> _relationActions;
        ISpruceTable<T> ISpruceTable<T>.Relate<T1>(Action<T, T1> relateAction)
        {
            if (_relationActions == null)
                _relationActions = new Dictionary<Type, Delegate>();

            var typeOfT1 = typeof(T1);
            if (!_relationActions.ContainsKey(typeOfT1))
                _relationActions.Add(typeOfT1, relateAction);

            return this;
        }

        T ISpruceTable<T>.SelectSingle()
        {
            using (var manager = new SpruceQueryManager<T>())
            {
                return manager.DoSelectSingle(_whereList, _orderBy);
            }
        }

       
        #endregion

        internal static ISpruceTable<T> Instance => new SpruceTable<T>();
    }
}