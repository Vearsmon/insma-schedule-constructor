using Domain.Models.RegistryItemModels;
using Domain.Models.RegistrySearchModels;

namespace Dal.RegistryRepositories.Lesson;

public interface ILessonRegistryRepository : IRegistryRepository<LessonRegistryItem, LessonRegistryInternalSearchModel>
{
}