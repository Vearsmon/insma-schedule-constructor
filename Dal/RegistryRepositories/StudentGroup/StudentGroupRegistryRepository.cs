using Dal.Entities;
using Dal.Repositories;
using Domain.Models.RegistryItemModels;
using Domain.Models.RegistrySearchModels;

namespace Dal.RegistryRepositories.StudentGroup;

internal class StudentGroupRegistryRepository(
    InsmaScheduleContext context,
    IReadonlyRepositoryMapper<DbStudentGroup, StudentGroupRegistryItem> mapper,
    IRegistryRepositoryOrderer<DbStudentGroup, StudentGroupRegistryInternalSearchModel> orderer,
    IPredicateBuilder<DbStudentGroup, StudentGroupRegistryInternalSearchModel> predicateBuilder)
    : ReadonlyRegistryRepository<InsmaScheduleContext, DbStudentGroup, StudentGroupRegistryItem,
            StudentGroupRegistryInternalSearchModel>(context, mapper, orderer, predicateBuilder),
        IStudentGroupRegistryRepository
{
}