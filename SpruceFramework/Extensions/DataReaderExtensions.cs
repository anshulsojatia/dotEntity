// #region Author Information
// // DataReaderExtensions.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using System;
using System.Collections.Generic;
using System.Data;

namespace SpruceFramework.Extensions
{
    public static class DataReaderExtensions
    {
        public static DataReaderRow[] GetDataReaderRows(this IDataReader dataReader, string[] columnNames)
        {
            var dataReaderRows = new List<DataReaderRow>();
            while (dataReader.Read())
            {
                var row = new DataReaderRow();
                foreach (var c in columnNames)
                {
                    row[c] = dataReader[c];
                }

                dataReaderRows.Add(row);
            }

            return dataReaderRows.ToArray();
        }

        public static DataTable GetDataTable(this IDataReader dataReader)
        {
            var schemaTable = dataReader.GetSchemaTable();
            var resultTable = new DataTable();

            foreach (DataRow dataRow in schemaTable.Rows)
            {
                var dataColumn = new DataColumn
                {
                    ColumnName = dataRow["ColumnName"].ToString(),
                    DataType = Type.GetType(dataRow["DataType"].ToString()),
                    ReadOnly = (bool) dataRow["IsReadOnly"],
                    AutoIncrement = (bool) dataRow["IsAutoIncrement"],
                    Unique = (bool) dataRow["IsUnique"]
                };

                resultTable.Columns.Add(dataColumn);
            }

            while (dataReader.Read())
            {
                var dataRow = resultTable.NewRow();
                for (var i = 0; i < resultTable.Columns.Count; i++)
                {
                    dataRow[i] = dataReader[i];
                }
                resultTable.Rows.Add(dataRow);
            }

            return resultTable;
        }
    }
}