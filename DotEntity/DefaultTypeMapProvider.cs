// #region Author Information
// // DefaultTypeMapProvider.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

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
                    { typeof(decimal), "NUMERIC(18,0)" },
                    { typeof(decimal?), "NUMERIC(18,0)" },
                    { typeof(bool), "BIT" },
                    { typeof(bool?), "BIT" },
                    { typeof(byte[]), "VARBINARY" },
                    { typeof(char), "NVARCHAR(1)"},
                    { typeof(char?), "NVARCHAR(1)"},
                    { typeof(Guid), "UNIQUEIDENTIFIER" },
                    { typeof(Guid?), "UNIQUEIDENTIFIER" }
                };
                return _typeMap;
            }
        }
    }
}