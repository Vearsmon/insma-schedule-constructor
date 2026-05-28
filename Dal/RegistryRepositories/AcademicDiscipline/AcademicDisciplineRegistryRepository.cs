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
        .Include(x => x.LessonBatchInfos)
        .ThenInclude(x => x.StudentGroups)
        .Include(x => x.LessonBatchInfos)
        .ThenInclude(x => x.Teachers)
        .Include(x => x.LessonBatchInfos)
        .ThenInclude(x => x.Rooms)
        .ThenInclude(x => x.Campus)
        .Include(x => x.LessonBatchInfos)
        .ThenInclude(x => x.DayOfWeekTimeIntervals);
}