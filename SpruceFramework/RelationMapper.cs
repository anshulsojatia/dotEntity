// #region Author Information
// // RelationMapper.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using System;
using System.Collections.Concurrent;
using SpruceFramework.Enumerations;

namespace SpruceFramework
{
    internal static class RelationMapper
    {
        internal static ConcurrentBag<Relation> Relations;

        static RelationMapper()
        {
            Relations = new ConcurrentBag<Relation>();
        }

        public static void Relate<TSource, TTarget>(string sourceColumnName, string destinationColumnName)
        {
            Relations.Add(new Relation()
            {
                SourceColumnName = sourceColumnName,
                DestinationColumnName = destinationColumnName,
                SourceType = typeof(TSource),
                DestinationType = typeof(TTarget),
            });
        }
    }
}