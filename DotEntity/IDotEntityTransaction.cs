// #region Author Information
// // IDotEntityTransaction.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using System;

namespace DotEntity
{
    public interface IDotEntityTransaction : IWrappedDisposable
    {
        void Commit();

        IDotEntityQueryManager Manager { get; }

        bool Success { get; set; }

        bool IsInternalTransaction { get; }
    }
}