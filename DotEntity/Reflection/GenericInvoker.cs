// #region Author Information
// // GenericMethodInvoker.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using System;
using System.Collections.Generic;
using System.Reflection;

namespace DotEntity.Reflection
{
    public class GenericInvoker
    {
        private static readonly Dictionary<Type, MethodInfo> InvokeMethods = new Dictionary<Type, MethodInfo>();
        private static readonly Dictionary<Type, PropertyInfo> InvokeProperties = new Dictionary<Type, PropertyInfo>();
        private static readonly Dictionary<Type, FieldInfo> InvokeFields = new Dictionary<Type, FieldInfo>();

        public static object Invoke(object instance, Type baseType,  Type genericType, string methodName, params object[] parameters)
        {
            if (InvokeMethods.TryGetValue(genericType, out MethodInfo method))
                return method.Invoke(instance, parameters);

            var genericTypeInstance = baseType.MakeGenericType(genericType);
            method = genericTypeInstance.GetMethod(methodName);
            InvokeMethods.Add(genericType, method);
            return method.Invoke(instance, parameters);
        }

        public static object Invoke(object instance, string methodName, params object[] parameters)
        {
            var method = instance.GetType().GetMethod(methodName);
            return method.Invoke(instance, parameters);
        }

        public static object InvokeProperty(object instance, Type baseType, Type genericType, string propertyName, params object[] index)
        {
            if (InvokeProperties.TryGetValue(genericType, out PropertyInfo property))
                return property.GetValue(instance, index);

            var genericTypeInstance = baseType.MakeGenericType(genericType);
            property = genericTypeInstance.GetProperty(propertyName);
            if (property == null)
            {
                throw new Exception("Can't find property with name");
            }
            InvokeProperties.Add(genericType, property);
            return property.GetValue(instance, index);
        }

        public static object InvokeField(object instance, Type baseType, Type genericType, string fieldName)
        {
            if (InvokeFields.TryGetValue(genericType, out FieldInfo fieldInfo))
                return fieldInfo.GetValue(instance);

            var genericTypeInstance = baseType.MakeGenericType(genericType);
            fieldInfo = genericTypeInstance.GetField(fieldName);
            if (fieldInfo == null)
            {
                throw new Exception("Can't find field with name");
            }
            InvokeFields.Add(genericType, fieldInfo);
            return fieldInfo.GetValue(instance);
        }

    }
}