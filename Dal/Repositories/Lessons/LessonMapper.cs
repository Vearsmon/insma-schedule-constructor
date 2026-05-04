using System.Diagnostics.CodeAnalysis;
using Dal.Entities;
using Dal.Mapping;
using Domain.Models;

namespace Dal.Repositories.Lessons;

public class LessonMapper : IRepositoryMapper<DbLesson, Lesson>
{
    [return: NotNullIfNotNull("entity")]
    public Lesson? Map(DbLesson? entity)
    {
        return MappingRegister.Map(entity);
    }

    public void Update(DbLesson entity, Lesson model)
    {
        MappingRegister.Update(model, entity);
        model.Id ??= Guid.Empty;
        entity.StudentGroups = model.StudentGroups.Select(x => new DbLessonStudentGroup { LessonId = model.Id!.Value, StudentGroupId = x.Id!.Value }).ToList();
        entity.Teachers = model.Teachers.Select(x => new DbLessonTeacher { LessonId = model.Id!.Value, TeacherId = x.Id!.Value }).ToList();
        entity.Rooms = model.Rooms.Select(x => new DbLessonRoom { LessonId = model.Id!.Value, RoomId = x.Id!.Value }).ToList();
    }
}