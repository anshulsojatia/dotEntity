// #region Author Information
// // Relation.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using System;

namespace DotEntity
{
    public class Relation
    {
        public Type SourceType { get; set; }

        public Type DestinationType { get; set; }

        public string SourceColumnName { get; set; }

        public string DestinationColumnName { get; set; }

        public static Relation Create<TSource, TDestination>(string sourceColumnName, string destinationColumnName)
        {
            return new Relation()
            {
                SourceColumnName = sourceColumnName,
                DestinationColumnName = destinationColumnName,
                SourceType = typeof(TSource),
                DestinationType = typeof(TDestination)
            };
          
        }
    }

}