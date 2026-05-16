using Dal.Entities;
using Dal.Transactions;
using Domain.Models;
using Domain.Models.SearchModels;
using Microsoft.EntityFrameworkCore;

namespace Dal.Repositories.AcademicDisciplines;

public class AcademicDisciplineRepository(
    InsmaScheduleContext context,
    IRepositoryMapper<DbAcademicDiscipline, AcademicDiscipline> mapper,
    ITransactionalService transactionalService,
    IPredicateBuilder<DbAcademicDiscipline, AcademicDisciplineSearchModel> predicateBuilder)
    : Repository<InsmaScheduleContext, DbAcademicDiscipline, AcademicDiscipline>(context, mapper, transactionalService), IAcademicDisciplineRepository
{
    public async Task<AcademicDiscipline[]> SearchAsync(AcademicDisciplineSearchModel searchModel)
    {
        return await base.SearchAsync(predicateBuilder, searchModel);
    }

    public override async Task<Guid> SaveAsync(AcademicDiscipline model, CancellationToken cancellationToken = default)
    {
        var id = model.Id;
        var previousAcademicDiscipline = id.HasValue ? await GetAsync(id.Value, cancellationToken) : null;
        if (previousAcademicDiscipline == null)
        {
            id = await base.SaveAsync(model, cancellationToken);
            return id.Value;
        }

        var currentLessonBatchInfos = model.GetAllBatchInfos();
        var removedLessonBatchInfoIds = previousAcademicDiscipline.GetAllBatchInfos()
            .Where(x => currentLessonBatchInfos.All(y => y.Id != x.Id))
            .Select(x => x.Id!.Value)
            .ToArray();

        await Context.Set<DbLessonPolicyViolation>().Where(x => removedLessonBatchInfoIds.Contains(x.Id)).ExecuteDeleteAsync(cancellationToken);

        await base.SaveAsync(model, cancellationToken);

        return id!.Value;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await ExistAsync(predicateBuilder, new AcademicDisciplineSearchModel { Id = id });
    }

    protected override IQueryable<DbAcademicDiscipline> Query() => Context.Set<DbAcademicDiscipline>()
        .Include(x => x.Schedule)
        .Include(x => x.LessonBatchInfos)
        .ThenInclude(x => x.StudentGroups)
        .Include(x => x.LessonBatchInfos)
        .ThenInclude(x => x.Teachers)
        .Include(x => x.LessonBatchInfos)
        .ThenInclude(x => x.Rooms);
}