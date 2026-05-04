using System.Diagnostics.CodeAnalysis;
using Dal.Entities;
using Dal.Mapping;
using Domain.Models;

namespace Dal.Repositories.StudentGroups;

public class StudentGroupMapper : IRepositoryMapper<DbStudentGroup, StudentGroup>
{
    [return: NotNullIfNotNull("entity")]
    public StudentGroup? Map(DbStudentGroup? entity)
    {
        return MappingRegister.Map(entity);
    }

    public void Update(DbStudentGroup entity, StudentGroup model)
    {
        MappingRegister.Update(model, entity);
        entity.Parents = model.Parents.Select(x => new DbStudentGroupLink { ParentStudentGroupId = x.Id ?? Guid.Empty, ChildStudentGroupId = model.Id ?? Guid.Empty }).ToList();
        entity.Children = model.Children.Select(x => new DbStudentGroupLink { ParentStudentGroupId = model.Id ?? Guid.Empty, ChildStudentGroupId = x.Id ?? Guid.Empty }).ToList();
    }
}