using Dal.Entities;
using Domain.Models.RegistrySearchModels;

namespace Dal.RegistryRepositories.TeacherPreference;

public class TeacherPreferenceRegistryOrderer
    : IRegistryRepositoryOrderer<DbTeacherPreference, TeacherPreferenceRegistryInternalSearchModel>
{
    public IQueryable<DbTeacherPreference> Order(IQueryable<DbTeacherPreference> queryable,
        TeacherPreferenceRegistryInternalSearchModel searchModel)
    {
        return queryable;
    }
}