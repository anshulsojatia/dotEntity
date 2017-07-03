// #region Author Information
// // DotEntityVersion.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using System.ComponentModel.DataAnnotations;

namespace SpruceFramework.Versioning
{
    public class DotEntityVersion
    {
        [Key]
        public int Id { get; set; }

        public string VersionKey { get; set; }
    }
}