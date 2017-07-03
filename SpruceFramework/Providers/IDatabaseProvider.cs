// #region Author Information
// // IDatabaseProvider.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion


using System.Data;

namespace SpruceFramework.Providers
{
    public interface IDatabaseProvider
    {
        IDbConnection Connection { get; }

        string ProviderName { get; }

        bool IsDatabaseVersioned(string tableName);

        IDatabaseTableGenerator DatabaseTableGenerator { get; }
    }
}