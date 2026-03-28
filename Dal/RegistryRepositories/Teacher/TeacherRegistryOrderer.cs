using Dal.Entities;
using Domain.Models.RegistrySearchModels;

namespace Dal.RegistryRepositories.Teacher;

public class TeacherRegistryOrderer : IRegistryRepositoryOrderer<DbTeacher, TeacherRegistryInternalSearchModel>
{
    public IQueryable<DbTeacher> Order(IQueryable<DbTeacher> queryable,
        TeacherRegistryInternalSearchModel searchModel)
    {
        return queryable;
    }
}