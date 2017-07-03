// #region Author Information
// // SpruceTransaction.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using SpruceFramework.Extensions;

namespace SpruceFramework
{
    internal sealed class SpruceTransaction : ISpruceTransaction
    {
        private bool _disposed = false;
        public void Dispose()
        {
            //good byee
            Manager?.Dispose();
            _disposed = true;
        }

        public bool IsDisposed()
        {
            return _disposed;
        }


        public void Commit()
        {
            if (!IsInternalTransaction)
                Success = Manager.AsSpruceQueryManager().CommitTransaction();
        }

        public void CommitInternal()
        {
            Success = Manager.AsSpruceQueryManager().CommitTransaction();
        }

        internal SpruceTransaction(bool internalTransaction = false)
        {
            Manager = new SpruceQueryManager(true);
            IsInternalTransaction = internalTransaction;
        }

        public ISpruceQueryManager Manager { get; }

        public bool Success { get; set; }
        public bool IsInternalTransaction { get; }
    }
}