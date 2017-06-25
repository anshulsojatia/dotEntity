// #region Author Information
// // DataReaderExtensions.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SpruceFramework.Extensions
{
    public static class DataReaderExtensions
    {
        public static List<DataReaderRow> GetDataReaderRows(this IDataReader dataReader, string[] columnNames, string typeName)
        {
            var dataReaderRows = new List<DataReaderRow>();
            while (dataReader.Read())
            {
                var row = new DataReaderRow();
                foreach (var c in columnNames)
                {
                    row[typeName + "." + c] = dataReader[c];
                }

                dataReaderRows.Add(row);
            }

            return dataReaderRows;
        }

        public static List<DataReaderRow> GetDataReaderRows(this IDataReader dataReader, Dictionary<Type, int> columnsWithSkipCount)
        {
            var dataReaderRows = new List<DataReaderRow>();
            var columnNames = new List<string>();

            var breakPointIndexes = columnsWithSkipCount.Values;
            var activeTypeName = columnsWithSkipCount.Keys.First().Name;

            for (var i = 0; i < dataReader.FieldCount; i++)
            {
                columnNames.Add(activeTypeName + "." + dataReader.GetName(i));
                if (breakPointIndexes.Contains(i + 1))
                {
                    activeTypeName = columnsWithSkipCount.First(x => x.Value == i + 1).Key.Name;
                }
            }

            while (dataReader.Read())
            {
                var row = new DataReaderRow();

                for (var i = 0; i < columnNames.Count; i++)
                {
                    var c = columnNames[i];
                    row[c] = dataReader[i];
                }
                dataReaderRows.Add(row);
            }

            return dataReaderRows;
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