/**
 * Copyright(C) 2017-2021  Sojatia Infocrafts Private Limited
 * 
 * This file (GenericInvoker.cs) is part of dotEntity(https://github.com/RoastedBytes/dotentity).
 * 
 * dotEntity is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 
 * dotEntity is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
 * GNU Affero General Public License for more details.
 
 * You should have received a copy of the GNU Affero General Public License
 * along with dotEntity.If not, see<http://www.gnu.org/licenses/>.

 * You can release yourself from the requirements of the AGPL license by purchasing
 * a commercial license (dotEntity Pro). Buying such a license is mandatory as soon as you
 * develop commercial activities involving the dotEntity software without
 * disclosing the source code of your own applications. The activites include:
 * shipping dotEntity with a closed source product, offering paid services to customers
 * as an Application Service Provider.
 * To know more about our commercial license email us at support@roastedbytes.com or
 * visit http://dotentity.net/licensing
 */
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace DotEntity.Reflection
{
    public class GenericInvoker
    {
        private static readonly ConcurrentDictionary<Type, MethodInfo> InvokeMethods = new ConcurrentDictionary<Type, MethodInfo>();
        private static readonly ConcurrentDictionary<Type, PropertyInfo> InvokeProperties = new ConcurrentDictionary<Type, PropertyInfo>();
        private static readonly ConcurrentDictionary<Type, FieldInfo> InvokeFields = new ConcurrentDictionary<Type, FieldInfo>();

        public static object Invoke(object instance, Type baseType,  Type genericType, string methodName, params object[] parameters)
        {
            if (InvokeMethods.TryGetValue(genericType, out MethodInfo method))
                return method.Invoke(instance, parameters);

            var genericTypeInstance = baseType.MakeGenericType(genericType);
            method = genericTypeInstance.GetMethod(methodName);
            InvokeMethods.TryAdd(genericType, method);
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

            Throw.IfObjectNull(property, propertyName);

            InvokeProperties.TryAdd(genericType, property);
            return property.GetValue(instance, index);
        }

        public static object InvokeField(object instance, Type baseType, Type genericType, string fieldName)
        {
            if (InvokeFields.TryGetValue(genericType, out FieldInfo fieldInfo))
                return fieldInfo.GetValue(instance);

            var genericTypeInstance = baseType.MakeGenericType(genericType);
            fieldInfo = genericTypeInstance.GetField(fieldName);

            Throw.IfObjectNull(fieldInfo, fieldName);

            InvokeFields.TryAdd(genericType, fieldInfo);
            return fieldInfo.GetValue(instance);
        }

    }
}