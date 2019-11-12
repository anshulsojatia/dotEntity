using System;
using System.Collections.Generic;

namespace DotEntity.Sqlite
{
    public class SqliteTypeMapProvider : ITypeMapProvider
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
                    { typeof(byte), "INTEGER" },
                    { typeof(byte?), "INTEGER" },
                    { typeof(short), "INTEGER"},
                    { typeof(short?), "INTEGER"},
                    { typeof(int) , "INTEGER" },
                    { typeof(int?) , "INTEGER" },
                    { typeof(uint) , "INTEGER" },
                    { typeof(long), "INTEGER"},
                    { typeof(long?), "INTEGER"},
                    { typeof(ulong), "INTEGER" },
                    { typeof(string), "TEXT" },
                    { typeof(DateTime), "TEXT" },
                    { typeof(DateTime?), "TEXT" },
                    { typeof(double), "REAL" },
                    { typeof(double?), "REAL" },
                    { typeof(decimal), "NUMERIC" },
                    { typeof(decimal?), "NUMERIC" },
                    { typeof(bool), "NUMERIC" },
                    { typeof(bool?), "NUMERIC" },
                    { typeof(byte[]), "BLOB" },
                    { typeof(char), "TEXT"},
                    { typeof(char?), "TEXT"},
                    { typeof(Enum), "INT" },
                    { typeof(Guid), "TEXT" }
                };
                return _typeMap;
            }
        }
    }
}