using System.Text;
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
            model.Id = id;
            var saveExpression = BuildSaveReferencesExpression(model);
            if (!string.IsNullOrEmpty(saveExpression)) await Context.Database.ExecuteSqlRawAsync(saveExpression, cancellationToken);
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

        var deleteExpression = BuildDeleteReferencesExpression(id!.Value, removedParents.Concat(removedChildren).ToArray());
        if (!string.IsNullOrEmpty(deleteExpression)) await Context.Database.ExecuteSqlRawAsync(deleteExpression, cancellationToken);
        await base.SaveAsync(model, cancellationToken);
        var saveReferencesExpression = BuildSaveReferencesExpression(model);
        if (!string.IsNullOrEmpty(saveReferencesExpression)) await Context.Database.ExecuteSqlRawAsync(saveReferencesExpression, cancellationToken);

        return id.Value;
    }

    public override async Task<Guid[]> SaveAllAsync(StudentGroup[] models, CancellationToken cancellationToken = default)
    {
        var result = new List<Guid>();

        var previousStudentGroupsById = (await SelectAsync(models
                .Where(x => x.Id.HasValue).Select(x => x.Id!.Value).ToArray(), cancellationToken))
            .ToDictionary(x => x.Id!.Value);

        var toDeleteIds = new List<Guid>();
        var saveExpressions = new List<string>();
        var deleteExpressions = new List<string>();

        foreach (var model in models)
        {
            var id = model.Id;
            var previousStudentGroup = id.HasValue ? previousStudentGroupsById[id.Value] : null;
            if (previousStudentGroup == null)
            {
                id = await base.SaveAsync(model, cancellationToken);
                model.Id = id;
                var saveExpression = BuildSaveReferencesExpression(model);
                if (!string.IsNullOrEmpty(saveExpression)) saveExpressions.Add(saveExpression);
                result.Add(id.Value);
                continue;
            }

            var removedChildren = previousStudentGroup.Children
                .Where(x => model.Children.All(y => y.Id != x.Id))
                .ToArray();
            var removedParents = previousStudentGroup.Parents
                .Where(x => model.Parents.All(y => y.Id != x.Id));

            if (model.StudentGroupType == StudentGroupType.Group)
            {
                toDeleteIds.AddRange(removedChildren.Select(x => x.Id!.Value));
            }

            var deleteExpression = BuildDeleteReferencesExpression(id!.Value, removedParents.Concat(removedChildren).ToArray());
            if (!string.IsNullOrEmpty(deleteExpression)) deleteExpressions.Add(deleteExpression);
            var saveReferencesExpression = BuildSaveReferencesExpression(model);
            if (!string.IsNullOrEmpty(saveReferencesExpression)) saveExpressions.Add(saveReferencesExpression);

            result.Add(id.Value);
        }

        await DeleteAsync(toDeleteIds.ToArray(), cancellationToken);
        if (deleteExpressions.Count > 0)
        {
            await Context.Database.ExecuteSqlRawAsync(string.Join("\n", deleteExpressions), cancellationToken);
        }
        await base.SaveAllAsync(models.Where(x => x.Id!.HasValue).ToArray(), cancellationToken);
        if (saveExpressions.Count > 0)
        {
            await Context.Database.ExecuteSqlRawAsync(string.Join("\n", saveExpressions), cancellationToken);
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
        .ThenInclude(x => x.Parents)
        .Include(x => x.Children)
        .ThenInclude(x => x.Children);

    private string? BuildSaveReferencesExpression(StudentGroup model)
    {
        var stringBuilder = new StringBuilder();
        foreach (var parent in model.Parents)
        {
            stringBuilder.AppendLine(
                $"""
                 INSERT INTO public.student_group_link (parent_id, child_id)
                 VALUES ('{parent.Id!.Value}', '{model.Id!.Value}')
                 ON CONFLICT (parent_id, child_id) DO NOTHING;
                 """);
        }

        return stringBuilder.Length > 0 ? stringBuilder.ToString() : null;
    }

    private string? BuildDeleteReferencesExpression(Guid modelId, StudentGroup[] modelReferences)
    {
        var stringBuilder = new StringBuilder();
        foreach (var parent in modelReferences)
        {
            stringBuilder.AppendLine(
                $"""
                 DELETE FROM public.student_group_link
                 WHERE (parent_id = '{modelId}' AND child_id = '{parent.Id!.Value}')
                 OR (child_id = '{modelId}' AND parent_id = '{parent.Id!.Value}');
                 """);
        }

        return stringBuilder.Length > 0 ? stringBuilder.ToString() : null;
    }
}