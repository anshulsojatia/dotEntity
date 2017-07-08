// #region Author Information
// // IDataTableGenerator.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using System;

namespace DotEntity
{
    public interface IDatabaseTableGenerator
    {
        string GetFormattedDbTypeForType(Type type, int maxLength = 0);

        string GetCreateTableScript<T>();

        string GetCreateTableScript(Type type);

        string GetDropTableScript<T>();

        string GetDropTableScript(Type type);

        string GetCreateConstraintScript(Relation relation);

        string GetDropConstraintScript(Relation relation);

        string GetAddColumnScript(Type type, string columnName, Type columnType, int maxLength = 0);

        string GetDropColumnScript(Type type, string columnName);

        string GetAlterColumnScript(Type type, string columnName, Type columnType, int maxLength = 0);
    }
}