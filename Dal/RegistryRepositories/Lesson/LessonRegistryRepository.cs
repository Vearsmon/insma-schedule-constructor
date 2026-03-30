using Dal.Entities;
using Dal.Repositories;
using Domain.Models.RegistryItemModels;
using Domain.Models.RegistrySearchModels;

namespace Dal.RegistryRepositories.Lesson;

internal class LessonRegistryRepository(
    InsmaScheduleContext context,
    IReadonlyRepositoryMapper<DbLesson, LessonRegistryItem> mapper,
    IRegistryRepositoryOrderer<DbLesson, LessonRegistryInternalSearchModel> orderer,
    IPredicateBuilder<DbLesson, LessonRegistryInternalSearchModel> predicateBuilder)
    : ReadonlyRegistryRepository<InsmaScheduleContext, DbLesson, LessonRegistryItem,
            LessonRegistryInternalSearchModel>(context, mapper, orderer, predicateBuilder),
        ILessonRegistryRepository
{
}