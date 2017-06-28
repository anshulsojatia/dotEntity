// #region Author Information
// // WrappedDisposableExtensions.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion
namespace SpruceFramework.Extensions
{
    public static class WrappedDisposableExtensions
    {
        public static bool IsNullOrDisposed(this IWrappedDisposable entity)
        {
            return entity == null || entity.IsDisposed();
        }
    }
}