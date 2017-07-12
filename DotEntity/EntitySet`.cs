// #region Author Information
// // EntitySet`.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using DotEntity.Enumerations;
using DotEntity.Extensions;

namespace DotEntity
{
    public sealed partial class EntitySet<T> : IEntitySet<T> where T : class
    {
        private readonly List<Expression<Func<T, bool>>> _whereList;
        private readonly Dictionary<Expression<Func<T, object>>, RowOrder> _orderBy;

        private List<IJoinMeta> _joinList;
        private List<LambdaExpression> _joinWhereList;
        private Dictionary<LambdaExpression, RowOrder> _joinOrderBy;

        internal EntitySet()
        {
            _whereList = new List<Expression<Func<T, bool>>>();
            _orderBy = new Dictionary<Expression<Func<T, object>>, RowOrder>();
        }

        #region static wrappers
        public static void Insert(T entity)
        {
            using (var manager = new DotEntityQueryManager())
            {
                manager.DoInsert(entity);
            }
        }

        public static void Insert(T[] entities)
        {
            using (var manager = new DotEntityQueryManager())
            {
                manager.DoInsert(entities);
            }
        }

        public static void Insert(T entity, IDotEntityTransaction transaction, Func<T, bool> action = null)
        {
            if (!transaction.IsNullOrDisposed())
            {
                transaction.Manager.AsDotEntityQueryManager().DoInsert(entity, action);
            }
        }

       /* public static void Delete(T entity)
        {
            using (var manager = new DotEntityQueryManager())
            {
                manager.DoDelete(entity);
            }
        }

        public static void Delete(T entity, IDotEntityTransaction transaction, Func<T, bool> action = null)
        {
            if (!transaction.IsNullOrDisposed())
            {
                transaction.Manager.AsDotEntityQueryManager().DoDelete(entity, action);
            }
        }*/

        public static void Delete(Expression<Func<T, bool>> where)
        {
            using (var manager = new DotEntityQueryManager())
            {
                manager.DoDelete(where);
            }
        }
        public static void Delete(Expression<Func<T, bool>> where, IDotEntityTransaction transaction)
        {
            if (!transaction.IsNullOrDisposed())
            {
                transaction.Manager.AsDotEntityQueryManager().DoDelete<T>(where);
            }
        }
        
        public static void Update(T entity)
        {
            using (var manager = new DotEntityQueryManager())
            {
                manager.DoUpdate(entity);
            }
        }

        public static void Update(dynamic entity, Expression<Func<T, bool>> where)
        {
            using (var manager = new DotEntityQueryManager())
            {
                manager.DoUpdate(entity, where);
            }
        }

        public static void Update(T entity, IDotEntityTransaction transaction, Func<T, bool> action = null)
        {
            if (!transaction.IsNullOrDisposed())
            {
                transaction.Manager.AsDotEntityQueryManager().DoUpdate(entity, action);
            }
        }

        public static void Update(dynamic entity, Expression<Func<T, bool>> where, IDotEntityTransaction transaction, Func<T, bool> action = null)
        {
            if (!transaction.IsNullOrDisposed())
            {
                transaction.Manager.AsDotEntityQueryManager().DoUpdate(entity, where, action);
            }
        }

        public static IEnumerable<T> Select()
        {
            using (var manager = new DotEntityQueryManager())
            {
                return manager.DoSelect<T>();
            }
        }
        public static IEnumerable<T> Query(string query, dynamic parameters = null)
        {
            using (var manager = new DotEntityQueryManager())
            {
                return manager.Do<T>(query, parameters);
            }
        }

        public static void Query(string query, dynamic parameters, IDotEntityTransaction transaction, Func<IEnumerable<T>, bool> action = null)
        {
            if (!transaction.IsNullOrDisposed())
            {
                transaction.Manager.AsDotEntityQueryManager().Do<T>(query, parameters, action);
            }
        }

        public static TType QueryScaler<TType>(string query, dynamic parameters = null)
        {
            using (var manager = new DotEntityQueryManager())
            {
                return manager.DoScaler<TType>(query, parameters);
            }
        }

        public static void QueryScaler<TType>(string query, dynamic parameters, IDotEntityTransaction transaction, Func<TType, bool> action = null)
        {
            if (!transaction.IsNullOrDisposed())
            {
                transaction.Manager.AsDotEntityQueryManager().DoScaler<TType>(query, parameters, action);
            }
        }

        public static IEntitySet<T> Where(Expression<Func<T, bool>> where)
        {
            return Instance.Where(where);
        }

        public static int Count()
        {
            return Instance.Count();
        }

        public static IEnumerable<T> SelectWithTotalMatches(out int totalMatches, int page = 1,
            int count = Int32.MaxValue, IDotEntityTransaction transaction = null)
        {
            return Instance.SelectWithTotalMatches(out totalMatches, page, count, transaction);
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
            if(_joinWhereList == null)
                _joinWhereList = new List<LambdaExpression>();

            _joinWhereList.Add(where);
            return this;
        }

        IEntitySet<T> IEntitySet<T>.OrderBy(Expression<Func<T, object>> orderBy, RowOrder rowOrder)
        {
            _orderBy.Add(orderBy, rowOrder);
            return this;
        }

        IEntitySet<T> IEntitySet<T>.OrderBy(LambdaExpression orderBy, RowOrder rowOrder)
        {
            if(_joinOrderBy == null)
                _joinOrderBy = new Dictionary<LambdaExpression, RowOrder>();
            _joinOrderBy.Add(orderBy, rowOrder);
            return this;
        }

        IEnumerable<T> IEntitySet<T>.Select(int page, int count, IDotEntityTransaction transaction)
        {
            using (var manager = new DotEntityQueryManager())
            {
                return manager.DoSelect(_whereList, _orderBy, page, count);
            }
        }

        IEnumerable<T> IEntitySet<T>.SelectWithTotalMatches(out int totalMatches, int page, int count, IDotEntityTransaction transaction)
        {
            using (var manager = new DotEntityQueryManager())
            {
                var selectWithCount = manager.DoSelectWithTotalMatches(_whereList, _orderBy, page, count);
                totalMatches = selectWithCount.Item1;
                return selectWithCount.Item2;
            }
        }

        IEnumerable<T> IEntitySet<T>.SelectNested(int page, int count, IDotEntityTransaction transaction)
        {
            using (var manager = new DotEntityQueryManager())
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

        int IEntitySet<T>.Count(IDotEntityTransaction transaction)
        {
            if (transaction != null)
            {
                Throw.IfNullOrDisposed(transaction, nameof(transaction));
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

        IEntitySet<T> IEntitySet<T>.Join<T1>(string sourceColumnName, string destinationColumnName, SourceColumn sourceColumn, JoinType joinType)
        {
            if(_joinList == null)
                _joinList = new List<IJoinMeta>();

            _joinList.Add(new JoinMeta<T1>(sourceColumnName, destinationColumnName, sourceColumn, joinType));
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

       
        #endregion

        internal static IEntitySet<T> Instance => new EntitySet<T>();
    }
}