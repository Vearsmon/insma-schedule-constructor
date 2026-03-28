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
    }
}