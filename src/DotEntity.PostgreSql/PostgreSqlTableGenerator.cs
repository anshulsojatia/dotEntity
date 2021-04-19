using System;
using System.Linq;
using System.Reflection;
using System.Text;
using DotEntity.Extensions;

namespace DotEntity.PostgreSql
{
    public class PostgreSqlTableGenerator : DefaultDatabaseTableGenerator
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

            for (var i = 0; i < propertyInfos.Length; i++)
            {
                var property = propertyInfos[i];
                var pType = property.PropertyType;
                var fieldName = property.Name;
                var dbFieldType = GetFormattedDbTypeForType(pType, property);
                //do we have key attribute here?
                if (fieldName == keyColumn && pType == typeof(int))
                {
                    dbFieldType = "SERIAL PRIMARY KEY";
                }
                builder.Append($"\t {fieldName.ToEnclosed()} {dbFieldType}");
                if (i < propertyInfos.Length - 1)
                {
                    builder.Append(",");
                    builder.Append(Environment.NewLine);
                }
            }

            builder.Append($");");
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
            builder.Append($"DROP CONSTRAINT {constraintName.ToEnclosed()};");
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
            builder.Append($"FOREIGN KEY ({relation.DestinationColumnName.ToEnclosed()}) REFERENCES {fromTable.ToEnclosed()}({relation.SourceColumnName.ToEnclosed()})");
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
            var builder = new StringBuilder($"DROP INDEX {indexName};");
            return builder.ToString();
        }

        public override string GetCreateIndexScript(Type type, string[] columnNames, string[] additionalColumns = null, bool unique = false)
        {
            var tableName = DotEntityDb.GetTableNameForType(type);
            var indexName = GetIndexName(tableName, columnNames);
            var columnStr = string.Join(",", columnNames.Select(x => x.ToEnclosed()));
            var uniqueStr = unique ? "UNIQUE " : "";
            var builder = new StringBuilder($"CREATE {uniqueStr}INDEX {indexName} ON {tableName.ToEnclosed()} ({columnStr})");
            return builder.ToString();
        }
    }
}
