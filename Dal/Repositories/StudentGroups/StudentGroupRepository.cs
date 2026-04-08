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

    public async Task<Dictionary<Guid, List<Guid>>> GetStudentGroupTreeIdsAsync(Guid[] studentGroupIds)
    {
        var studentGroupTrees = await Query()
            .AsNoTracking()
            .Include(x => x.Parent)
            .ThenInclude(x => x!.Parent)
            .Include(x => x.Children)
            .ThenInclude(x => x!.Children)
            .Where(x => studentGroupIds.Contains(x.Id))
            .ToArrayAsync();

        var result = new Dictionary<Guid, List<Guid>>();
        foreach (var studentGroup in studentGroupTrees)
        {
            result[studentGroup.Id] = [studentGroup.Id];
            if (studentGroup.Parent != null)
            {
                result[studentGroup.Id].Add(studentGroup.Parent.Id);
            }

            if (studentGroup.Children.Count > 0)
            {
                result[studentGroup.Id].AddRange(studentGroup.Children.Select(x => x.Id));
            }
        }
        return result;
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
}