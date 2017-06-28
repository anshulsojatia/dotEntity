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
    }
}