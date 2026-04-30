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
        var predicate = predicateBuilder.Build(searchModel);

        var entities = await Query()
            .AsNoTracking()
            .Include(x => x.Parents)
            .ThenInclude(x => x!.Parents)
            .Include(x => x.Children)
            .ThenInclude(x => x!.Children)
            .Where(predicate)
            .ToArrayAsync();

        return entities.Select(x => MapperReadonly.Map(x)).ToArray();
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return (await base.SelectAsync([id])).Length == 1;
    }

    public async Task<Dictionary<Guid, List<Guid>>> GetStudentGroupTreeIdsAsync(Guid[] studentGroupIds)
    {
        var studentGroupTrees = await Query()
            .AsNoTracking()
            .Include(x => x.Parents)
            .ThenInclude(x => x!.Parents)
            .Include(x => x.Children)
            .ThenInclude(x => x!.Children)
            .Where(x => studentGroupIds.Contains(x.Id))
            .ToArrayAsync();

        var result = new Dictionary<Guid, List<Guid>>();
        foreach (var studentGroup in studentGroupTrees)
        {
            result[studentGroup.Id] = [studentGroup.Id];
            result[studentGroup.Id].AddRange(studentGroup.Parents.Select(x => x.Id));
            result[studentGroup.Id].AddRange(studentGroup.Children.Select(x => x.Id));
        }
        return result;
    }
}