// #region Author Information
// // QueryProcessor.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DotEntity
{
    internal class QueryProcessor
    {
        public IDbCommand GetQueryCommand(IDbConnection connection, string sqlQuery, IList<QueryInfo> parameters, bool loadIdOfAffectedRow = false, string idParameterName = "", CommandType commandType = CommandType.Text)
        {
            var command = connection.CreateCommand();
            command.CommandText = sqlQuery;
            command.CommandType = commandType;
            if (parameters != null)
            {
                foreach (var parameter in parameters.Where(x => !x.SupportOperator && !x.IsPropertyValueAlsoProperty))
                {
                    var cmdParameter = command.CreateParameter();
                    cmdParameter.ParameterName = parameter.ParameterName;
                    cmdParameter.Value = parameter.PropertyValue;
                    command.Parameters.Add(cmdParameter);
                }
            }
            
            if (loadIdOfAffectedRow)
            {
                //add an output parameter
                var idParameter = command.CreateParameter();
                idParameter.ParameterName = idParameterName;
                idParameter.Direction = ParameterDirection.Output;
                idParameter.DbType = DbType.Int32;
                command.Parameters.Add(idParameter);
            }
            return command;
        }

        internal static QueryProcessor Instance => Singleton<QueryProcessor>.Instance;

    }
}