using System.Linq.Expressions;
using Dal.Entities;
using Domain.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dal.Helpers;

public static class ModelBuilderExtensions
{
    public static EntityTypeBuilder<TEntity> HasFixedLengthConstraint<TEntity>(this EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, object?>> prop, int length, bool acceptNull = false) where TEntity : class
    {
        return builder.ToTable(t => t.HasFixedLengthConstraint(prop, length, acceptNull));
    }

    public static CheckConstraintBuilder HasFixedLengthConstraint<T>(this TableBuilder<T> tableBuilder,
        Expression<Func<T, object?>> prop, int length, bool acceptNull) where T : class
    {
        var (name, sql) = FixedLengthConstraint(prop.GetPropertyAccess().Name, length, acceptNull);
        return tableBuilder.HasCheckConstraint(name, sql);
    }

    public static void HasConcurrencyTokenCheck<TEntityWithConcurrencyToken>(
        this EntityTypeBuilder<TEntityWithConcurrencyToken> builder)
        where TEntityWithConcurrencyToken : class, IDbEntityWithConcurrencyToken
    {
        builder.Property(x => x.ConcurrencyToken)
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsRowVersion();
    }

    public static IndexBuilder<T> HasNotNullFilter<T>(this IndexBuilder<T> indexBuilder,
        Expression<Func<T, object?>> prop) where T : class
    {
        var propertyName = prop.GetPropertyAccess().Name.ToSnakeCase();
        return indexBuilder.HasFilter($"{propertyName} IS NOT NULL");
    }

    public static ModelBuilder SetUnderscoreSnakeConventions(this ModelBuilder modelBuilder)
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            var tableName = SnakeCase(Cleanup(entity.DisplayName()));
            entity.SetTableName(tableName);

            foreach (var property in entity.GetProperties())
            {
                if (property.Name == nameof(IDbEntityWithConcurrencyToken.ConcurrencyToken))
                {
                    continue;
                }
                property.SetColumnName(SnakeCase(Cleanup(property.Name)));
            }
            foreach (var mutableKey in entity.GetKeys())
            {
                var keyName = Cleanup(mutableKey.GetName()!)
                    .Replace("PK_", "pk_")
                    .Replace("AK_", "ak_");
                mutableKey.SetName(SnakeCase(keyName));
            }
            foreach (var mutableForeignKey in entity.GetForeignKeys())
            {
                var keyName = Cleanup(mutableForeignKey.GetConstraintName()!)
                    .Replace("FK_", "fk_");
                mutableForeignKey.SetConstraintName(SnakeCase(keyName));
            }
            foreach (var mutableIndex in entity.GetIndexes())
            {
                var indexName = mutableIndex.GetDatabaseName()!
                    .Replace("IX_", "ix_");
                mutableIndex.SetDatabaseName(indexName);
            }
        }
        return modelBuilder;
    }

    public static string SnakeCase(this string name)
    {
        var upperCaseLength = 0;
        for (var i = 1; i < name.Length; i++)
        {
            if (name[i] >= 'A' && name[i] <= 'Z')
            {
                upperCaseLength++;
            }
        }
        var bufferSize = name.Length + upperCaseLength;

        Span<char> buffer = stackalloc char[bufferSize];

        var bufferPosition = 0;
        var namePosition = 0;
        while (bufferPosition < buffer.Length)
        {
            if (namePosition > 0 && name[namePosition] >= 'A' && name[namePosition] <= 'Z')
            {
                buffer[bufferPosition] = '_';
                buffer[bufferPosition + 1] = char.ToLower(name[namePosition]);
                bufferPosition += 2;
                namePosition++;
                continue;
            }

            if (namePosition != 0)
            {
                buffer[bufferPosition] = name[namePosition];
            }
            else
            {
                buffer[bufferPosition] = char.ToLower(name[namePosition]);
            }
            bufferPosition++;
            namePosition++;
        }

        return buffer.ToString();
    }

    public static string GetTableName(this string dbClassName)
    {
        return SnakeCase(Cleanup(dbClassName));
    }

    private static string Cleanup(string name)
    {
        return name.Replace("Db", string.Empty).Replace("Entity", string.Empty);
    }

    private static (string Name, string Sql) FixedLengthConstraint(string fieldName, int length, bool acceptNull)
    {
        var dbFieldName = fieldName.ToSnakeCase();
        return ($"{dbFieldName}_length", $"{(acceptNull ? $"{dbFieldName} IS NULL OR " : string.Empty)}LENGTH({dbFieldName}) = {length}");
    }
}
