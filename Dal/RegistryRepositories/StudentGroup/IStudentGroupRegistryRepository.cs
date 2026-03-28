using Domain.Models.RegistryItemModels;
using Domain.Models.RegistrySearchModels;

namespace Dal.RegistryRepositories.StudentGroup;

public interface IStudentGroupRegistryRepository
    : IRegistryRepository<StudentGroupRegistryItem, StudentGroupRegistryInternalSearchModel>
{
}