using Dal.Entities;
using Domain.Models.RegistrySearchModels;

namespace Dal.RegistryRepositories.Lesson;

public class LessonRegistryOrderer : IRegistryRepositoryOrderer<DbLesson, LessonRegistryInternalSearchModel>
{
    public IQueryable<DbLesson> Order(IQueryable<DbLesson> queryable, LessonRegistryInternalSearchModel searchModel)
    {
        return queryable;
    }
}