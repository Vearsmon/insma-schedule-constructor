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
        // return await base.SearchAsync(predicateBuilder, searchModel);
        var entities = await Query()
            .AsNoTracking()
            .Where(predicateBuilder.Build(searchModel))
            .ToArrayAsync();

        return entities.Select(x => MapperReadonly.Map(x)).ToArray();
    }

    protected override IQueryable<DbLesson> Query()
    {
        return Context.Set<DbLesson>()
            .Include(x => x.Room)
            .Include(x => x.StudentGroup)
            .Include(x => x.AcademicDiscipline)
            .Include(x => x.Teacher)
            .Include(x => x.ValidationMessages);
    }
}