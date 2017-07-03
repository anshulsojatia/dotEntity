// #region Author Information
// // SpruceTable.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion


using System;

namespace SpruceFramework
{
    public sealed class SpruceTable
    {
        public static ISpruceTransaction BeginTransaction()
        {
            return new SpruceTransaction();
        }

        internal static ISpruceTransaction BeginInternalTransaction()
        {
            return new SpruceTransaction(true);
        }
    }
}