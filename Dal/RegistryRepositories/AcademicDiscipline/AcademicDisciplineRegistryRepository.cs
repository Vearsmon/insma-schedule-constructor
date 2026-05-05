using Dal.Entities;
using Dal.Repositories;
using Domain.Models.RegistryItemModels;
using Domain.Models.RegistrySearchModels;
using Microsoft.EntityFrameworkCore;

namespace Dal.RegistryRepositories.AcademicDiscipline;

internal class AcademicDisciplineRegistryRepository(
    InsmaScheduleContext context,
    IReadonlyRepositoryMapper<DbAcademicDiscipline, AcademicDisciplineRegistryItem> mapper,
    IRegistryRepositoryOrderer<DbAcademicDiscipline, AcademicDisciplineRegistryInternalSearchModel> orderer,
    IPredicateBuilder<DbAcademicDiscipline, AcademicDisciplineRegistryInternalSearchModel> predicateBuilder)
    : ReadonlyRegistryRepository<InsmaScheduleContext, DbAcademicDiscipline, AcademicDisciplineRegistryItem,
            AcademicDisciplineRegistryInternalSearchModel>(context, mapper, orderer, predicateBuilder),
        IAcademicDisciplineRegistryRepository
{
    protected override IQueryable<DbAcademicDiscipline> Query => Context.Set<DbAcademicDiscipline>()
        .AsNoTracking()
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
        .ThenInclude(x => x.Room)
    ;
}