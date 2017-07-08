// #region Author Information
// // IDataDeserializer.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using System;
using System.Collections.Generic;
using System.Data;

namespace DotEntity
{
    public interface IDataDeserializer
    {
        string[] GetColumns();

        string GetKeyColumn();
    }
    public interface IDataDeserializer<out T> : IDataDeserializer where T : class
    {
        T DeserializeSingle(IDataReader reader);

        IEnumerable<T> DeserializeMany(IDataReader reader);

        IEnumerable<T> DeserializeManyNested(IDataReader reader, IList<IJoinMeta> joinMetas, Dictionary<Type, Delegate> relationActions);
    }
}