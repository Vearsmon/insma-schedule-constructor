using Dal.Entities;
using Dal.Transactions;
using Domain.Exceptions;
using Domain.Models;
using Domain.Models.Enums;
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
    public override async Task<Guid> SaveAsync(StudentGroup model, CancellationToken cancellationToken = default)
    {
        var previousStudentGroup = model.Id.HasValue ? await GetAsync(model.Id!.Value, cancellationToken) : null;
        if (previousStudentGroup != null)
        {
            var removedChildIds = previousStudentGroup.Children
                .Where(x => model.Children.All(y => y.Id != x.Id))
                .Select(x => x.Id!.Value);
            var removedParentIds = previousStudentGroup.Parents
                .Where(x => model.Parents.All(y => y.Id != x.Id))
                .Select(x => x.Id!.Value);
            var referencesToDelete = removedChildIds
                .Select(x => new DbStudentGroupLink { ParentStudentGroupId = model.Id!.Value, ChildStudentGroupId = x })
                .Concat(removedParentIds
                    .Select(x => new DbStudentGroupLink { ParentStudentGroupId = x, ChildStudentGroupId = model.Id!.Value }))
                .ToArray();
            Context.Set<DbStudentGroupLink>().RemoveRange(referencesToDelete);
            await Context.SaveChangesAsync(cancellationToken);

            model.Children = model.Children.Where(x => previousStudentGroup.Children.All(y => y.Id != x.Id)).ToArray();
            model.Parents = model.Parents.Where(x => previousStudentGroup.Parents.All(y => y.Id != x.Id)).ToArray();
        }
        return await base.SaveAsync(model, cancellationToken);
    }

    public override async Task<StudentGroup> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await Query().AsNoTracking()
            .AsNoTracking()
            .Include(x => x.Parents)
            .ThenInclude(x => x!.ParentStudentGroup)
            .Include(x => x.Children)
            .ThenInclude(x => x!.ChildStudentGroup)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        var foundEntity = entity ?? throw new ServiceException(ServiceExceptionTypes.EntityNotFound);
        return MapperReadonly.Map(foundEntity);
    }

    public async Task<StudentGroup[]> SearchAsync(StudentGroupSearchModel searchModel)
    {
        var predicate = predicateBuilder.Build(searchModel);

        var entities = await Query()
            .AsNoTracking()
            .Include(x => x.Parents)
            .ThenInclude(x => x!.ParentStudentGroup)
            .Include(x => x.Children)
            .ThenInclude(x => x!.ChildStudentGroup)
            .Where(predicate)
            .ToArrayAsync();

        return entities.Select(x => MapperReadonly.Map(x)).ToArray();
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await ExistAsync(predicateBuilder, new StudentGroupSearchModel { Id = id });
    }

    public async Task<Dictionary<Guid, List<Guid>>> GetStudentGroupTreeIdsAsync(Guid[] studentGroupIds)
    {
        var studentGroupTrees = await Query()
            .AsNoTracking()
            .Include(x => x.Parents)
            .ThenInclude(x => x!.ParentStudentGroup)
            .Include(x => x.Children)
            .ThenInclude(x => x!.ChildStudentGroup)
            .Where(x => studentGroupIds.Contains(x.Id))
            .ToArrayAsync();

        var result = new Dictionary<Guid, List<Guid>>();
        foreach (var studentGroup in studentGroupTrees)
        {
            result[studentGroup.Id] = [studentGroup.Id];
            result[studentGroup.Id].AddRange(studentGroup.Parents.Select(x => x.ParentStudentGroupId));
            result[studentGroup.Id].AddRange(studentGroup.Children.Select(x => x.ChildStudentGroupId));
        }
        return result;
    }
}