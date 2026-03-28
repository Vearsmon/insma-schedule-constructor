using System.Diagnostics.CodeAnalysis;
using Dal.Entities;
using Dal.Mapping;
using Domain.Models;

namespace Dal.Repositories.Teachers;

public class TeacherMapper : IRepositoryMapper<DbTeacher, Teacher>
{
    [return: NotNullIfNotNull("entity")]
    public Teacher? Map(DbTeacher? entity)
    {
        return MappingRegister.Map(entity);
    }

    public void Update(DbTeacher entity, Teacher model)
    {
        MappingRegister.Update(model, entity);
    }
}