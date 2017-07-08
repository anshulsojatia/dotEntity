// #region Author Information
// // Instantiator.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using System;
using System.Linq.Expressions;

namespace DotEntity.Reflection
{
    public static class Instantiator
    {
        public static object GetInstance(Type type)
        {
            return ((Delegate) GenericInvoker.InvokeField(null, typeof(Instantiator<>), type, "Instance")).DynamicInvoke();
        }
    }
    public static class Instantiator<T>
    {
        private static readonly Type TypeOfT = typeof(T);
     
        public static readonly Func<T> Instance = Expression.Lambda<Func<T>>
        (
            Expression.New(TypeOfT)
        ).Compile();

        /// <summary>
        /// Returns specified number of instances real and quick
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public static T[] Instances(int count)
        {
            var tArray = new T[count];
            for (var i = 0; i < count; i++)
                tArray[i] = Instance();

            return tArray;
        }
    }
}