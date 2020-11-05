/**
 * Copyright(C) 2017  Apexol Technologies
 * 
 * This file (DefaultTypeMapProvider.cs) is part of dotEntity(https://github.com/RoastedBytes/dotentity).
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
using System.Collections.Generic;

namespace DotEntity
{
    public class DefaultTypeMapProvider : ITypeMapProvider
    {
        private static Dictionary<Type, string> _typeMap;

        public Dictionary<Type, string> TypeMap
        {
            get
            {
                if (_typeMap != null)
                    return _typeMap;
                _typeMap = new Dictionary<Type, string>
                {
                    { typeof(byte), "TINYINT" },
                    { typeof(byte?), "TINYINT" },
                    { typeof(sbyte), "" },
                    { typeof(short), "SMALLINT"},
                    { typeof(short?), "SMALLINT"},
                    { typeof(int) , "INT" },
                    { typeof(int?) , "INT" },
                    { typeof(uint) , "" },
                    { typeof(long), "BIGINT"},
                    { typeof(long?), "BIGINT"},
                    { typeof(ulong), "" },
                    { typeof(string), "NVARCHAR" },
                    { typeof(DateTime), "DATETIME" },
                    { typeof(DateTime?), "DATETIME" },
                    { typeof(double), "FLOAT" },
                    { typeof(double?), "FLOAT" },
                    { typeof(decimal), "NUMERIC(18,5)" },
                    { typeof(decimal?), "NUMERIC(18,5)" },
                    { typeof(float), "NUMERIC(18,5)" },
                    { typeof(float?), "NUMERIC(18,5)" },
                    { typeof(bool), "BIT" },
                    { typeof(bool?), "BIT" },
                    { typeof(byte[]), "VARBINARY(MAX)" },
                    { typeof(char), "NVARCHAR(1)"},
                    { typeof(char?), "NVARCHAR(1)"},
                    { typeof(Guid), "UNIQUEIDENTIFIER" },
                    { typeof(Guid?), "UNIQUEIDENTIFIER" },
                    { typeof(Enum), "INT" }
                };
                return _typeMap;
            }
        }
    }
}