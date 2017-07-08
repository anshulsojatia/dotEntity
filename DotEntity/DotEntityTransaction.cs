// #region Author Information
// // DotEntityTransaction.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using DotEntity.Extensions;

namespace DotEntity
{
    internal sealed class DotEntityTransaction : IDotEntityTransaction
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
                Success = Manager.AsDotEntityQueryManager().CommitTransaction();
        }

        public void CommitInternal()
        {
            Success = Manager.AsDotEntityQueryManager().CommitTransaction();
        }

        internal DotEntityTransaction(bool internalTransaction = false)
        {
            Manager = new DotEntityQueryManager(true);
            IsInternalTransaction = internalTransaction;
        }

        public IDotEntityQueryManager Manager { get; }

        public bool Success { get; set; }
        public bool IsInternalTransaction { get; }
    }
}