using Dal.Entities;
using Dal.Transactions;
using Domain.Models;
using Domain.Models.SearchModels;

namespace Dal.Repositories.Users;

public class UserRepository(
    InsmaScheduleContext context,
    IRepositoryMapper<DbUser, User> mapper,
    ITransactionalService transactionalService,
    IPredicateBuilder<DbUser, UserSearchModel> predicateBuilder)
    : Repository<InsmaScheduleContext, DbUser, User>(context, mapper, transactionalService), IUserRepository
{
    public async Task<User[]> SearchAsync(UserSearchModel searchModel)
    {
        return await base.SearchAsync(predicateBuilder, searchModel);
    }
}