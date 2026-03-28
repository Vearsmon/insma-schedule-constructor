using Domain.Models.RegistryItemModels;
using Domain.Models.RegistrySearchModels;

namespace Dal.RegistryRepositories.AcademicDiscipline;

public interface IAcademicDisciplineRegistryRepository
    : IRegistryRepository<AcademicDisciplineRegistryItem, AcademicDisciplineRegistryInternalSearchModel>
{
}