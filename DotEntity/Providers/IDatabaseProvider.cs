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

        bool IsDatabaseVersioned(string versionTableName);

        IDatabaseTableGenerator DatabaseTableGenerator { get; set; }

        IQueryGenerator QueryGenerator { get; set; }

        ITypeMapProvider TypeMapProvider { get; set; }

        int MaximumParametersPerQuery { get; set; }
    }
}