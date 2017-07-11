// #region Author Information
// // PropertyAccessor.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using System;
using System.Linq.Expressions;
using System.Reflection;

namespace DotEntity.Reflection
{
    internal static class PropertyAccessor
    {
        internal static Action<T, TClass> CreateSetter<T, TClass>(this PropertyInfo propertyInfo)
        {
            return (Action<T, TClass>) propertyInfo.GetSetMethod().CreateDelegate(typeof(Action<T, TClass>));
        }

        internal static Func<T, TType> CreateGetter<T, TType>(this PropertyInfo propertyInfo)
        {
            return (Func<T, TType>) propertyInfo.GetGetMethod().CreateDelegate(typeof(Func<T, TType>));
        }

        internal static Delegate CreateSetter<TType>(this PropertyInfo propertyInfo, Type objType)
        {
            var genericType = typeof(Action<,>).MakeGenericType(objType, typeof(TType));
            return propertyInfo.GetSetMethod().CreateDelegate(genericType);
        }

        internal static Delegate CreateGetter<TType>(this PropertyInfo propertyInfo, Type objType)
        {
            var genericType = typeof(Func<,>).MakeGenericType(objType, typeof(TType));
            return propertyInfo.GetGetMethod().CreateDelegate(genericType);
        }

    }
}