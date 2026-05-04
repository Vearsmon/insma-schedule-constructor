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
        .Include(x => x.AcademicDisciplineLectureLessonBatchInfos)
        .ThenInclude(x => x.Teachers)
        .Include(x => x.AcademicDisciplineLectureLessonBatchInfos)
        .ThenInclude(x => x.Rooms)

        .Include(x => x.AcademicDisciplinePracticeLessonBatchInfos)
        .ThenInclude(x => x.StudentGroups)
        .Include(x => x.AcademicDisciplinePracticeLessonBatchInfos)
        .ThenInclude(x => x.Teachers)
        .Include(x => x.AcademicDisciplinePracticeLessonBatchInfos)
        .ThenInclude(x => x.Rooms)

        .Include(x => x.AcademicDisciplineLabLessonBatchInfos)
        .ThenInclude(x => x.StudentGroups)
        .Include(x => x.AcademicDisciplineLabLessonBatchInfos)
        .ThenInclude(x => x.Teachers)
        .Include(x => x.AcademicDisciplineLabLessonBatchInfos)
        .ThenInclude(x => x.Rooms)

        .Include(x => x.AcademicDisciplineExamLessonBatchInfos)
        .ThenInclude(x => x.StudentGroups)
        .Include(x => x.AcademicDisciplineExamLessonBatchInfos)
        .ThenInclude(x => x.Teachers)
        .Include(x => x.AcademicDisciplineExamLessonBatchInfos)
        .ThenInclude(x => x.Rooms)

        .Include(x => x.AcademicDisciplineTestLessonBatchInfos)
        .ThenInclude(x => x.StudentGroups)
        .Include(x => x.AcademicDisciplineTestLessonBatchInfos)
        .ThenInclude(x => x.Teachers)
        .Include(x => x.AcademicDisciplineTestLessonBatchInfos)
        .ThenInclude(x => x.Rooms);
}