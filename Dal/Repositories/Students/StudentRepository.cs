using Dal.Entities;
using Dal.Transactions;
using Domain.Models;
using Domain.Models.SearchModels;

namespace Dal.Repositories.Students;

public class StudentRepository(
    InsmaScheduleContext context,
    IRepositoryMapper<DbStudent, Student> mapper,
    ITransactionalService transactionalService,
    IPredicateBuilder<DbStudent, StudentSearchModel> predicateBuilder)
    : Repository<InsmaScheduleContext, DbStudent, Student>(context, mapper, transactionalService), IStudentRepository
{
    public async Task<Student[]> SearchAsync(StudentSearchModel searchModel)
    {
        return await base.SearchAsync(predicateBuilder, searchModel);
    }
}