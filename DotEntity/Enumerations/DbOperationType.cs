// #region Author Information
// // DbOperationType.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion
namespace DotEntity.Enumerations
{
    public enum DbOperationType
    {
        Insert, //c
        Update, //u
        Delete, //d
        Select,  //r
        SelectScaler,
        Procedure,
        Other,
        MultiQuery
    }
}