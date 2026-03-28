using System.Linq.Expressions;
using Dal.Entities;
using Dal.Helpers;
using Domain.Models.RegistrySearchModels;

namespace Dal.RegistryRepositories.TeacherPreference;

public class TeacherPreferenceRegistryPredicateBuilder : IPredicateBuilder<DbTeacherPreference, TeacherPreferenceRegistryInternalSearchModel>
{
    public Expression<Func<DbTeacherPreference, bool>> Predicate { get; } = PredicateBuilderExtensions.True<DbTeacherPreference>();

    public Expression<Func<DbTeacherPreference, bool>> Build(TeacherPreferenceRegistryInternalSearchModel searchModel)
    {
        return Predicate;
    }
}