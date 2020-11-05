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

 * You can release yourself from the requirements of the AGPL license by purchasing
 * a commercial license (dotEntity Pro). Buying such a license is mandatory as soon as you
 * develop commercial activities involving the dotEntity software without
 * disclosing the source code of your own applications. The activites include:
 * shipping dotEntity with a closed source product, offering paid services to customers
 * as an Application Service Provider.
 * To know more about our commercial license email us at support@roastedbytes.com or
 * visit http://dotentity.net/licensing
 */
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DotEntity.Caching;

namespace DotEntity.Extensions
{
    internal static class DataReaderExtensions
    {
        internal static List<DataReaderRow> GetDataReaderRows(this IDataReader dataReader, string[] columnNames, string typeName, DotEntityDbCommand command)
        {
            Throw.IfArgumentNullOrEmpty(typeName, nameof(typeName));
            var dataReaderRows = new List<DataReaderRow>();
            var ordinalsCaptured = ProcessedQueryCache.TryGet(command.Query, out Dictionary<string, int> columnOrdinals);
            while (dataReader.Read())
            {
                if (!ordinalsCaptured)
                {
                    ordinalsCaptured = true;
                    columnOrdinals = columnOrdinals ?? new Dictionary<string, int>();
                    //the microsoft sqlite reader's get ordinal performs case sensitive comparision, we'll have to lower case the columns in the case
                    var doLower = typeName == "sqlite_master" && dataReader.GetType().FullName == "Microsoft.Data.Sqlite.SqliteDataReader";
                    foreach (var c in columnNames)
                    {
                        try
                        {
                            columnOrdinals.Add(typeName + "." + c, dataReader.GetOrdinal(doLower ? c.ToLowerInvariant() : c));
                        }
                        catch(IndexOutOfRangeException ex)
                        {
                            //may be the column name doesn't exist in the result. skip it
                            continue;
                        }
                    }
                    ProcessedQueryCache.TrySet(command.Query, columnOrdinals);
                }
                var row = new DataReaderRow();
                foreach (var c in columnOrdinals)
                {
                    row[c.Key] = dataReader[c.Value];
                }
                dataReaderRows.Add(row);
            }

            return dataReaderRows;
        }

        internal static List<DataReaderRow> GetDataReaderRows(this IDataReader dataReader)
        {
            var dataReaderRows = new List<DataReaderRow>();
            var columnNames = new Dictionary<string, List<int>>(); //stores column name and the indexes at which they appear

            for (var i = 0; i < dataReader.FieldCount; i++)
            {
                var fieldName = dataReader.GetName(i).ReplaceFirst("_", ".");
                if (columnNames.ContainsKey(fieldName))
                    columnNames[fieldName].Add(i);
                else
                {
                    columnNames.Add(fieldName, new List<int>() {i});
                }
            }

            while (dataReader.Read())
            {
                var row = new DataReaderRow();
                foreach (var cDetails in columnNames)
                {
                    var columnName = cDetails.Key;
                    for (var instanceIndex = 0; instanceIndex < cDetails.Value.Count; instanceIndex++)
                    {
                        row[columnName, instanceIndex] = dataReader[cDetails.Value[instanceIndex]] ?? DBNull.Value;
                    }
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
                string[] columnNames = null;
                try
                {
                    columnNames = GetColumnNames(dataReader);
                }
                catch
                {
                    //we'll have to manually do something-----------------------------------------
                }                                                                               //|
                while (dataReader.Read())                                                       //|
                {                                                                               //|
                    var fieldCount = dataReader.FieldCount;                                     //|
                    var row = new DataReaderRow();                                              //|
                    if (columnNames == null)  //<--------------------------------------------------
                    {
                        columnNames = new string[fieldCount];
                        for (var i = 0; i < fieldCount; i++)
                            columnNames[i] = dataReader.GetName(i);
                    }

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
           return null;
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