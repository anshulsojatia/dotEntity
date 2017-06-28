// #region Author Information
// // IWrappedDisposable.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using System;

namespace SpruceFramework
{
    public interface IWrappedDisposable : IDisposable
    {
        bool IsDisposed();
    }
}