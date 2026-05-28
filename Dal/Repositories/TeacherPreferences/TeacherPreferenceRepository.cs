using Dal.Entities;
using Dal.Transactions;
using Domain.Models;
using Domain.Models.SearchModels;
using Microsoft.EntityFrameworkCore;

namespace Dal.Repositories.TeacherPreferences;

public class TeacherPreferenceRepository(
    InsmaScheduleContext context,
    IRepositoryMapper<DbTeacherPreference, TeacherPreference> mapper,
    ITransactionalService transactionalService,
    IPredicateBuilder<DbTeacherPreference, TeacherPreferenceSearchModel> predicateBuilder,
    IPredicateBuilder<DbTeacherPreference, TeacherPreferenceConflictsSearchModel> conflictsPredicateBuilder)
    : Repository<InsmaScheduleContext, DbTeacherPreference, TeacherPreference>(context, mapper, transactionalService), ITeacherPreferenceRepository
{
    public async Task<TeacherPreference[]> SearchAsync(TeacherPreferenceSearchModel searchModel)
    {
        return await base.SearchAsync(predicateBuilder, searchModel);
    }

    public async Task<TeacherPreference[]> SearchConflictsAsync(TeacherPreferenceConflictsSearchModel searchModel)
    {
        return await base.SearchAsync(conflictsPredicateBuilder, searchModel);
    }

    protected override IQueryable<DbTeacherPreference> Query() => Context.Set<DbTeacherPreference>()
        .Include(x => x.Schedule)
        .Include(x => x.Teacher)
        .Include(x => x.Room)
        .ThenInclude(x => x.Campus);
}