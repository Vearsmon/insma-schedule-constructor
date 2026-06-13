using System.Text;
using Dal.Entities;
using Dal.Transactions;
using Domain.Models;
using Domain.Models.SearchModels;
using Microsoft.EntityFrameworkCore;

namespace Dal.Repositories.LessonBatchInfo;

public class LessonBatchInfoRepository(
    InsmaScheduleContext context,
    IRepositoryMapper<DbLessonBatchInfo, Domain.Models.LessonBatchInfo> mapper,
    ITransactionalService transactionalService,
    IPredicateBuilder<DbLessonBatchInfo, LessonBatchInfoSearchModel> predicateBuilder,
    IPredicateBuilder<DbLessonBatchInfo, LessonBatchInfoConflictsSearchModel> conflictsPredicateBuilder)
    : Repository<InsmaScheduleContext, DbLessonBatchInfo, Domain.Models.LessonBatchInfo>(context, mapper, transactionalService), ILessonBatchInfoRepository
{
    public async Task<Domain.Models.LessonBatchInfo[]> SearchAsync(LessonBatchInfoSearchModel searchModel)
    {
        return await base.SearchAsync(predicateBuilder, searchModel);
    }

    public async Task<Domain.Models.LessonBatchInfo[]> SearchConflictsAsync(LessonBatchInfoConflictsSearchModel searchModel)
    {
        return await base.SearchAsync(conflictsPredicateBuilder, searchModel);
    }

    public override async Task<Guid> SaveAsync(Domain.Models.LessonBatchInfo model, CancellationToken cancellationToken = default)
    {
        var id = model.Id;
        var previousLessonBatchInfo = id.HasValue ? await GetAsync(id.Value, cancellationToken) : null;
        if (previousLessonBatchInfo == null)
        {
            id = await base.SaveAsync(model, cancellationToken);
            model.Id = id;
            var saveExpression = BuildSaveReferencesExpression(model);
            if (!string.IsNullOrEmpty(saveExpression)) await Context.Database.ExecuteSqlRawAsync(saveExpression, cancellationToken);
            return id.Value;
        }

        var removedStudentGroups = previousLessonBatchInfo.StudentGroups
            .Where(x => model.StudentGroups.All(y => y.Id != x.Id))
            .ToArray();
        var removedTeachers = previousLessonBatchInfo.Teachers
            .Where(x => model.Teachers.All(y => y.Id != x.Id))
            .ToArray();
        var removedRooms = previousLessonBatchInfo.Rooms
            .Where(x => model.Rooms.All(y => y.Id != x.Id))
            .ToArray();

        var deleteExpression = BuildDeleteReferencesExpression(id!.Value, removedStudentGroups, removedTeachers, removedRooms);
        if (!string.IsNullOrEmpty(deleteExpression)) await Context.Database.ExecuteSqlRawAsync(deleteExpression, cancellationToken);
        await base.SaveAsync(model, cancellationToken);
        var saveReferencesExpression = BuildSaveReferencesExpression(model);
        if (!string.IsNullOrEmpty(saveReferencesExpression)) await Context.Database.ExecuteSqlRawAsync(saveReferencesExpression, cancellationToken);

        return id.Value;
    }

    public override async Task<Guid[]> SaveAllAsync(Domain.Models.LessonBatchInfo[] models, CancellationToken cancellationToken = default)
    {
        var result = new List<Guid>();

        var previousLessonBatchInfosById = (await SelectAsync(models
                .Where(x => x.Id.HasValue).Select(x => x.Id!.Value).ToArray(), cancellationToken))
            .ToDictionary(x => x.Id!.Value);

        var saveExpressions = new List<string>();
        var deleteExpressions = new List<string>();

        foreach (var model in models)
        {
            var id = model.Id;
            var previousLessonBatchInfo = id.HasValue ? previousLessonBatchInfosById[id.Value] : null;
            if (previousLessonBatchInfo == null)
            {
                id = await base.SaveAsync(model, cancellationToken);
                model.Id = id;
                var saveExpression = BuildSaveReferencesExpression(model);
                if (!string.IsNullOrEmpty(saveExpression)) saveExpressions.Add(saveExpression);
                result.Add(id.Value);
                continue;
            }

            var removedStudentGroups = previousLessonBatchInfo.StudentGroups
                .Where(x => model.StudentGroups.All(y => y.Id != x.Id))
                .ToArray();
            var removedTeachers = previousLessonBatchInfo.Teachers
                .Where(x => model.Teachers.All(y => y.Id != x.Id))
                .ToArray();
            var removedRooms = previousLessonBatchInfo.Rooms
                .Where(x => model.Rooms.All(y => y.Id != x.Id))
                .ToArray();

            var deleteExpression = BuildDeleteReferencesExpression(id!.Value, removedStudentGroups, removedTeachers, removedRooms);
            if (!string.IsNullOrEmpty(deleteExpression)) deleteExpressions.Add(deleteExpression);
            var saveReferencesExpression = BuildSaveReferencesExpression(model);
            if (!string.IsNullOrEmpty(saveReferencesExpression)) saveExpressions.Add(saveReferencesExpression);

            result.Add(id.Value);
        }

        if (deleteExpressions.Count > 0)
        {
            await Context.Database.ExecuteSqlRawAsync(string.Join("\n", deleteExpressions), cancellationToken);
        }
        await base.SaveAllAsync(models.Where(x => x.Id.HasValue).ToArray(), cancellationToken);
        if (saveExpressions.Count > 0)
        {
            await Context.Database.ExecuteSqlRawAsync(string.Join("\n", saveExpressions), cancellationToken);
        }

        return result.ToArray();
    }

    protected override IQueryable<DbLessonBatchInfo> Query() => Context.Set<DbLessonBatchInfo>()
        .Include(x => x.StudentGroups)
        .Include(x => x.Teachers)
        .Include(x => x.Rooms)
        .ThenInclude(x => x.Campus)
        .Include(x => x.DayOfWeekTimeIntervals);

    private string? BuildSaveReferencesExpression(Domain.Models.LessonBatchInfo model)
    {
        var stringBuilder = new StringBuilder();
        foreach (var studentGroup in model.StudentGroups)
        {
            stringBuilder.AppendLine(
                $"""
                 INSERT INTO public.lesson_batch_info_student_group (lesson_batch_info_id, student_group_id)
                 VALUES ('{model.Id!.Value}', '{studentGroup.Id!.Value}')
                 ON CONFLICT (lesson_batch_info_id, student_group_id) DO NOTHING;
                 """);
        }
        foreach (var teacher in model.Teachers)
        {
            stringBuilder.AppendLine(
                $"""
                 INSERT INTO public.lesson_batch_info_teacher (lesson_batch_info_id, teacher_id)
                 VALUES ('{model.Id!.Value}', '{teacher.Id!.Value}')
                 ON CONFLICT (lesson_batch_info_id, teacher_id) DO NOTHING;
                 """);
        }
        foreach (var room in model.Rooms)
        {
            stringBuilder.AppendLine(
                $"""
                 INSERT INTO public.lesson_batch_info_room (lesson_batch_info_id, room_id)
                 VALUES ('{model.Id!.Value}', '{room.Id!.Value}')
                 ON CONFLICT (lesson_batch_info_id, room_id) DO NOTHING;
                 """);
        }

        return stringBuilder.Length > 0 ? stringBuilder.ToString() : null;
    }

    private string? BuildDeleteReferencesExpression(Guid modelId,
        StudentGroup[] studentGroups, Teacher[] teachers, Room[] rooms)
    {
        var stringBuilder = new StringBuilder();
        foreach (var studentGroup in studentGroups)
        {
            stringBuilder.AppendLine(
                $"""
                 DELETE FROM public.lesson_batch_info_student_group
                 WHERE (lesson_batch_info_id = '{modelId}' AND student_group_id = '{studentGroup.Id!.Value}');
                 """);
        }
        foreach (var teacher in teachers)
        {
            stringBuilder.AppendLine(
                $"""
                 DELETE FROM public.lesson_batch_info_teacher
                 WHERE (lesson_batch_info_id = '{modelId}' AND teacher_id = '{teacher.Id!.Value}');
                 """);
        }
        foreach (var room in rooms)
        {
            stringBuilder.AppendLine(
                $"""
                 DELETE FROM public.lesson_batch_info_room
                 WHERE (lesson_batch_info_id = '{modelId}' AND room_id = '{room.Id!.Value}');
                 """);
        }

        return stringBuilder.Length > 0 ? stringBuilder.ToString() : null;
    }
}