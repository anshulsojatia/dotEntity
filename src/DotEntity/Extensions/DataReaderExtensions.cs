/**
 * Copyright(C) 2017  Apexol Technologies
 * 
 * This file (DataReaderExtensions.cs) is part of dotEntity(https://github.com/RoastedBytes/dotentity).
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

 * You can release yourself from the requirements of the license by purchasing
 * a commercial license.Buying such a license is mandatory as soon as you
 * develop commercial software involving the dotEntity software without
 * disclosing the source code of your own applications.
 * To know more about our commercial license email us at support@roastedbytes.com or
 * visit http://dotentity.net/legal/commercial
 */
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DotEntity.Extensions
{
    internal static class DataReaderExtensions
    {
        internal static List<DataReaderRow> GetDataReaderRows(this IDataReader dataReader, string[] columnNames, string typeName)
        {
            Throw.IfArgumentNullOrEmpty(typeName, nameof(typeName));
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
#if !NETSTANDARD15
            var schemaTable = dataReader.GetSchemaTable();
            var resultColumns = new string[schemaTable.Rows.Count];
            var i = 0;
            foreach (DataRow dataRow in schemaTable.Rows)
            {
                resultColumns[i++] = dataRow["ColumnName"].ToString();
            }
            return resultColumns;
#else
            var arr = new string[dataReader.FieldCount];
            for (var i = 0; i < arr.Length; i++)
                arr[i] = $"::{i + 1}";
            return arr;
#endif

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