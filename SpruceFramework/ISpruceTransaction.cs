// #region Author Information
// // ISpruceTransaction.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using System;

namespace SpruceFramework
{
    public interface ISpruceTransaction : IWrappedDisposable
    {
        void Commit();

        ISpruceQueryManager Manager { get; }

        bool Success { get; set; }

        bool IsInternalTransaction { get; }
    }
}