using Domain.Models;
using Domain.Models.SearchModels;

namespace Dal.Repositories.Lessons;

public interface ILessonRepository : IRepository<Lesson>
{
    Task<Lesson[]> SearchAsync(LessonSearchModel searchModel);
}