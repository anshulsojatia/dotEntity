// #region Author Information
// // SpruceTransaction.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion
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
            //todo: throw exception here
            (Manager as SpruceQueryManager).CommitTransaction();
        }

        internal SpruceTransaction()
        {
            Manager = new SpruceQueryManager(true);
        }

        public ISpruceQueryManager Manager { get; }
    }
}