using Dal.Entities;
using Domain.Models.RegistrySearchModels;

namespace Dal.RegistryRepositories.StudentGroup;

public class StudentGroupRegistryOrderer
    : IRegistryRepositoryOrderer<DbStudentGroup, StudentGroupRegistryInternalSearchModel>
{
    public IQueryable<DbStudentGroup> Order(IQueryable<DbStudentGroup> queryable,
        StudentGroupRegistryInternalSearchModel searchModel)
    {
        return queryable;
    }
}