using System.Diagnostics.CodeAnalysis;
using Dal.Entities;
using Dal.Mapping;
using Domain.Models;

namespace Dal.Repositories.Users;

public class UserMapper : IRepositoryMapper<DbUser, User>
{
    [return: NotNullIfNotNull("entity")]
    public User? Map(DbUser? entity)
    {
        return MappingRegister.Map(entity);
    }

    public void Update(DbUser entity, User model)
    {
        MappingRegister.Update(model, entity);
    }
}