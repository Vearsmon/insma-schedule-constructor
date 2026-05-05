using System.Diagnostics.CodeAnalysis;
using Dal.Entities;
using Dal.Mapping;
using Domain.Models;

namespace Dal.Repositories.StudentGroups;

public class StudentGroupMapper : IRepositoryMapper<DbStudentGroup, StudentGroup>
{
    [return: NotNullIfNotNull("entity")]
    public StudentGroup? Map(DbStudentGroup? entity) => StudentGroupMappingRegister.MapEntityToModel(entity);

    public void Update(DbStudentGroup entity, StudentGroup model)
    {
        StudentGroupMappingRegister.UpdateEntityWithModel(model, entity);
    }
}