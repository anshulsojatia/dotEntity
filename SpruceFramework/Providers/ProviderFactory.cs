// #region Author Information
// // ProviderFactory.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using System.Data;

namespace SpruceFramework.Providers
{
    public class ProviderFactory
    {
        public static IDbConnection Connect()
        {
            return Spruce.Provider.Connection;
        }
    }
}