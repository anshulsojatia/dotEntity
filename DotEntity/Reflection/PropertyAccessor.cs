// #region Author Information
// // PropertyAccessor.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using System;
using System.Reflection;

namespace DotEntity.Reflection
{
    public static class PropertyAccessor
    {
        internal static Action<T, TClass> CreateSetter<T, TClass>(this PropertyInfo propertyInfo)
        {
            return (Action<T, TClass>) propertyInfo.GetSetMethod().CreateDelegate(typeof(Action<T, TClass>));
        }

        internal static Func<T, TType> CreateGetter<T, TType>(this PropertyInfo propertyInfo)
        {
            return (Func<T, TType>) propertyInfo.GetGetMethod().CreateDelegate(typeof(Func<T, TType>));
        }
    }
}