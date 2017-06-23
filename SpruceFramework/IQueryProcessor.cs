// #region Author Information
// // IQueryProcessor.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using System.Collections.Generic;
using System.Data;

namespace SpruceFramework
{
    internal interface IQueryProcessor
    {
        IDbCommand GetQueryCommand(IDbConnection connection, string sqlQuery, IList<QueryParameter> parameters, bool loadIdOfAffectedRow = false);
    }
}