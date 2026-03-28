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
        return Predicate;
    }
}