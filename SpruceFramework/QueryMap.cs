// #region Author Information
// // QueryMap.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using System.Collections.Generic;

namespace SpruceFramework
{
    internal class QueryMap
    {
        public string Query { get; set; }

        public List<QueryParameter> Parameters { get; set; }
    }
}