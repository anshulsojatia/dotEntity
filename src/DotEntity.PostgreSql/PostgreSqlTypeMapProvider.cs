using System;
using System.Collections.Generic;

namespace DotEntity.PostgreSql
{
    public class PostgreSqlTypeMapProvider : ITypeMapProvider
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
                    { typeof(byte), "SMALLINT" },
                    { typeof(byte?), "SMALLINT" },
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
                    { typeof(DateTime), "TIMESTAMP" },
                    { typeof(DateTime?), "TIMESTAMP" },
                    { typeof(double), "FLOAT8" },
                    { typeof(double?), "FLOAT8" },
                    { typeof(decimal), "NUMERIC(18,5)" },
                    { typeof(decimal?), "NUMERIC(18,5)" },
                    { typeof(float), "FLOAT8" },
                    { typeof(float?), "FLOAT8" },
                    { typeof(bool), "BOOLEAN" },
                    { typeof(bool?), "BOOLEAN" },
                    { typeof(byte[]), "BYTEA" },
                    { typeof(char), "VARCHAR(1)"},
                    { typeof(char?), "VARCHAR(1)"},
                    { typeof(Enum), "INT" },
                    { typeof(Guid), "UUID" }
                };
                return _typeMap;
            }
        }
    }
}