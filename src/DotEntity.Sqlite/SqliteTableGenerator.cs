using System;
using System.Linq;
using System.Reflection;
using System.Text;
using DotEntity.Extensions;

namespace DotEntity.Sqlite
{
    public class SqliteTableGenerator : DefaultDatabaseTableGenerator
    {
        public override string GetFormattedDbTypeForType(Type type, PropertyInfo propertyInfo = null)
        {
            ThrowIfInvalidDataTypeMapping(type, out string dbTypeString);
            var typeBuilder = new StringBuilder(dbTypeString);
            var nullable = IsNullable(type, propertyInfo);
            typeBuilder.Append(nullable ? " NULL" : " NOT NULL");
            return typeBuilder.ToString();
        }

        public override string GetCreateTableScript(Type type)
        {
            return GetCreateTableScriptWithRelation(type);
        }

        private string GetCreateTableScriptWithRelation(Type type, Relation relation = null)
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
                //do we have key attribute here?
                if (fieldName == keyColumn && pType == typeof(int))
                {
                    identityString = " PRIMARY KEY AUTOINCREMENT";
                }
                builder.Append($"\t {fieldName.ToEnclosed()} {dbFieldType}{identityString},");
                builder.Append(Environment.NewLine);
            }
            if (relation != null)
            {
                var toTable = DotEntityDb.GetTableNameForType(relation.SourceType);
                builder.Append($"FOREIGN KEY({relation.DestinationColumnName.ToEnclosed()}) REFERENCES {toTable.ToEnclosed()}({relation.SourceColumnName.ToEnclosed()}),");
                builder.Append(Environment.NewLine);
            }
            var query = builder.ToString().TrimEnd(',', '\n', '\r') + ");";
            return query;
        }

        public override string GetCreateConstraintScript(Relation relation)
        {
            //sqlite doesn't support adding foreign key constraints to an existing table
            //we will have to create a new table with constraints, copy the data from old table, drop the old table, rename the new table huh :|

            var toTable = DotEntityDb.GetTableNameForType(relation.DestinationType);
            var builder = new StringBuilder($"ALTER TABLE {toTable.ToEnclosed()} RENAME TO {(toTable + "_temporary").ToEnclosed()};");
            builder.Append(Environment.NewLine);
            builder.Append(GetCreateTableScriptWithRelation(relation.DestinationType, relation));
            builder.Append(Environment.NewLine);
            builder.Append($"INSERT INTO {toTable.ToEnclosed()} SELECT * FROM {(toTable + "_temporary").ToEnclosed()};");
            builder.Append(Environment.NewLine);
            builder.Append($"DROP TABLE {(toTable + "_temporary").ToEnclosed()};");
            return builder.ToString();
        }

        public override string GetDropConstraintScript(Relation relation)
        {
            //sqlite doesn't support adding foreign key constraints to an existing table
            //we will have to create a new table with constraints, copy the data from old table, drop the old table, rename the new table huh :|

            var toTable = DotEntityDb.GetTableNameForType(relation.DestinationType);
            var builder = new StringBuilder($"ALTER TABLE {toTable.ToEnclosed()} RENAME TO {(toTable + "_temporary").ToEnclosed()};");
            builder.Append(Environment.NewLine);
            builder.Append(GetCreateTableScriptWithRelation(relation.DestinationType));
            builder.Append(Environment.NewLine);
            builder.Append($"INSERT INTO {toTable.ToEnclosed()} SELECT * FROM {(toTable + "_temporary").ToEnclosed()};");
            builder.Append(Environment.NewLine);
            builder.Append($"DROP TABLE {(toTable + "_temporary").ToEnclosed()};");
            return builder.ToString();
        }

        public override string GetDropTableScript(string tableName)
        {
            return $"DROP TABLE IF EXISTS {tableName.ToEnclosed()};";
        }
    }
}