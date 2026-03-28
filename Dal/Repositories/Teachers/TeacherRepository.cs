using Dal.Entities;
using Dal.Transactions;
using Domain.Models;
using Domain.Models.SearchModels;

namespace Dal.Repositories.Teachers;

public class TeacherRepository(
    InsmaScheduleContext context,
    IRepositoryMapper<DbTeacher, Teacher> mapper,
    ITransactionalService transactionalService,
    IPredicateBuilder<DbTeacher, TeacherSearchModel> predicateBuilder)
    : Repository<InsmaScheduleContext, DbTeacher, Teacher>(context, mapper, transactionalService), ITeacherRepository
{
    public async Task<Teacher[]> SearchAsync(TeacherSearchModel searchModel)
    {
        return await base.SearchAsync(predicateBuilder, searchModel);
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return (await base.SelectAsync([id])).Length == 1;
    }
}