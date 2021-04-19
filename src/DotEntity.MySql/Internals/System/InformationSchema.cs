// #region Author Information
// // InformationSchema.cs
// // 
// // (c) Sojatia Infocrafts Private Limited. All Rights Reserved.
// // 
// #endregion
namespace DotEntity.MySql.Internals.System
{
    internal static class InformationSchema
    {
        public class Tables
        {
            public string TABLE_CATALOG { get; set; }

            public string TABLE_SCHEMA { get; set; }

            public string TABLE_NAME { get; set; }

            public string TABLE_TYPE { get; set; }
        }

        public class Columns
        {
            public string TABLE_CATALOG { get; set; }

            public string TABLE_SCHEMA { get; set; }

            public string TABLE_NAME { get; set; }

            public string COLUMN_NAME { get; set; }

            public string ORDINAL_POSITION { get; set; }

            public string COLUMN_DEFAULT { get; set; }

            public string IS_NULLABLE { get; set; }

            public string DATA_TYPE { get; set; }

            public string CHARACTER_MAXIMUM_LENGTH { get; set; }
        }
    }
}