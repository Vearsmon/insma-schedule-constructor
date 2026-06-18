using System.Linq.Expressions;
using Dal.Entities;
using Dal.Helpers;
using Domain.Models.RegistrySearchModels;

namespace Dal.RegistryRepositories.StudentGroup;

public class StudentGroupRegistryPredicateBuilder
    : IPredicateBuilder<DbStudentGroup, StudentGroupRegistryInternalSearchModel>
{
    public Expression<Func<DbStudentGroup, bool>> Predicate { get; } =
        PredicateBuilderExtensions.True<DbStudentGroup>();

    public Expression<Func<DbStudentGroup, bool>> Build(StudentGroupRegistryInternalSearchModel searchModel)
    {
        var nameLowerCaseTrimmed = string.IsNullOrEmpty(searchModel.Name) ? null : searchModel.Name!.ToLower().Trim().Replace("  ", " ");

        return Predicate
                .AndIf(searchModel.StudentGroupType.HasValue, f => f.StudentGroupType == searchModel.StudentGroupType)
                .AndIf(!string.IsNullOrEmpty(nameLowerCaseTrimmed),
                    f => f.Name.ToLower().Contains(nameLowerCaseTrimmed!))
            ;
    }
}