/**
 * Copyright(C) 2017  Apexol Technologies
 * 
 * This file (QueryProcessor.cs) is part of dotEntity(https://github.com/RoastedBytes/dotentity).
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