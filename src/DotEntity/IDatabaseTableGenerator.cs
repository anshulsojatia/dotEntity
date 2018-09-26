/**
 * Copyright(C) 2017  Apexol Technologies
 * 
 * This file (IDatabaseTableGenerator.cs) is part of dotEntity(https://github.com/RoastedBytes/dotentity).
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
using System.Reflection;

namespace DotEntity
{
    public interface IDatabaseTableGenerator
    {
        string GetFormattedDbTypeForType(Type type, PropertyInfo propertyInfo = null);

        string GetCreateTableScript<T>();

        string GetCreateTableScript(Type type);

        string GetDropTableScript<T>();

        string GetDropTableScript(Type type);

        string GetDropTableScript(string tableName);

        string GetCreateConstraintScript(Relation relation);

        string GetDropConstraintScript(Relation relation);

        string GetAddColumnScript<T, T1>(string columnName, T1 value, PropertyInfo propertyInfo = null);

        string GetDropColumnScript(Type type, string columnName);

        string GetAlterColumnScript(Type type, string columnName, Type columnType, PropertyInfo propertyInfo = null);
    }
}