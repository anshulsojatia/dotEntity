// #region Author Information
// // IDataDeserializer.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using System.Data;

namespace SpruceFramework
{
    public interface IDataDeserializer
    {
        string[] GetColumns();

        string GetKeyColumn();
    }
    public interface IDataDeserializer<out T> : IDataDeserializer where T : class
    {
        T DeserializeSingle(IDataReader reader);

        T[] DeserializeMany(IDataReader reader);


    }
}