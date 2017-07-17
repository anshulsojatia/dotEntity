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

 * You can release yourself from the requirements of the license by purchasing
 * a commercial license.Buying such a license is mandatory as soon as you
 * develop commercial software involving the dotEntity software without
 * disclosing the source code of your own applications.
 * To know more about our commercial license email us at support@roastedbytes.com or
 * visit http://dotentity.net/legal/commercial
 */
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