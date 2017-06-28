// #region Author Information
// // SpruceDbConnector.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using System;
using System.Data;
using SpruceFramework.Enumerations;

namespace SpruceFramework
{
    internal static class SpruceDbConnector
    {
        public static void ExecuteCommand(SpruceDbCommand command)
        {
            ExecuteCommands(new[] {command});
        }

        public static void ExecuteCommands(SpruceDbCommand[] commands, bool useTransaction = false, IsolationLevel isolationLevel = IsolationLevel.Serializable)
        {
            var queryProcessor = QueryProcessor.Instance;
            using (var con = Spruce.Provider.Connection)
            {
                con.Open();
                using (var trans = useTransaction ? con.BeginTransaction(isolationLevel) : null)
                {
                    foreach (var spruceDbCommand in commands)
                    {
                        switch (spruceDbCommand.OperationType)
                        {
                            case DbOperationType.Insert:
                            case DbOperationType.SelectSingle:
                                using (var cmd =
                                    queryProcessor.GetQueryCommand(con, spruceDbCommand.Query, spruceDbCommand.QueryParameters, true, spruceDbCommand.KeyColumn))
                                {
                                    var value = cmd.ExecuteScalar();

                                    spruceDbCommand.SetRawResult(value);
                                }
                                break;
                            case DbOperationType.Update:
                            case DbOperationType.Delete:
                                using (var cmd =
                                    queryProcessor.GetQueryCommand(con, spruceDbCommand.Query, spruceDbCommand.QueryParameters))
                                {
                                    spruceDbCommand.SetRawResult(cmd.ExecuteNonQuery());
                                }
                                break;
                            case DbOperationType.Select:
                                using (var cmd =
                                    queryProcessor.GetQueryCommand(con, spruceDbCommand.Query, spruceDbCommand.QueryParameters))
                                {
                                    using (var reader = cmd.ExecuteReader())
                                    {
                                        spruceDbCommand.SetDataReader(reader);
                                    }
                                }
                                break;
                            case DbOperationType.Procedure:
                                using (var cmd =
                                    queryProcessor.GetQueryCommand(con, spruceDbCommand.Query, spruceDbCommand.QueryParameters, commandType: CommandType.StoredProcedure))
                                {
                                    using (var reader = cmd.ExecuteReader())
                                    {
                                        spruceDbCommand.SetDataReader(reader);
                                    }
                                }
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        if(!spruceDbCommand.ContinueNextCommand)
                            trans?.Rollback();
                        
                    }
                    trans?.Commit();
                }

            }
        }
    }
}