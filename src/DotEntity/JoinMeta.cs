/**
 * Copyright(C) 2017  Apexol Technologies
 * 
 * This file (JoinMeta.cs) is part of dotEntity(https://github.com/RoastedBytes/dotentity).
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
 * a commercial license. Buying such a license is mandatory as soon as you
 * develop commercial activities involving the dotEntity software without
 * disclosing the source code of your own applications. The activites include:
 * shipping dotEntity with a closed source product, offering paid services to customers
 * as an Application Service Provider.
 * To know more about our commercial license email us at support@roastedbytes.com or
 * visit http://dotentity.net/legal/commercial
 */
using System;
using DotEntity.Enumerations;

namespace DotEntity
{
    /// <summary>
    /// Specifies the source column type in a join operation
    /// </summary>
    public enum SourceColumn
    {
        /// <summary>
        /// The source column in context belongs to the parent repository
        /// </summary>
        Parent,
        /// <summary>
        /// The source column in context belongs to the previous repository in chained operation
        /// </summary>
        Chained
    }

    /// <summary>
    /// Specifies a join definition for querying the database
    /// </summary>
    public interface IJoinMeta
    {
        string SourceColumnName { get; set; }

        string DestinationColumnName { get; set; }

        Type OnType { get; }

        SourceColumn SourceColumn { get; set; }

        JoinType JoinType { get; set; }
    }

    /// <summary>
    /// Specifies a join definition to the target type <typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T">The entity class that maps to a specific database table</typeparam>
    public class JoinMeta<T> : IJoinMeta
    {
        /// <summary>
        /// Creates a new instance of <see cref="JoinMeta{T}"/>
        /// </summary>
        /// <param name="sourceColumnName">The column name of the source table</param>
        /// <param name="destinationColumnName">The column name of the destination table</param>
        /// <param name="sourceColumnType">(optional) The <see cref="SourceColumn"/> type in this join. Default is <see cref="SourceColumn.Chained"/></param>
        /// <param name="joinType">(optional) The <see cref="JoinType"/> of this join. Default is <see cref="JoinType.Inner"/></param>
        public JoinMeta(string sourceColumnName, string destinationColumnName, SourceColumn sourceColumnType = SourceColumn.Chained, JoinType joinType = JoinType.Inner)
        {
            SourceColumnName = sourceColumnName;
            DestinationColumnName = destinationColumnName;
            SourceColumn = sourceColumnType;
            JoinType = joinType;
        }
        public string SourceColumnName { get; set; }
        public string DestinationColumnName { get; set; }
        public Type OnType => typeof(T);
        public SourceColumn SourceColumn { get; set; }
        public JoinType JoinType { get; set; }
    }

}