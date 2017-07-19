/**
 * Copyright(C) 2017  Apexol Technologies
 * 
 * This file (Instantiator.cs) is part of dotEntity(https://github.com/RoastedBytes/dotentity).
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
 * a commercial license. Buying such a license is mandatory as soon as you
 * develop commercial activities involving the dotEntity software without
 * disclosing the source code of your own applications. The activites include:
 * shipping dotEntity with a closed source product, offering paid services to customers
 * as an Application Service Provider.
 * To know more about our commercial license email us at support@roastedbytes.com or
 * visit http://dotentity.net/legal/commercial
 */
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