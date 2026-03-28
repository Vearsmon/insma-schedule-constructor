using Domain.Dto.RegistryDto;

namespace Dal.RegistryRepositories;

public interface IRegistryRepository<TRegistryItem, in TSearchModel>
{
    Task<RegistryDto<TRegistryItem>> SearchAsync(TSearchModel searchModel);
}
