/**
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

 * You can release yourself from the requirements of the AGPL license by purchasing
 * a commercial license (dotEntity or dotEntity Pro). Buying such a license is mandatory as soon as you
 * develop commercial activities involving the dotEntity software without
 * disclosing the source code of your own applications. The activites include:
 * shipping dotEntity with a closed source product, offering paid services to customers
 * as an Application Service Provider.
 * To know more about our commercial license email us at support@roastedbytes.com or
 * visit http://dotentity.net/licensing
 */
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotEntity
{
    public class DataReaderRow
    {
        private Dictionary<string, object> RowInformation { get; }

        public DataReaderRow()
        {
            RowInformation = new Dictionary<string, object>();
        }

        public string[] Columns => RowInformation.Keys.ToArray();

        public object this[string columnName]
        {
            get
            {
                RowInformation.TryGetValue(columnName, out object value);
                return value;
            }
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
            foreach(var columnName in columnNames)
                if (!row1[columnName].Equals(row2[columnName]))
                    return false;

            return true;
        }

        public static bool AreAllColumnsNull(DataReaderRow row, string[] columnNames, int skipColumns)
        {
            foreach (var columnName in columnNames)
                if (row[columnName] != DBNull.Value)
                    return false;

            return true;
        }
    }
}