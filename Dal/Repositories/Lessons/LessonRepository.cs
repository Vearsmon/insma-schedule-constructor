using Dal.Entities;
using Dal.Transactions;
using Domain.Models;
using Domain.Models.SearchModels;
using Microsoft.EntityFrameworkCore;

namespace Dal.Repositories.Lessons;

public class LessonRepository(
    InsmaScheduleContext context,
    IRepositoryMapper<DbLesson, Lesson> mapper,
    ITransactionalService transactionalService,
    IPredicateBuilder<DbLesson, LessonSearchModel> predicateBuilder)
    : Repository<InsmaScheduleContext, DbLesson, Lesson>(context, mapper, transactionalService), ILessonRepository
{
    public async Task<Lesson[]> SearchAsync(LessonSearchModel searchModel)
    {
        return await base.SearchAsync(predicateBuilder, searchModel);
    }

    public override async Task<Guid> SaveAsync(Lesson model, CancellationToken cancellationToken = default)
    {
        var previousLesson = model.Id.HasValue ? await GetAsync(model.Id!.Value, cancellationToken) : null;
        if (previousLesson != null)
        {
            await Context.Set<DbLessonPolicyViolation>().Where(x => x.LessonId == previousLesson.Id).ExecuteDeleteAsync(cancellationToken);
            await Context.SaveChangesAsync(cancellationToken);
        }
        return await base.SaveAsync(model, cancellationToken);
    }

    protected override IQueryable<DbLesson> Query()
    {
        return Context.Set<DbLesson>()
            .Include(x => x.AcademicDiscipline)
            .Include(x => x.StudentGroups)
            .ThenInclude(x => x.StudentGroup)
            .Include(x => x.Teachers)
            .ThenInclude(x => x.Teacher)
            .Include(x => x.Rooms)
            .ThenInclude(x => x.Room)
            .Include(x => x.LessonBatchInfo)
            .Include(x => x.Violations);
    }
}