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

    public async Task<string[]> SearchCyphersAsync(Guid scheduleId)
    {
        return await Query()
            .AsNoTracking()
            .Where(x => x.ScheduleId == scheduleId)
            .Select(x => x.Cypher)
            .Distinct()
            .ToArrayAsync();
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return (await base.SelectAsync([id])).Length == 1;
    }
}