// #region Author Information
// // DataReaderRow.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using System.Collections.Generic;

namespace SpruceFramework
{
    public class DataReaderRow
    {
        private Dictionary<string, object> RowInformation { get; }
        
        public DataReaderRow()
        {
            RowInformation = new Dictionary<string, object>();
        }

        public object this[string columnName]
        {
            get => RowInformation[columnName];
            set => RowInformation[columnName] = value;
        }
    }
}