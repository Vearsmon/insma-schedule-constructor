using Domain.Models;
using Domain.Models.SearchModels;

namespace Dal.Repositories.LessonValidationMessages;

public interface ILessonValidationMessageRepository : IRepository<LessonValidationMessage>
{
    Task<LessonValidationMessage[]> SearchAsync(LessonValidationMessageSearchModel searchModel);
}