// #region Author Information
// // Accessor.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using System;
using System.Linq.Expressions;
using System.Reflection;

namespace DotEntity.Reflection
{
    public class Accessor<TS>
    {
        public static Accessor<TS, T> Create<T>(Expression<Func<TS, T>> memberSelector)
        {
            return new GetterSetter<T>(memberSelector);
        }

        public Accessor<TS, T> Get<T>(Expression<Func<TS, T>> memberSelector)
        {
            return Create(memberSelector);
        }

        public Accessor()
        {

        }

        class GetterSetter<T> : Accessor<TS, T>
        {
            public GetterSetter(Expression<Func<TS, T>> memberSelector) : base(memberSelector)
            {

            }
        }
    }

    public class Accessor<TS, T> : Accessor<TS>
    {
        readonly Func<TS, T> _getter;
        readonly Action<TS, T> _setter;

        public bool IsReadable { get; private set; }
        public bool IsWritable { get; private set; }

        public T this[TS instance]
        {
            get
            {
                if (!IsReadable)
                    throw new ArgumentException("Property get method not found.");

                return _getter(instance);
            }
            set
            {
                if (!IsWritable)
                    throw new ArgumentException("Property set method not found.");

                _setter(instance, value);
            }
        }

        protected Accessor(Expression<Func<TS, T>> memberSelector) //access not given to outside world
        {
            var prop = memberSelector.GetPropertyInfo();
            IsReadable = prop.CanRead;
            IsWritable = prop.CanWrite;
            AssignDelegate(IsReadable, ref _getter, prop.GetGetMethod());
            AssignDelegate(IsWritable, ref _setter, prop.GetSetMethod());
        }

        void AssignDelegate<TK>(bool assignable, ref TK assignee, MethodInfo assignor) where TK : class
        {
            if (assignable)
                assignee = assignor.CreateDelegate<TK>();
        }

    }

    internal static class AccessorExtensions
    {
        // a generic extension for CreateDelegate
        public static T CreateDelegate<T>(this MethodInfo method) where T : class
        {
            return Delegate.CreateDelegate(typeof(T), method) as T;
        }

        public static PropertyInfo GetPropertyInfo<S, T>(this Expression<Func<S, T>> propertySelector)
        {
            var body = propertySelector.Body as MemberExpression;
            if (body == null)
                throw new MissingMemberException("something went wrong");

            return body.Member as PropertyInfo;
        }
    }
}