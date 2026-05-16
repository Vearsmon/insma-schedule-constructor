using Dal.Entities;
using Dal.Transactions;
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
        var id = model.Id;
        var previousStudentGroup = id.HasValue ? await GetAsync(id.Value, cancellationToken) : null;
        if (previousStudentGroup == null)
        {
            id = await base.SaveAsync(model, cancellationToken);
            await SaveReferencesAsync(id.Value, model.Parents);
            return id.Value;
        }

        var removedChildren = previousStudentGroup.Children
            .Where(x => model.Children.All(y => y.Id != x.Id))
            .ToArray();
        var removedParents = previousStudentGroup.Parents
            .Where(x => model.Parents.All(y => y.Id != x.Id));

        if (model.StudentGroupType == StudentGroupType.Group)
        {
            await DeleteAsync(removedChildren.Select(x => x.Id!.Value).ToArray(), cancellationToken);
        }

        await DeleteReferencesAsync(model.Id!.Value, removedParents.Concat(removedChildren).ToArray());
        await base.SaveAsync(model, cancellationToken);
        await SaveReferencesAsync(id!.Value, model.Parents);

        return id.Value;
    }

    public override async Task<Guid[]> SaveAllAsync(StudentGroup[] models, CancellationToken cancellationToken = default)
    {
        var result = new List<Guid>();
        foreach (var model in models)
        {
            var id = await SaveAsync(model, cancellationToken);
            result.Add(id);
        }
        return result.ToArray();
    }

    public async Task<StudentGroup[]> SearchAsync(StudentGroupSearchModel searchModel)
    {
        return await base.SearchAsync(predicateBuilder, searchModel);
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await ExistAsync(predicateBuilder, new StudentGroupSearchModel { Id = id });
    }

    public async Task<Dictionary<Guid, List<Guid>>> GetStudentGroupTreeIdsAsync(Guid[] studentGroupIds)
    {
        var studentGroupTrees = await SelectAsync(studentGroupIds);

        var result = new Dictionary<Guid, List<Guid>>();
        foreach (var studentGroup in studentGroupTrees)
        {
            result[studentGroup.Id!.Value] = [studentGroup.Id!.Value];
            result[studentGroup.Id!.Value].AddRange(studentGroup.Parents.Select(x => x.Id!.Value));
            result[studentGroup.Id!.Value].AddRange(studentGroup.Children.Select(x => x.Id!.Value));
        }
        return result;
    }

    protected override IQueryable<DbStudentGroup> Query() => Context.Set<DbStudentGroup>()
        .Include(x => x.Parents)
        .Include(x => x.Children);

    private async Task SaveReferencesAsync(Guid modelId, StudentGroup[] modelParents)
    {
        foreach (var parent in modelParents)
        {
            await Context.Database.ExecuteSqlInterpolatedAsync(
                $"""
                 INSERT INTO public.student_group_link (parent_id, child_id)
                 VALUES ({parent.Id!.Value}, {modelId})
                 ON CONFLICT (parent_id, child_id) DO NOTHING
                 """);
        }
    }

    private async Task DeleteReferencesAsync(Guid modelId, StudentGroup[] modelReferences)
    {
        foreach (var parent in modelReferences)
        {
            await Context.Database.ExecuteSqlInterpolatedAsync(
                $"""
                 DELETE FROM public.student_group_link
                 WHERE (parent_id = {modelId} AND child_id = {parent.Id!.Value})
                 OR (child_id = {modelId} AND parent_id = {parent.Id!.Value})
                 """);
        }
    }
}