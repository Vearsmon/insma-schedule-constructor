using System.Diagnostics.CodeAnalysis;
using Dal.Entities;
using Dal.Mapping;
using Domain.Models;

namespace Dal.Repositories.Campuses;

public class CampusMapper : IRepositoryMapper<DbCampus, Campus>
{
    [return: NotNullIfNotNull("entity")]
    public Campus? Map(DbCampus? entity)
    {
        return MappingRegister.Map(entity);
    }

    public void Update(DbCampus entity, Campus model)
    {
        MappingRegister.Update(model, entity);
    }
}