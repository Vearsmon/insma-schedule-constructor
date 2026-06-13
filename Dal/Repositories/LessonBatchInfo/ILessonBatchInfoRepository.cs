using Domain.Models.SearchModels;

namespace Dal.Repositories.LessonBatchInfo;

public interface ILessonBatchInfoRepository : IRepository<Domain.Models.LessonBatchInfo>
{
    Task<Domain.Models.LessonBatchInfo[]> SearchAsync(LessonBatchInfoSearchModel searchModel);

    Task<Domain.Models.LessonBatchInfo[]> SearchConflictsAsync(LessonBatchInfoConflictsSearchModel searchModel);
}