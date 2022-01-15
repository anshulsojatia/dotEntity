using System;
using System.Linq;
using System.Reflection;
using System.Text;
using DotEntity.Extensions;

namespace DotEntity.MySql
{
    public class MySqlTableGenerator : DefaultDatabaseTableGenerator
    {
        public override string GetFormattedDbTypeForType(Type type, PropertyInfo propertyInfo = null)
        {
            ThrowIfInvalidDataTypeMapping(type, out string dbTypeString);
            var typeBuilder = new StringBuilder(dbTypeString);
            var nullable = IsNullable(type, propertyInfo);
            var maxLength = 0;
            if (maxLength > 0)
                typeBuilder.Append($"({maxLength})");
            typeBuilder.Append(nullable ? " NULL" : " NOT NULL");
            return typeBuilder.ToString();
        }

        public override string GetCreateTableScript(Type type)
        {
            var tableName = DotEntityDb.GetTableNameForType(type);
            var properties = type.GetDatabaseUsableProperties();

            var builder = new StringBuilder($"CREATE TABLE {tableName.ToEnclosed()}{Environment.NewLine}(");
            var keyColumn = type.GetKeyColumnName();

            //is key column nullable
            var propertyInfos = properties as PropertyInfo[] ?? properties.ToArray();
            Throw.IfKeyTypeNullable(propertyInfos.First(x => x.Name == keyColumn).PropertyType, keyColumn);

            foreach (var property in propertyInfos)
            {
                var pType = property.PropertyType;
                var fieldName = property.Name;
                var dbFieldType = GetFormattedDbTypeForType(pType, property);
                var identityString = "";
                var collation = "";
                //do we have key attribute here?
                if (fieldName == keyColumn && pType == typeof(int))
                {
                    identityString = " AUTO_INCREMENT";
                }
                //set collation for string
                if(pType == typeof(string))
                {
                    collation = " COLLATE utf8mb4_unicode_ci";
                }
                builder.Append($"\t {fieldName.ToEnclosed()} {dbFieldType}{identityString}{collation},");
                builder.Append(Environment.NewLine);
            }
            builder.Append($"PRIMARY KEY ({keyColumn.ToEnclosed()}));");
            return builder.ToString();
        }

        public override string GetDropConstraintScript(Relation relation)
        {
            var fromTable = DotEntityDb.GetTableNameForType(relation.SourceType);
            var toTable = DotEntityDb.GetTableNameForType(relation.DestinationType);
            var constraintName = "c" + GetForeignKeyConstraintName(fromTable, toTable, relation.SourceColumnName,
                relation.DestinationColumnName);
            //mysql restricts constraint names to 64 chars
            if (constraintName.Length > 64)
                constraintName = constraintName.Substring(0, 64);

            var builder = new StringBuilder($"ALTER TABLE {toTable.ToEnclosed()}{Environment.NewLine}");
            builder.Append($"DROP FOREIGN KEY {constraintName.ToEnclosed()};");
            return builder.ToString();
        }

        public override string GetAddColumnScript<T, T1>(string columnName, T1 value, PropertyInfo propertyInfo = null)
        {
            var tableName = DotEntityDb.GetTableNameForType(typeof(T));
            var constraintName = $"CONSTRAINT DF_{tableName}_{columnName}";
            var dataTypeString = GetFormattedDbTypeForType(typeof(T1), propertyInfo);
            var builder = new StringBuilder($"ALTER TABLE {tableName.ToEnclosed()}{Environment.NewLine}");
            builder.Append($"ADD {columnName.ToEnclosed()} {dataTypeString} NOT NULL");
            if (value != null)
            {
                builder.Append($" DEFAULT '{GetValueInTargetType<T1>(value)}'{Environment.NewLine}");
            }
            return builder.ToString();
        }

        public override string GetDropTableScript(string tableName)
        {
            return $"DROP TABLE IF EXISTS {tableName.ToEnclosed()};";
        }

        public override string GetCreateConstraintScript(Relation relation, bool withCascade = false)
        {
            var fromTable = DotEntityDb.GetTableNameForType(relation.SourceType);
            var toTable = DotEntityDb.GetTableNameForType(relation.DestinationType);
            var constraintName = GetForeignKeyConstraintName(fromTable, toTable, relation.SourceColumnName,
                relation.DestinationColumnName);

            //mysql restricts constraint names to 64 chars
            if (constraintName.Length > 64)
                constraintName = constraintName.Substring(0, 63);
            var builder = new StringBuilder($"ALTER TABLE {toTable.ToEnclosed()}{Environment.NewLine}");
            builder.Append($"ADD CONSTRAINT {("c" + constraintName).ToEnclosed()}{Environment.NewLine}");
            builder.Append($"FOREIGN KEY {constraintName.ToEnclosed()}({relation.DestinationColumnName.ToEnclosed()}) REFERENCES {fromTable.ToEnclosed()}({relation.SourceColumnName.ToEnclosed()})");
            if (withCascade)
            {
                builder.Append($" ON DELETE CASCADE");
            }
            builder.Append(";");
            return builder.ToString();
        }

        public override string GetDropIndexScript(string tableName, string[] columnNames)
        {
            var indexName = GetIndexName(tableName, columnNames);
            var builder = new StringBuilder($"ALTER TABLE {tableName.ToEnclosed()}{Environment.NewLine}");
            builder.Append($"DROP INDEX {indexName};");
            return builder.ToString();
        }

        public override string GetCreateIndexScript(Type type, string[] columnNames, string[] additionalColumns = null, bool unique = false)
        {
            //ignore additional columns for mysql
            return base.GetCreateIndexScript(type, columnNames, null, unique);
        }
    }
}
