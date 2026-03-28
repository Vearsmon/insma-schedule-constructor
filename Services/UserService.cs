using Dal.Repositories.Users;
using Domain.Services;

namespace Services;

public class UserService(IUserRepository userRepository) : IUserService
{
}