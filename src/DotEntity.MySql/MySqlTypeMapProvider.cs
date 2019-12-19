using System;
using System.Collections.Generic;

namespace DotEntity.MySql
{
    public class MySqlTypeMapProvider : ITypeMapProvider
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
                    { typeof(string), "TEXT" },
                    { typeof(DateTime), "DATETIME" },
                    { typeof(DateTime?), "DATETIME" },
                    { typeof(double), "FLOAT" },
                    { typeof(double?), "FLOAT" },
                    { typeof(decimal), "NUMERIC(18,5)" },
                    { typeof(decimal?), "NUMERIC(18,5)" },
                    { typeof(bool), "TINYINT(1)" },
                    { typeof(bool?), "TINYINT(1)" },
                    { typeof(byte[]), "BLOB" },
                    { typeof(char), "VARCHAR(1)"},
                    { typeof(char?), "VARCHAR(1)"},
                    { typeof(Enum), "INT" },
                    { typeof(Guid), "CHAR(36)" }
                };
                return _typeMap;
            }
        }
    }
}