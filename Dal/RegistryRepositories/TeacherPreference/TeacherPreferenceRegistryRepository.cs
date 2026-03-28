using Dal.Entities;
using Dal.Repositories;
using Domain.Models.RegistryItemModels;
using Domain.Models.RegistrySearchModels;

namespace Dal.RegistryRepositories.TeacherPreference;

internal class TeacherPreferenceRegistryRepository(
    InsmaScheduleContext context,
    IReadonlyRepositoryMapper<DbTeacherPreference, TeacherPreferenceRegistryItem> mapper,
    IRegistryRepositoryOrderer<DbTeacherPreference, TeacherPreferenceRegistryInternalSearchModel> orderer,
    IPredicateBuilder<DbTeacherPreference, TeacherPreferenceRegistryInternalSearchModel> predicateBuilder)
    : ReadonlyRegistryRepository<InsmaScheduleContext, DbTeacherPreference, TeacherPreferenceRegistryItem,
            TeacherPreferenceRegistryInternalSearchModel>(context, mapper, orderer, predicateBuilder),
        ITeacherPreferenceRegistryRepository
{
}