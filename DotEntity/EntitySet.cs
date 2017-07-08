// #region Author Information
// // EntitySet.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion


namespace DotEntity
{
    public sealed class EntitySet
    {
        public static IDotEntityTransaction BeginTransaction()
        {
            return new DotEntityTransaction();
        }

        internal static IDotEntityTransaction BeginInternalTransaction()
        {
            return new DotEntityTransaction(true);
        }

        public static IMultiResult Query(string query, dynamic parameters)
        {
            using (var manager = new DotEntityQueryManager())
            {
                return manager.DoMultiResult(query, parameters);
            }
        }
    }
}