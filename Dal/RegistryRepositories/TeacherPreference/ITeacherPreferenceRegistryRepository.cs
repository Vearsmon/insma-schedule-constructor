using Domain.Models.RegistryItemModels;
using Domain.Models.RegistrySearchModels;

namespace Dal.RegistryRepositories.TeacherPreference;

public interface ITeacherPreferenceRegistryRepository
    : IRegistryRepository<TeacherPreferenceRegistryItem, TeacherPreferenceRegistryInternalSearchModel>
{
}