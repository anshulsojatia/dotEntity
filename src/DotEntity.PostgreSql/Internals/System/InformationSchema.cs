// #region Author Information
// // InformationSchema.cs
// // 
// // (c) Sojatia Infocrafts Private Limited. All Rights Reserved.
// // 
// #endregion
namespace DotEntity.PostgreSql.Internals.System
{
    internal static class InformationSchema
    {
        public class Tables
        {
            public string table_catalog { get; set; }

            public string table_schema { get; set; }

            public string table_name { get; set; }

            public string table_type { get; set; }
        }

        public class Columns
        {
            public string table_catalog { get; set; }

            public string table_schema { get; set; }

            public string table_name { get; set; }

            public string column_name { get; set; }

            public string ordinal_position { get; set; }

            public string column_default { get; set; }

            public string is_nullable { get; set; }

            public string data_type { get; set; }

            public string character_maximum_length { get; set; }
        }
    }
}