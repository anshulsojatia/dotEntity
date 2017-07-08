// #region Author Information
// // DotEntityDbConnector.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using System;
using System.Data;
using DotEntity.Enumerations;

namespace DotEntity
{
    internal static class DotEntityDbConnector
    {
        public static void ExecuteCommand(DotEntityDbCommand command)
        {
            ExecuteCommands(new[] {command});
        }

        public static void ExecuteCommands(DotEntityDbCommand[] commands, bool useTransaction = false, IsolationLevel isolationLevel = IsolationLevel.Serializable)
        {
            var queryProcessor = QueryProcessor.Instance;
            using (var con = DotEntityDb.Provider.Connection)
            {
                con.Open();
                using (var trans = useTransaction ? con.BeginTransaction(isolationLevel) : null)
                {
                    foreach (var DotEntityDbCommand in commands)
                    {
                        switch (DotEntityDbCommand.OperationType)
                        {
                            case DbOperationType.Insert:
                            case DbOperationType.SelectScaler:
                                using (var cmd =
                                    queryProcessor.GetQueryCommand(con, DotEntityDbCommand.Query, DotEntityDbCommand.QueryInfos, true, DotEntityDbCommand.KeyColumn))
                                {
                                    cmd.Transaction = trans;
                                    var value = cmd.ExecuteScalar();

                                    DotEntityDbCommand.SetRawResult(value);
                                }
                                break;
                            case DbOperationType.Update:
                            case DbOperationType.Delete:
                            case DbOperationType.Other:    
                                using (var cmd =
                                    queryProcessor.GetQueryCommand(con, DotEntityDbCommand.Query, DotEntityDbCommand.QueryInfos))
                                {
                                    cmd.Transaction = trans;
                                    DotEntityDbCommand.SetRawResult(cmd.ExecuteNonQuery());
                                }
                                break;
                            case DbOperationType.Select:
                                using (var cmd =
                                    queryProcessor.GetQueryCommand(con, DotEntityDbCommand.Query, DotEntityDbCommand.QueryInfos))
                                {
                                    cmd.Transaction = trans;
                                    using (var reader = cmd.ExecuteReader())
                                    {
                                        DotEntityDbCommand.SetDataReader(reader);
                                    }
                                }
                                break;
                            case DbOperationType.MultiQuery:
                                //the difference between a multiquery and select is that in multiquery,
                                //we don't dispose the reader immediately. It's the responsibility of the
                                //reader processor to dispose it manually
                                //todo: Can we have a better solution than this?
                                using (var cmd =
                                    queryProcessor.GetQueryCommand(con, DotEntityDbCommand.Query, DotEntityDbCommand.QueryInfos))
                                {
                                    cmd.Transaction = trans;
                                    var reader = cmd.ExecuteReader();
                                    DotEntityDbCommand.SetDataReader(reader);
                                }
                                break;
                            case DbOperationType.Procedure:
                                using (var cmd =
                                    queryProcessor.GetQueryCommand(con, DotEntityDbCommand.Query, DotEntityDbCommand.QueryInfos, commandType: CommandType.StoredProcedure))
                                {
                                    cmd.Transaction = trans;
                                    using (var reader = cmd.ExecuteReader())
                                    {
                                        DotEntityDbCommand.SetDataReader(reader);
                                    }
                                }
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        if(!DotEntityDbCommand.ContinueNextCommand)
                            trans?.Rollback();
                        
                    }
                    if (trans?.Connection != null)
                        trans.Commit();
                }

            }
        }
    }
}