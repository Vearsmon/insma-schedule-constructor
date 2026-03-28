using System.Linq.Expressions;
using Dal.Entities;
using Dal.Helpers;
using Domain.Models.RegistrySearchModels;

namespace Dal.RegistryRepositories.Teacher;

public class TeacherRegistryPredicateBuilder : IPredicateBuilder<DbTeacher, TeacherRegistryInternalSearchModel>
{
    public Expression<Func<DbTeacher, bool>> Predicate { get; } = PredicateBuilderExtensions.True<DbTeacher>();

    public Expression<Func<DbTeacher, bool>> Build(TeacherRegistryInternalSearchModel searchModel)
    {
        return Predicate;
    }
}