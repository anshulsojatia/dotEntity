// #region Author Information
// // IDatabaseVersion.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion
namespace DotEntity.Versioning
{
    public interface IDatabaseVersion
    {
        string VersionKey { get; }

        void Upgrade(IDotEntityTransaction transaction);

        void Downgrade(IDotEntityTransaction transaction);
    }
}