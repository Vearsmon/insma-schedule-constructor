using Dal.Entities;
using Dal.Repositories;
using Domain.Models.RegistryItemModels;
using Domain.Models.RegistrySearchModels;

namespace Dal.RegistryRepositories.Teacher;

internal class TeacherRegistryRepository(
    InsmaScheduleContext context,
    IReadonlyRepositoryMapper<DbTeacher, TeacherRegistryItem> mapper,
    IRegistryRepositoryOrderer<DbTeacher, TeacherRegistryInternalSearchModel> orderer,
    IPredicateBuilder<DbTeacher, TeacherRegistryInternalSearchModel> predicateBuilder)
    : ReadonlyRegistryRepository<InsmaScheduleContext, DbTeacher, TeacherRegistryItem,
            TeacherRegistryInternalSearchModel>(context, mapper, orderer, predicateBuilder),
        ITeacherRegistryRepository
{
}