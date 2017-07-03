// #region Author Information
// // SpruceTable`.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using SpruceFramework.Enumerations;
using SpruceFramework.Extensions;

namespace SpruceFramework
{
    public sealed partial class SpruceTable<T> : ISpruceTable<T> where T : class
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
            using (var manager = new SpruceQueryManager())
            {
                manager.DoInsert(entity);
            }
        }

        public static void Insert(T[] entities)
        {
            using (var manager = new SpruceQueryManager())
            {
                manager.DoInsert(entities);
            }
        }

        public static void Insert(T entity, ISpruceTransaction transaction, Func<T, bool> action = null)
        {
            if (!transaction.IsNullOrDisposed())
            {
                transaction.Manager.AsSpruceQueryManager().DoInsert(entity, action);
            }
        }

       /* public static void Delete(T entity)
        {
            using (var manager = new SpruceQueryManager())
            {
                manager.DoDelete(entity);
            }
        }

        public static void Delete(T entity, ISpruceTransaction transaction, Func<T, bool> action = null)
        {
            if (!transaction.IsNullOrDisposed())
            {
                transaction.Manager.AsSpruceQueryManager().DoDelete(entity, action);
            }
        }*/

        public static void Delete(Expression<Func<T, bool>> where)
        {
            using (var manager = new SpruceQueryManager())
            {
                manager.DoDelete(where);
            }
        }
        public static void Delete(Expression<Func<T, bool>> where, ISpruceTransaction transaction)
        {
            if (!transaction.IsNullOrDisposed())
            {
                transaction.Manager.AsSpruceQueryManager().DoDelete<T>(where);
            }
        }
        
        public static void Update(T entity)
        {
            using (var manager = new SpruceQueryManager())
            {
                manager.DoUpdate(entity);
            }
        }

        public static void Update(dynamic entity, Expression<Func<T, bool>> where)
        {
            using (var manager = new SpruceQueryManager())
            {
                manager.DoUpdate(entity, where);
            }
        }

        public static void Update(T entity, ISpruceTransaction transaction, Func<T, bool> action = null)
        {
            if (!transaction.IsNullOrDisposed())
            {
                transaction.Manager.AsSpruceQueryManager().DoUpdate(entity, action);
            }
        }

        public static void Update(dynamic entity, Expression<Func<T, bool>> where, ISpruceTransaction transaction, Func<T, bool> action = null)
        {
            if (!transaction.IsNullOrDisposed())
            {
                transaction.Manager.AsSpruceQueryManager().DoUpdate(entity, where, action);
            }
        }

        public static IEnumerable<T> Select()
        {
            using (var manager = new SpruceQueryManager())
            {
                return manager.DoSelect<T>();
            }
        }
        public static IEnumerable<T> Query(string query, dynamic parameters = null)
        {
            using (var manager = new SpruceQueryManager())
            {
                return manager.Do<T>(query, parameters);
            }
        }

        public static void Query(string query, dynamic parameters, ISpruceTransaction transaction, Func<IEnumerable<T>, bool> action = null)
        {
            if (!transaction.IsNullOrDisposed())
            {
                transaction.Manager.AsSpruceQueryManager().Do<T>(query, parameters, action);
            }
        }

        public static TType QueryScaler<TType>(string query, dynamic parameters = null)
        {
            using (var manager = new SpruceQueryManager())
            {
                return manager.DoScaler<TType>(query, parameters);
            }
        }

        public static void QueryScaler<TType>(string query, dynamic parameters, ISpruceTransaction transaction, Func<TType, bool> action = null)
        {
            if (!transaction.IsNullOrDisposed())
            {
                transaction.Manager.AsSpruceQueryManager().DoScaler<TType>(query, parameters, action);
            }
        }

        public static ISpruceTable<T> Where(Expression<Func<T, bool>> where)
        {
            return Instance.Where(where);
        }

        public static int Count()
        {
            return Instance.Count();
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

        IEnumerable<T> ISpruceTable<T>.Select(int page, int count, ISpruceTransaction transaction)
        {
            using (var manager = new SpruceQueryManager())
            {
                return manager.DoSelect(_whereList, _orderBy, page, count);
            }
        }

        IEnumerable<T> ISpruceTable<T>.SelectNested(int page, int count, ISpruceTransaction transaction)
        {
            using (var manager = new SpruceQueryManager())
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

                return manager.DoSelect<T>(_joinList, _relationActions, _joinWhereList, _joinOrderBy, page, count);
            }
        }

        int ISpruceTable<T>.Count(ISpruceTransaction transaction)
        {
            using (var manager = new SpruceQueryManager())
            {
                return manager.DoCount<T>(_whereList, null);
            }
        }

        ISpruceTable<T> ISpruceTable<T>.Join<T1>(string sourceColumnName, string destinationColumnName, SourceColumn sourceColumn, JoinType joinType)
        {
            if(_joinList == null)
                _joinList = new List<IJoinMeta>();

            _joinList.Add(new JoinMeta<T1>(sourceColumnName, destinationColumnName, sourceColumn, joinType));
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

        T ISpruceTable<T>.SelectSingle(ISpruceTransaction transaction)
        {
            using (var manager = new SpruceQueryManager())
            {
                return manager.DoSelectSingle(_whereList, _orderBy);
            }
        }

       
        #endregion

        internal static ISpruceTable<T> Instance => new SpruceTable<T>();
    }
}