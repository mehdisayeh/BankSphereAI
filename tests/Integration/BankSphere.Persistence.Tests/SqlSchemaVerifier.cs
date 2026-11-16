using System.Data.Common;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BankSphere.Persistence.Tests;

internal static class SqlSchemaVerifier
{
    internal sealed record TableStatus(string Name, int Columns, int Indexes, int ForeignKeys, int Checks, long Rows);
    internal sealed record SchemaStatus(string Schema, string[] AppliedMigrations, TableStatus[] Tables);

    internal static async Task<SchemaStatus> VerifyAsync(DbContext context, string schema)
    {
        var model = context.GetService<IDesignTimeModel>().Model;
        Require(model.GetDefaultSchema() == schema, $"{schema}: default schema mismatch");
        Require(!context.Database.HasPendingModelChanges(), $"{schema}: pending model changes");
        var expectedMigrations = context.Database.GetMigrations().ToArray();
        var applied = (await context.Database.GetAppliedMigrationsAsync()).ToArray();
        Require(expectedMigrations.SequenceEqual(applied), $"{schema}: migration history mismatch");
        await context.Database.OpenConnectionAsync();
        var connection = context.Database.GetDbConnection();
        var tables = model.GetRelationalModel().Tables.ToArray();
        Require(tables.All(table => table.Schema == schema), $"{schema}: model table outside its schema");
        var actualTables = await QueryAsync(connection,
            "SELECT t.name FROM sys.tables t JOIN sys.schemas s ON t.schema_id=s.schema_id WHERE s.name=@schema ORDER BY t.name", schema);
        Require(actualTables.Select(row => row[0]).Order(StringComparer.Ordinal).SequenceEqual(
            tables.Select(table => table.Name).Append("__EFMigrationsHistory").Order(StringComparer.Ordinal)), $"{schema}: table set mismatch");
        var results = new List<TableStatus>();
        foreach (var table in tables.OrderBy(table => table.Name, StringComparer.Ordinal))
        {
            var columns = await QueryAsync(connection, """
                SELECT c.name, CAST(c.is_nullable AS int),
                    CASE WHEN ty.name IN ('varchar','nvarchar','char','nchar','binary','varbinary') THEN ty.name+'('+
                        CASE WHEN c.max_length=-1 THEN 'max' ELSE CONVERT(varchar(10),c.max_length/CASE WHEN ty.name IN ('nvarchar','nchar') THEN 2 ELSE 1 END) END+')'
                    WHEN ty.name IN ('decimal','numeric') THEN ty.name+'('+CONVERT(varchar(10),c.precision)+','+CONVERT(varchar(10),c.scale)+')'
                    ELSE ty.name END
                FROM sys.columns c JOIN sys.types ty ON c.user_type_id=ty.user_type_id
                WHERE c.object_id=OBJECT_ID(QUOTENAME(@schema)+'.'+QUOTENAME(@table))
                """, schema, table.Name);
            Require(columns.Count == table.Columns.Count(), $"{schema}.{table.Name}: column count mismatch");
            foreach (var column in table.Columns)
            {
                var actual = columns.SingleOrDefault(row => row[0] == column.Name);
                Require(actual is not null && actual[1] == (column.IsNullable ? "1" : "0")
                    && string.Equals(actual[2], column.StoreType, StringComparison.OrdinalIgnoreCase),
                    $"{schema}.{table.Name}.{column.Name}: store type/nullability mismatch");
            }

            var indexes = await QueryAsync(connection, """
                SELECT i.name, CAST(i.is_unique AS int), COALESCE(i.filter_definition,''), c.name, CAST(i.is_disabled AS int)
                FROM sys.indexes i JOIN sys.index_columns ic ON i.object_id=ic.object_id AND i.index_id=ic.index_id
                JOIN sys.columns c ON c.object_id=ic.object_id AND c.column_id=ic.column_id
                WHERE i.object_id=OBJECT_ID(QUOTENAME(@schema)+'.'+QUOTENAME(@table)) AND ic.key_ordinal>0
                ORDER BY i.name, ic.key_ordinal
                """, schema, table.Name);
            var expectedIndexes = table.UniqueConstraints.Select(index => (index.Name, Unique: true, Filter: "", Columns: index.Columns.Select(column => column.Name).ToArray()))
                .Concat(table.Indexes.Select(index => (index.Name, index.IsUnique, index.Filter ?? "", index.Columns.Select(column => column.Name).ToArray()))).ToArray();
            Require(indexes.Select(row => row[0]).Distinct().Count() == expectedIndexes.Length, $"{schema}.{table.Name}: index count mismatch");
            foreach (var (name, unique, filter, names) in expectedIndexes)
            {
                var actual = indexes.Where(row => row[0] == name).ToArray();
                Require(actual.Select(row => row[3]).SequenceEqual(names)
                    && actual.All(row => row[1] == (unique ? "1" : "0") && Normalize(row[2]) == Normalize(filter) && row[4] == "0"),
                    $"{schema}.{table.Name}.{name}: index/unique key mismatch");
            }
            var primary = await QueryAsync(connection,
                "SELECT name FROM sys.key_constraints WHERE parent_object_id=OBJECT_ID(QUOTENAME(@schema)+'.'+QUOTENAME(@table)) AND type='PK'", schema, table.Name);
            Require(primary.Count == 1 && primary[0][0] == table.PrimaryKey?.Name, $"{schema}.{table.Name}: primary key mismatch");

            var foreignKeys = await QueryAsync(connection, """
                SELECT fk.name, pc.name, rs.name, rt.name, rc.name, fk.delete_referential_action_desc,
                    CAST(fk.is_disabled AS int), CAST(fk.is_not_trusted AS int)
                FROM sys.foreign_keys fk JOIN sys.foreign_key_columns fkc ON fk.object_id=fkc.constraint_object_id
                JOIN sys.columns pc ON pc.object_id=fkc.parent_object_id AND pc.column_id=fkc.parent_column_id
                JOIN sys.tables rt ON rt.object_id=fkc.referenced_object_id JOIN sys.schemas rs ON rs.schema_id=rt.schema_id
                JOIN sys.columns rc ON rc.object_id=fkc.referenced_object_id AND rc.column_id=fkc.referenced_column_id
                WHERE fk.parent_object_id=OBJECT_ID(QUOTENAME(@schema)+'.'+QUOTENAME(@table))
                ORDER BY fk.name,fkc.constraint_column_id
                """, schema, table.Name);
            Require(foreignKeys.Select(row => row[0]).Distinct().Count() == table.ForeignKeyConstraints.Count(), $"{schema}.{table.Name}: FK count mismatch");
            foreach (var fk in table.ForeignKeyConstraints)
            {
                var actual = foreignKeys.Where(row => row[0] == fk.Name).ToArray();
                var delete = fk.OnDeleteAction switch
                {
                    ReferentialAction.Cascade => "CASCADE", ReferentialAction.SetNull => "SET_NULL",
                    ReferentialAction.SetDefault => "SET_DEFAULT", _ => "NO_ACTION"
                };
                Require(actual.Select(row => row[1]).SequenceEqual(fk.Columns.Select(column => column.Name))
                    && actual.Select(row => row[4]).SequenceEqual(fk.PrincipalColumns.Select(column => column.Name))
                    && actual.All(row => row[2] == fk.PrincipalTable.Schema && row[3] == fk.PrincipalTable.Name
                        && row[5] == delete && row[6] == "0" && row[7] == "0"), $"{schema}.{table.Name}.{fk.Name}: FK mismatch");
            }

            var checks = await QueryAsync(connection, """
                SELECT name,definition,CAST(is_disabled AS int),CAST(is_not_trusted AS int) FROM sys.check_constraints
                WHERE parent_object_id=OBJECT_ID(QUOTENAME(@schema)+'.'+QUOTENAME(@table))
                """, schema, table.Name);
            Require(checks.Count == table.CheckConstraints.Count(), $"{schema}.{table.Name}: check count mismatch");
            foreach (var check in table.CheckConstraints)
            {
                var actual = checks.SingleOrDefault(row => row[0] == check.Name);
                Require(actual is not null && Normalize(actual[1]) == Normalize(check.Sql) && actual[2] == "0" && actual[3] == "0",
                    $"{schema}.{table.Name}.{check.Name}: check definition/status mismatch");
            }
            var qualified = $"[{schema.Replace("]", "]]", StringComparison.Ordinal)}].[{table.Name.Replace("]", "]]", StringComparison.Ordinal)}]";
            var rows = await QueryAsync(connection, $"SELECT COUNT_BIG(*) FROM {qualified}", schema);
            results.Add(new(table.Name, columns.Count, expectedIndexes.Length, table.ForeignKeyConstraints.Count(), checks.Count,
                long.Parse(rows[0][0], CultureInfo.InvariantCulture)));
        }
        return new(schema, applied, results.ToArray());
    }

    private static string Normalize(string value) => Regex.Replace(value, @"[\s\[\]()]", "", RegexOptions.CultureInvariant,
        TimeSpan.FromSeconds(1)).ToUpperInvariant();

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static async Task<List<string[]>> QueryAsync(DbConnection connection, string sql, string schema, string? table = null)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        var schemaParameter = command.CreateParameter(); schemaParameter.ParameterName = "@schema"; schemaParameter.Value = schema;
        command.Parameters.Add(schemaParameter);
        if (table is not null)
        {
            var parameter = command.CreateParameter(); parameter.ParameterName = "@table"; parameter.Value = table; command.Parameters.Add(parameter);
        }
        await using var reader = await command.ExecuteReaderAsync();
        var result = new List<string[]>();
        while (await reader.ReadAsync()) result.Add(Enumerable.Range(0, reader.FieldCount)
            .Select(index => Convert.ToString(reader.GetValue(index), CultureInfo.InvariantCulture) ?? "").ToArray());
        return result;
    }
}
