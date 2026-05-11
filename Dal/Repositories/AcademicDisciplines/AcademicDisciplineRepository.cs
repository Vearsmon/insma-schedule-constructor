using Dal.Entities;
using Dal.Mapping;
using Dal.Transactions;
using Domain.Models;
using Domain.Models.Enums;
using Domain.Models.SearchModels;
using Microsoft.EntityFrameworkCore;

namespace Dal.Repositories.AcademicDisciplines;

public class AcademicDisciplineRepository(
    InsmaScheduleContext context,
    IRepositoryMapper<DbAcademicDiscipline, AcademicDiscipline> mapper,
    ITransactionalService transactionalService,
    IPredicateBuilder<DbAcademicDiscipline, AcademicDisciplineSearchModel> predicateBuilder)
    : Repository<InsmaScheduleContext, DbAcademicDiscipline, AcademicDiscipline>(context, mapper, transactionalService), IAcademicDisciplineRepository
{
    public async Task<AcademicDiscipline[]> SearchAsync(AcademicDisciplineSearchModel searchModel)
    {
        return await base.SearchAsync(predicateBuilder, searchModel);
    }

    public override async Task<Guid> SaveAsync(AcademicDiscipline model, CancellationToken cancellationToken = default)
    {
        var previousAcademicDiscipline = model.Id.HasValue ? await GetAsync(model.Id!.Value, cancellationToken) : null;
        if (previousAcademicDiscipline != null)
        {
            var previousLessonBatchInfos = Enum.GetValues<AcademicDisciplineType>()
                .SelectMany(previousAcademicDiscipline.GetBatchInfosByType)
                .ToArray();
            var currentLessonBatchInfos = Enum.GetValues<AcademicDisciplineType>()
                .SelectMany(model.GetBatchInfosByType);
            var lessonBatchInfosToDeleteIds = previousLessonBatchInfos
                .Where(previous => currentLessonBatchInfos.All(current => current.Id != previous.Id))
                .Select(toDelete => toDelete.Id!.Value);
            var lessonBatchInfosToUpdate = previousLessonBatchInfos
                .Where(previous => currentLessonBatchInfos.Any(current => current.Id == previous.Id))
                .ToArray();
            var lessonBatchInfosToUpdateIds = lessonBatchInfosToUpdate.Select(toUpdate => toUpdate.Id!.Value);

            await Context.Set<DbLessonBatchInfo>().Where(x => lessonBatchInfosToDeleteIds.Contains(x.Id)).ExecuteDeleteAsync(cancellationToken);
            var toUpdateEntities = await Context.Set<DbLessonBatchInfo>().Where(x => lessonBatchInfosToUpdateIds.Contains(x.Id)).ToArrayAsync(cancellationToken: cancellationToken);
            foreach (var toUpdateEntity in toUpdateEntities)
            {
                LessonBatchInfoMappingRegister.UpdateEntityWithModel(lessonBatchInfosToUpdate.Single(x => x.Id == toUpdateEntity.Id), toUpdateEntity);
            }
            await Context.Set<DbLessonBatchInfoStudentGroup>().Where(x => lessonBatchInfosToUpdateIds.Contains(x.LessonBatchInfoId)).ExecuteDeleteAsync(cancellationToken);
            await Context.Set<DbLessonBatchInfoTeacher>().Where(x => lessonBatchInfosToUpdateIds.Contains(x.LessonBatchInfoId)).ExecuteDeleteAsync(cancellationToken);
            await Context.Set<DbLessonBatchInfoRoom>().Where(x => lessonBatchInfosToUpdateIds.Contains(x.LessonBatchInfoId)).ExecuteDeleteAsync(cancellationToken);

            var toUpdateLectureIds = model.LectureLessonBatchInfos.Where(x => x.Id.HasValue).Select(x => x.Id!.Value).ToArray();
            var toUpdatePracticeIds = model.PracticeLessonBatchInfos.Where(x => x.Id.HasValue).Select(x => x.Id!.Value).ToArray();
            var toUpdateLabIds = model.LabLessonBatchInfos.Where(x => x.Id.HasValue).Select(x => x.Id!.Value).ToArray();
            var toUpdateExamIds = model.ExamLessonBatchInfos.Where(x => x.Id.HasValue).Select(x => x.Id!.Value).ToArray();
            var toUpdateTestIds = model.TestLessonBatchInfos.Where(x => x.Id.HasValue).Select(x => x.Id!.Value).ToArray();
            model.LectureLessonBatchInfos = model.LectureLessonBatchInfos.Where(x => !x.Id.HasValue).ToArray();
            model.PracticeLessonBatchInfos = model.PracticeLessonBatchInfos.Where(x => !x.Id.HasValue).ToArray();
            model.LabLessonBatchInfos = model.LabLessonBatchInfos.Where(x => !x.Id.HasValue).ToArray();
            model.ExamLessonBatchInfos = model.ExamLessonBatchInfos.Where(x => !x.Id.HasValue).ToArray();
            model.TestLessonBatchInfos = model.TestLessonBatchInfos.Where(x => !x.Id.HasValue).ToArray();
            await Context.SaveChangesAsync(cancellationToken);

            var id = await base.SaveAsync(model, cancellationToken);
            var newAcademicDiscipline = Context.Set<DbAcademicDiscipline>().Single(x => x.Id == id);
            foreach (var toUpdateId in toUpdateLectureIds)
            {
                newAcademicDiscipline.AcademicDisciplineLectureLessonBatchInfos.Add(toUpdateEntities.Single(x => x.Id == toUpdateId));
            }
            foreach (var toUpdateId in toUpdatePracticeIds)
            {
                newAcademicDiscipline.AcademicDisciplinePracticeLessonBatchInfos.Add(toUpdateEntities.Single(x => x.Id == toUpdateId));
            }
            foreach (var toUpdateId in toUpdateLabIds)
            {
                newAcademicDiscipline.AcademicDisciplineLabLessonBatchInfos.Add(toUpdateEntities.Single(x => x.Id == toUpdateId));
            }
            foreach (var toUpdateId in toUpdateExamIds)
            {
                newAcademicDiscipline.AcademicDisciplineExamLessonBatchInfos.Add(toUpdateEntities.Single(x => x.Id == toUpdateId));
            }
            foreach (var toUpdateId in toUpdateTestIds)
            {
                newAcademicDiscipline.AcademicDisciplineTestLessonBatchInfos.Add(toUpdateEntities.Single(x => x.Id == toUpdateId));
            }

            await Context.SaveChangesAsync(cancellationToken);
            return id;
        }

        return await base.SaveAsync(model, cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await ExistAsync(predicateBuilder, new AcademicDisciplineSearchModel { Id = id });
    }

    protected override IQueryable<DbAcademicDiscipline> Query() => Context.Set<DbAcademicDiscipline>()
        .Include(x => x.Schedule)
        .Include(x => x.AcademicDisciplineLectureLessonBatchInfos)
        .ThenInclude(x => x.StudentGroups)
        .ThenInclude(x => x.StudentGroup)
        .Include(x => x.AcademicDisciplineLectureLessonBatchInfos)
        .ThenInclude(x => x.Teachers)
        .ThenInclude(x => x.Teacher)
        .Include(x => x.AcademicDisciplineLectureLessonBatchInfos)
        .ThenInclude(x => x.Rooms)
        .ThenInclude(x => x.Room)

        .Include(x => x.AcademicDisciplinePracticeLessonBatchInfos)
        .ThenInclude(x => x.StudentGroups)
        .ThenInclude(x => x.StudentGroup)
        .Include(x => x.AcademicDisciplinePracticeLessonBatchInfos)
        .ThenInclude(x => x.Teachers)
        .ThenInclude(x => x.Teacher)
        .Include(x => x.AcademicDisciplinePracticeLessonBatchInfos)
        .ThenInclude(x => x.Rooms)
        .ThenInclude(x => x.Room)

        .Include(x => x.AcademicDisciplineLabLessonBatchInfos)
        .ThenInclude(x => x.StudentGroups)
        .ThenInclude(x => x.StudentGroup)
        .Include(x => x.AcademicDisciplineLabLessonBatchInfos)
        .ThenInclude(x => x.Teachers)
        .ThenInclude(x => x.Teacher)
        .Include(x => x.AcademicDisciplineLabLessonBatchInfos)
        .ThenInclude(x => x.Rooms)
        .ThenInclude(x => x.Room)

        .Include(x => x.AcademicDisciplineExamLessonBatchInfos)
        .ThenInclude(x => x.StudentGroups)
        .ThenInclude(x => x.StudentGroup)
        .Include(x => x.AcademicDisciplineExamLessonBatchInfos)
        .ThenInclude(x => x.Teachers)
        .ThenInclude(x => x.Teacher)
        .Include(x => x.AcademicDisciplineExamLessonBatchInfos)
        .ThenInclude(x => x.Rooms)
        .ThenInclude(x => x.Room)

        .Include(x => x.AcademicDisciplineTestLessonBatchInfos)
        .ThenInclude(x => x.StudentGroups)
        .ThenInclude(x => x.StudentGroup)
        .Include(x => x.AcademicDisciplineTestLessonBatchInfos)
        .ThenInclude(x => x.Teachers)
        .ThenInclude(x => x.Teacher)
        .Include(x => x.AcademicDisciplineTestLessonBatchInfos)
        .ThenInclude(x => x.Rooms)
        .ThenInclude(x => x.Room);
}