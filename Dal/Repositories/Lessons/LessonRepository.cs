using Dal.Entities;
using Dal.Transactions;
using Domain.Models;
using Domain.Models.SearchModels;
using Microsoft.EntityFrameworkCore;

namespace Dal.Repositories.Lessons;

public class LessonRepository(
    InsmaScheduleContext context,
    IRepositoryMapper<DbLesson, Lesson> mapper,
    ITransactionalService transactionalService,
    IPredicateBuilder<DbLesson, LessonSearchModel> predicateBuilder,
    IPredicateBuilder<DbLesson, LessonConflictsSearchModel> conflictsPredicateBuilder)
    : Repository<InsmaScheduleContext, DbLesson, Lesson>(context, mapper, transactionalService), ILessonRepository
{
    public async Task<Lesson[]> SearchAsync(LessonSearchModel searchModel)
    {
        return await base.SearchAsync(predicateBuilder, searchModel);
    }

    public async Task<Lesson[]> SearchConflictsAsync(LessonConflictsSearchModel searchModel)
    {
        return await base.SearchAsync(conflictsPredicateBuilder, searchModel);
    }

    public override async Task<Guid> SaveAsync(Lesson model, CancellationToken cancellationToken = default)
    {
        var id = model.Id;
        var previousLesson = id.HasValue ? await GetAsync(id.Value, cancellationToken) : null;
        if (previousLesson == null)
        {
            id = await base.SaveAsync(model, cancellationToken);
            model.Id = id;
            await SaveReferencesAsync(model);
            return id.Value;
        }

        await Context.Set<DbLessonPolicyViolation>().Where(x => x.LessonId == previousLesson.Id).ExecuteDeleteAsync(cancellationToken);

        var removedStudentGroups = previousLesson.StudentGroups
            .Where(x => model.StudentGroups.All(y => y.Id != x.Id))
            .ToArray();
        var removedTeachers = previousLesson.Teachers
            .Where(x => model.Teachers.All(y => y.Id != x.Id))
            .ToArray();
        var removedRooms = previousLesson.Rooms
            .Where(x => model.Rooms.All(y => y.Id != x.Id))
            .ToArray();

        await DeleteReferencesAsync(id!.Value, removedStudentGroups, removedTeachers, removedRooms);
        await base.SaveAsync(model, cancellationToken);
        await SaveReferencesAsync(model);

        return id.Value;
    }

    public override async Task<Guid[]> SaveAllAsync(Lesson[] models, CancellationToken cancellationToken = default)
    {
        var result = new List<Guid>();
        foreach (var model in models)
        {
            var id = await SaveAsync(model, cancellationToken);
            result.Add(id);
        }
        return result.ToArray();
    }

    protected override IQueryable<DbLesson> Query() => Context.Set<DbLesson>()
        .Include(x => x.AcademicDiscipline)
        .Include(x => x.StudentGroups)
        .Include(x => x.Teachers)
        .Include(x => x.Rooms)
        .Include(x => x.LessonBatchInfo)
        .ThenInclude(x => x!.StudentGroups)
        .Include(x => x.LessonBatchInfo)
        .ThenInclude(x => x!.Teachers)
        .Include(x => x.LessonBatchInfo)
        .ThenInclude(x => x!.Rooms)
        .Include(x => x.Violations);

    private async Task SaveReferencesAsync(Lesson model)
    {
        foreach (var studentGroup in model.StudentGroups)
        {
            await Context.Database.ExecuteSqlInterpolatedAsync(
                $"""
                 INSERT INTO public.lesson_student_group (lesson_id, student_group_id)
                 VALUES ({model.Id!.Value}, {studentGroup.Id!.Value})
                 ON CONFLICT (lesson_id, student_group_id) DO NOTHING
                 """);
        }
        foreach (var teacher in model.Teachers)
        {
            await Context.Database.ExecuteSqlInterpolatedAsync(
                $"""
                 INSERT INTO public.lesson_teacher (lesson_id, teacher_id)
                 VALUES ({model.Id!.Value}, {teacher.Id!.Value})
                 ON CONFLICT (lesson_id, teacher_id) DO NOTHING
                 """);
        }
        foreach (var room in model.Rooms)
        {
            await Context.Database.ExecuteSqlInterpolatedAsync(
                $"""
                 INSERT INTO public.lesson_room (lesson_id, room_id)
                 VALUES ({model.Id!.Value}, {room.Id!.Value})
                 ON CONFLICT (lesson_id, room_id) DO NOTHING
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
                 DELETE FROM public.lesson_student_group
                 WHERE (lesson_id = {modelId} AND student_group_id = {studentGroup.Id!.Value})
                 """);
        }
        foreach (var teacher in teachers)
        {
            await Context.Database.ExecuteSqlInterpolatedAsync(
                $"""
                 DELETE FROM public.lesson_teacher
                 WHERE (lesson_id = {modelId} AND teacher_id = {teacher.Id!.Value})
                 """);
        }
        foreach (var room in rooms)
        {
            await Context.Database.ExecuteSqlInterpolatedAsync(
                $"""
                 DELETE FROM public.lesson_room
                 WHERE (lesson_id = {modelId} AND room_id = {room.Id!.Value})
                 """);
        }
    }
}