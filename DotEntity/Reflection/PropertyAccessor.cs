/**
 * Copyright(C) 2017  Apexol Technologies
 * 
 * This file (PropertyAccessor.cs) is part of dotEntity(https://github.com/RoastedBytes/dotentity).
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

 * You can release yourself from the requirements of the license by purchasing
 * a commercial license.Buying such a license is mandatory as soon as you
 * develop commercial software involving the dotEntity software without
 * disclosing the source code of your own applications.
 * To know more about our commercial license email us at support@roastedbytes.com or
 * visit http://dotentity.net/legal/commercial
 */
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