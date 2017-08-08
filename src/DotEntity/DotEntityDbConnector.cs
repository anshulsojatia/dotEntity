/**
 * Copyright(C) 2017  Apexol Technologies
 * 
 * This file (DotEntityDbConnector.cs) is part of dotEntity(https://github.com/RoastedBytes/dotentity).
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
using System.Data;
using DotEntity.Enumerations;

namespace DotEntity
{
    internal static class DotEntityDbConnector
    {
        public static void ExecuteCommand(DotEntityDbCommand command, IsolationLevel isolationLevel = IsolationLevel.Serializable)
        {
            ExecuteCommands(new[] {command}, isolationLevel);
        }

        public static void ExecuteCommands(DotEntityDbCommand[] commands, IsolationLevel isolationLevel = IsolationLevel.Serializable)
        {
            var queryProcessor = QueryProcessor.Instance;
            using (var con = DotEntityDb.Provider.Connection)
            {
                con.Open();
                foreach (var dotEntityDbCommand in commands)
                {
                    switch (dotEntityDbCommand.OperationType)
                    {
                        case DbOperationType.Insert:
                        case DbOperationType.SelectScaler:
                            using (var cmd =
                                queryProcessor.GetQueryCommand(con, dotEntityDbCommand.Query, dotEntityDbCommand.QueryInfos, true, dotEntityDbCommand.KeyColumn))
                            {
                                var value = cmd.ExecuteScalar();

                                dotEntityDbCommand.SetRawResult(value);
                            }
                            break;
                        case DbOperationType.Update:
                        case DbOperationType.Delete:
                        case DbOperationType.Other:
                            using (var cmd =
                                queryProcessor.GetQueryCommand(con, dotEntityDbCommand.Query, dotEntityDbCommand.QueryInfos))
                            {
                                dotEntityDbCommand.SetRawResult(cmd.ExecuteNonQuery());
                            }
                            break;
                        case DbOperationType.Select:
                        case DbOperationType.MultiQuery:
                            using (var cmd =
                                queryProcessor.GetQueryCommand(con, dotEntityDbCommand.Query, dotEntityDbCommand.QueryInfos))
                            {
                                using (var reader = cmd.ExecuteReader())
                                {
                                    dotEntityDbCommand.SetDataReader(reader);
                                }
                            }
                            break;
                        case DbOperationType.Procedure:
                            using (var cmd =
                                queryProcessor.GetQueryCommand(con, dotEntityDbCommand.Query, dotEntityDbCommand.QueryInfos, commandType: CommandType.StoredProcedure))
                            {
                                using (var reader = cmd.ExecuteReader())
                                {
                                    dotEntityDbCommand.SetDataReader(reader);
                                }
                            }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

            }
        }
    }
}