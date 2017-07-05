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
    internal static class DataReaderExtensions
    {
        internal static List<DataReaderRow> GetDataReaderRows(this IDataReader dataReader, string[] columnNames, string typeName)
        {
            if(string.IsNullOrEmpty(typeName))
                throw new Exception("Must provide a valid type name");

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
                    row[c] = dataReader[i] ?? DBNull.Value;
                }
                dataReaderRows.Add(row);
            }

            return dataReaderRows;
        }

        internal static List<List<DataReaderRow>> GetRawDataReaderRows(this IDataReader dataReader)
        {
            var dataReaderRows = new List<List<DataReaderRow>>();
            var hasResultSet = true;
            var resultIndex = 0;
            while (hasResultSet)
            {
                var newList = new List<DataReaderRow>();
                var columnNames = GetColumnNames(dataReader);
                while (dataReader.Read())
                {
                    var row = new DataReaderRow();
                    foreach (var c in columnNames)
                    {
                        row[c] = dataReader[c];
                    }

                    newList.Add(row);
                }
                dataReaderRows.Add(newList);
                hasResultSet = dataReader.NextResult();
            }
            return dataReaderRows;
        }

        public static string[] GetColumnNames(this IDataReader dataReader)
        {
            var schemaTable = dataReader.GetSchemaTable();
            var resultColumns = new string[schemaTable.Rows.Count];
            var i = 0;
            foreach (DataRow dataRow in schemaTable.Rows)
            {
                resultColumns[i++] = dataRow["ColumnName"].ToString();
            }
            return resultColumns;
        }

        public static void PrefixTypeName(this List<DataReaderRow> rows, string typeName)
        {
            if (rows == null || rows.Count == 0)
                return;
            var columns = rows[0].Columns;
            foreach (var row in rows)
            {
                foreach (var column in columns)
                {
                    row.RenameColumn(column, typeName + "." + column);
                }
            }
        }
    }
}