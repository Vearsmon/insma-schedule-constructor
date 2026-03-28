using Dal.Entities;
using Dal.Transactions;
using Domain.Models;
using Domain.Models.SearchModels;

namespace Dal.Repositories.TeacherPreferences;

public class TeacherPreferenceRepository(
    InsmaScheduleContext context,
    IRepositoryMapper<DbTeacherPreference, TeacherPreference> mapper,
    ITransactionalService transactionalService,
    IPredicateBuilder<DbTeacherPreference, TeacherPreferenceSearchModel> predicateBuilder)
    : Repository<InsmaScheduleContext, DbTeacherPreference, TeacherPreference>(context, mapper, transactionalService), ITeacherPreferenceRepository
{
    public async Task<TeacherPreference[]> SearchAsync(TeacherPreferenceSearchModel searchModel)
    {
        var teacherPreferences = await base.SearchAsync(predicateBuilder, searchModel);
        if (searchModel.TimeInterval != null)
        {
            return teacherPreferences
                // .Where(f => f.TimeFrom <= searchModel.TimeInterval.TimeTo && f.TimeTo >= searchModel.TimeInterval.TimeFrom)
                .ToArray();
        }

        return teacherPreferences;
    }
}