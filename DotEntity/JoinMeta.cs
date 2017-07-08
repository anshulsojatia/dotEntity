// #region Author Information
// // JoinMeta.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using System;
using DotEntity.Enumerations;

namespace DotEntity
{
    public enum SourceColumn
    {
        Parent,
        Chained
    }
    public interface IJoinMeta
    {
        string SourceColumnName { get; set; }

        string DestinationColumnName { get; set; }

        Type OnType { get; }

        SourceColumn SourceColumn { get; set; }

        JoinType JoinType { get; set; }
    }

    public class JoinMeta<T> : IJoinMeta
    {
        public JoinMeta(string sourceColumnName, string destinationColumnName, SourceColumn sourceColumn = SourceColumn.Chained, JoinType joinType = JoinType.Inner)
        {
            SourceColumnName = sourceColumnName;
            DestinationColumnName = destinationColumnName;
            SourceColumn = sourceColumn;
            JoinType = joinType;
        }
        public string SourceColumnName { get; set; }
        public string DestinationColumnName { get; set; }
        public Type OnType => typeof(T);
        public SourceColumn SourceColumn { get; set; }
        public JoinType JoinType { get; set; }
    }

}