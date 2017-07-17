﻿/**
 * Copyright(C) 2017  Apexol Technologies
 * 
 * This file (DataReaderRow.cs) is part of dotEntity(https://github.com/RoastedBytes/dotentity).
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
using System.Linq;

namespace DotEntity
{
    public class DataReaderRow
    {
        private Dictionary<string, object> RowInformation { get; }

        private bool _allowDuplicateColumns;

        public DataReaderRow(bool allowDuplicateColumns = false)
        {
            RowInformation = new Dictionary<string, object>();
            _allowDuplicateColumns = allowDuplicateColumns;
        }

        public string[] Columns => RowInformation.Keys.ToArray();

        public object this[string columnName]
        {
            get => RowInformation[columnName];
            set => RowInformation[columnName] = value;
        }

        public object this[int columnIndex] => RowInformation[Columns[columnIndex]];

        public void RenameColumn(string columnName, string newColumnName)
        {
            if (columnName == newColumnName)
                return;

            RowInformation[newColumnName] = RowInformation[columnName];
            RowInformation.Remove(columnName);
        }

        public static bool AreSameRowsForColumns(DataReaderRow row1, DataReaderRow row2, string[] columnNames, int skipColumns)
        {
            if (row1 == null || row2 == null)
                return false;
            var maxLoopValue = skipColumns + columnNames.Length;
            for (var i = skipColumns; i < maxLoopValue; i++)
            {
                var columnName = row1.Columns[i];
                if (row1[columnName].ToString() != row2[columnName].ToString())
                    return false;

            }
          
            return true;
        }

        public static bool AreAllColumnsNull(DataReaderRow row, string[] columnNames, int skipColumns)
        {
            var maxLoopValue = skipColumns + columnNames.Length;
            for (var i = skipColumns; i < maxLoopValue; i++)
            {
                var columnName = row.Columns[i];
                if (row[columnName] != DBNull.Value)
                    return false;

            }
            return true;
        }
    }
}