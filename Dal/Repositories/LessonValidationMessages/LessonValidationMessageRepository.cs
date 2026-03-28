using Dal.Entities;
using Dal.Transactions;
using Domain.Models;
using Domain.Models.SearchModels;
using Microsoft.EntityFrameworkCore;

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
        // return await base.SearchAsync(predicateBuilder, searchModel);
        var entities = await Query()
            .AsNoTracking()
            .Where(predicateBuilder.Build(searchModel))
            .ToArrayAsync();

        return entities.Select(x => MapperReadonly.Map(x)).ToArray();
    }
}