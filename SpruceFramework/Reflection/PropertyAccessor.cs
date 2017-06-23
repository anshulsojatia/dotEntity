// #region Author Information
// // PropertyAccessor.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using System;
using System.Reflection;

namespace SpruceFramework.Reflection
{
    public static class PropertyAccessor
    {
        internal static Action<T, TClass> CreateSetter<T, TClass>(this PropertyInfo propertyInfo)
        {
            return (Action<T, TClass>) Delegate.CreateDelegate(typeof(Action<T, TClass>), propertyInfo.GetSetMethod());
        }
    }
}