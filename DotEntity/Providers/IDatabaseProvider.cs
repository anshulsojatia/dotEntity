// #region Author Information
// // IDatabaseProvider.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion


using System.Data;

namespace DotEntity.Providers
{
    public interface IDatabaseProvider
    {
        IDbConnection Connection { get; }

        string ProviderName { get; }

        bool IsDatabaseVersioned(string tableName);

        IDatabaseTableGenerator DatabaseTableGenerator { get; }

        IQueryGenerator QueryGenerator { get; }

        int MaximumParametersPerQuery { get; }
    }
}