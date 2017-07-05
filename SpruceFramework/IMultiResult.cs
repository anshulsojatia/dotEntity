// #region Author Information
// // IMultiResult.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using System.Collections.Generic;

namespace SpruceFramework
{
    public interface IMultiResult : IWrappedDisposable
    {
        T SelectAs<T>() where T : class;

        IEnumerable<T> SelectAllAs<T>() where T : class;

        TType SelectScalerAs<TType>();
    }
}