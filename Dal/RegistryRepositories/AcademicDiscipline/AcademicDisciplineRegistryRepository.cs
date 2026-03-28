using Dal.Entities;
using Dal.Repositories;
using Domain.Models.RegistryItemModels;
using Domain.Models.RegistrySearchModels;

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
}