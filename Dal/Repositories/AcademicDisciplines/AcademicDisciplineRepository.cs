using Dal.Entities;
using Dal.Transactions;
using Domain.Models;
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