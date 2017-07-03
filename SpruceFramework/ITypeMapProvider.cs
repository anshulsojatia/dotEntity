// #region Author Information
// // ITypeMapProvider.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using System;
using System.Collections.Generic;

namespace SpruceFramework
{
    public interface ITypeMapProvider
    {
        Dictionary<Type, string> TypeMap { get; }
    }
}