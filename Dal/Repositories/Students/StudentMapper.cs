using System.Diagnostics.CodeAnalysis;
using Dal.Entities;
using Dal.Mapping;
using Domain.Models;

namespace Dal.Repositories.Students;

public class StudentMapper : IRepositoryMapper<DbStudent, Student>
{
    [return: NotNullIfNotNull("entity")]
    public Student? Map(DbStudent? entity) => StudentMappingRegister.MapEntityToModel(entity);

    public void Update(DbStudent entity, Student model)
    {
        StudentMappingRegister.UpdateEntityWithModel(model, entity);
    }
}