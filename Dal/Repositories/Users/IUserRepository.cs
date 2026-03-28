using Domain.Models;
using Domain.Models.SearchModels;

namespace Dal.Repositories.Users;

public interface IUserRepository : IRepository<User>
{
    Task<User[]> SearchAsync(UserSearchModel searchModel);
}