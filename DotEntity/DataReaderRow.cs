// #region Author Information
// // DataReaderRow.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

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