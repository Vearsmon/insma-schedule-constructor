using Dal.Entities;
using Dal.Repositories;
using Domain.Models.RegistryItemModels;
using Domain.Models.RegistrySearchModels;
using Microsoft.EntityFrameworkCore;

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
    protected override IQueryable<DbStudentGroup> Query => Context.Set<DbStudentGroup>()
        .Include(x => x.Children)
        .Include(x => x.Parents);
}