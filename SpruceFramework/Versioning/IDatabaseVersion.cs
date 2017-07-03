// #region Author Information
// // IDatabaseVersion.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion
namespace SpruceFramework.Versioning
{
    public interface IDatabaseVersion
    {
        string VersionKey { get; }

        void Upgrade(ISpruceTransaction transaction);

        void Downgrade(ISpruceTransaction transaction);
    }
}