using System.Diagnostics.CodeAnalysis;
using Dal.Entities;
using Dal.Mapping;
using Dal.Repositories;
using Domain.Models.RegistryItemModels;

namespace Dal.RegistryRepositories.Lesson;

public class LessonRegistryMapper : IReadonlyRepositoryMapper<DbLesson, LessonRegistryItem>
{
    [return: NotNullIfNotNull("entity")]
    public LessonRegistryItem? Map(DbLesson? entity) => MappingRegister.MapRegistryItem(entity);
}