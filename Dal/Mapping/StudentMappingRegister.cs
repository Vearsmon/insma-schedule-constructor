using Dal.Entities;
using Domain.Models;
using Riok.Mapperly.Abstractions;

namespace Dal.Mapping;

[Mapper]
public static partial class StudentMappingRegister
{
    [MapProperty(nameof(DbStudent.User), nameof(Student.User), Use = nameof(@UserMappingRegister.MapEntityToModel))]
    [MapProperty(nameof(DbStudent.StudentGroup), nameof(Student.StudentGroup), Use = nameof(@StudentGroupMappingRegister.MapEntityToModel))]
    public static partial Student? MapEntityToModel(DbStudent? entity);

    [MapProperty(nameof(Student.User), nameof(DbStudent.User), Use = nameof(@UserMappingRegister.MapModelToEntity))]
    [MapProperty(nameof(Student.StudentGroup), nameof(DbStudent.StudentGroup), Use = nameof(@StudentGroupMappingRegister.MapModelToEntity))]
    public static partial DbStudent? MapModelToEntity(Student? model);

    [MapProperty(nameof(Student.User), nameof(DbStudent.User), Use = nameof(@UserMappingRegister.MapModelToEntity))]
    [MapProperty(nameof(Student.StudentGroup), nameof(DbStudent.StudentGroup), Use = nameof(@StudentGroupMappingRegister.MapModelToEntity))]
    public static partial void UpdateEntityWithModel(Student? model, DbStudent? entity);
}