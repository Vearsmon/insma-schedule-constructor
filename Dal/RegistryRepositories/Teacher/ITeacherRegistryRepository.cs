using Domain.Models.RegistryItemModels;
using Domain.Models.RegistrySearchModels;

namespace Dal.RegistryRepositories.Teacher;

public interface ITeacherRegistryRepository
    : IRegistryRepository<TeacherRegistryItem, TeacherRegistryInternalSearchModel>
{
}