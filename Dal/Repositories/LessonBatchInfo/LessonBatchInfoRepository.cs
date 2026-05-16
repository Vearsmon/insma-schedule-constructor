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
    IPredicateBuilder<DbLessonBatchInfo, LessonBatchInfoSearchModel> predicateBuilder)
    : Repository<InsmaScheduleContext, DbLessonBatchInfo, Domain.Models.LessonBatchInfo>(context, mapper, transactionalService), ILessonBatchInfoRepository
{
    public override async Task<Guid> SaveAsync(Domain.Models.LessonBatchInfo model, CancellationToken cancellationToken = default)
    {
        var id = model.Id;
        var previousLessonBatchInfo = id.HasValue ? await GetAsync(id.Value, cancellationToken) : null;
        if (previousLessonBatchInfo == null)
        {
            id = await base.SaveAsync(model, cancellationToken);
            model.Id = id;
            await SaveReferencesAsync(model);
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

        await DeleteReferencesAsync(id!.Value, removedStudentGroups, removedTeachers, removedRooms);
        await base.SaveAsync(model, cancellationToken);
        await SaveReferencesAsync(model);

        return id.Value;
    }

    public override async Task<Guid[]> SaveAllAsync(Domain.Models.LessonBatchInfo[] models, CancellationToken cancellationToken = default)
    {
        var result = new List<Guid>();
        foreach (var model in models)
        {
            var id = await SaveAsync(model, cancellationToken);
            result.Add(id);
        }
        return result.ToArray();
    }

    protected override IQueryable<DbLessonBatchInfo> Query() => Context.Set<DbLessonBatchInfo>()
        .Include(x => x.StudentGroups)
        .Include(x => x.Teachers)
        .Include(x => x.Rooms);

    private async Task SaveReferencesAsync(Domain.Models.LessonBatchInfo model)
    {
        foreach (var studentGroup in model.StudentGroups)
        {
            await Context.Database.ExecuteSqlInterpolatedAsync(
                $"""
                 INSERT INTO public.lesson_batch_info_student_group (lesson_batch_info_id, student_group_id)
                 VALUES ({model.Id!.Value}, {studentGroup.Id!.Value})
                 ON CONFLICT (lesson_batch_info_id, student_group_id) DO NOTHING
                 """);
        }
        foreach (var teacher in model.Teachers)
        {
            await Context.Database.ExecuteSqlInterpolatedAsync(
                $"""
                 INSERT INTO public.lesson_batch_info_teacher (lesson_batch_info_id, teacher_id)
                 VALUES ({model.Id!.Value}, {teacher.Id!.Value})
                 ON CONFLICT (lesson_batch_info_id, teacher_id) DO NOTHING
                 """);
        }
        foreach (var room in model.Rooms)
        {
            await Context.Database.ExecuteSqlInterpolatedAsync(
                $"""
                 INSERT INTO public.lesson_batch_info_room (lesson_batch_info_id, room_id)
                 VALUES ({model.Id!.Value}, {room.Id!.Value})
                 ON CONFLICT (lesson_batch_info_id, room_id) DO NOTHING
                 """);
        }
    }

    private async Task DeleteReferencesAsync(Guid modelId,
        StudentGroup[] studentGroups, Teacher[] teachers, Room[] rooms)
    {
        foreach (var studentGroup in studentGroups)
        {
            await Context.Database.ExecuteSqlInterpolatedAsync(
                $"""
                 DELETE FROM public.lesson_batch_info_student_group
                 WHERE (lesson_batch_info_id = {modelId} AND student_group_id = {studentGroup.Id!.Value})
                 """);
        }
        foreach (var teacher in teachers)
        {
            await Context.Database.ExecuteSqlInterpolatedAsync(
                $"""
                 DELETE FROM public.lesson_batch_info_teacher
                 WHERE (lesson_batch_info_id = {modelId} AND teacher_id = {teacher.Id!.Value})
                 """);
        }
        foreach (var room in rooms)
        {
            await Context.Database.ExecuteSqlInterpolatedAsync(
                $"""
                 DELETE FROM public.lesson_batch_info_room
                 WHERE (lesson_batch_info_id = {modelId} AND room_id = {room.Id!.Value})
                 """);
        }
    }
}