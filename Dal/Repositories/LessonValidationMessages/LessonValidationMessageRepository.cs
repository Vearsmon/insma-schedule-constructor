using Dal.Entities;
using Dal.Transactions;
using Domain.Models;
using Domain.Models.SearchModels;

namespace Dal.Repositories.LessonValidationMessages;

public class LessonValidationMessageRepository(
    InsmaScheduleContext context,
    IRepositoryMapper<DbLessonValidationMessage, LessonValidationMessage> mapper,
    ITransactionalService transactionalService,
    IPredicateBuilder<DbLessonValidationMessage, LessonValidationMessageSearchModel> predicateBuilder)
    : Repository<InsmaScheduleContext, DbLessonValidationMessage, LessonValidationMessage>(context, mapper, transactionalService), ILessonValidationMessageRepository
{
    public async Task<LessonValidationMessage[]> SearchAsync(LessonValidationMessageSearchModel searchModel)
    {
        return await base.SearchAsync(predicateBuilder, searchModel);
    }
}