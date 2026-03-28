using Dal.Entities;
using Dal.Transactions;
using Domain.Models;
using Domain.Models.SearchModels;
using Microsoft.EntityFrameworkCore;

namespace Dal.Repositories.StudentGroups;

public class StudentGroupRepository(
    InsmaScheduleContext context,
    IRepositoryMapper<DbStudentGroup, StudentGroup> mapper,
    ITransactionalService transactionalService,
    IPredicateBuilder<DbStudentGroup, StudentGroupSearchModel> predicateBuilder)
    : Repository<InsmaScheduleContext, DbStudentGroup, StudentGroup>(context, mapper, transactionalService), IStudentGroupRepository
{
    public async Task<StudentGroup[]> SearchAsync(StudentGroupSearchModel searchModel)
    {
        return await base.SearchAsync(predicateBuilder, searchModel);
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return (await base.SelectAsync([id])).Length == 1;
    }

    public async Task<Guid[]> GetStudentGroupTreeIdsAsync(Guid studentGroupId)
    {
        var studentGroupTreeFlat = await Query()
            .AsNoTracking()
            .Include(x => x.Parent)
            .ThenInclude(x => x!.Parent)
            .Include(x => x.Children)
            .ThenInclude(x => x!.Children)
            .Where(x => x.Id == studentGroupId)
            .ToArrayAsync();

        return studentGroupTreeFlat.Select(x => x.Id).ToArray();
    }
}