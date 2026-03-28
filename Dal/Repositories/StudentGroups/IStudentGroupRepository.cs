using Domain.Models;
using Domain.Models.SearchModels;

namespace Dal.Repositories.StudentGroups;

public interface IStudentGroupRepository : IRepository<StudentGroup>
{
    Task<StudentGroup[]> SearchAsync(StudentGroupSearchModel searchModel);

    Task<bool> ExistsAsync(Guid id);

    Task<Guid[]> GetStudentGroupTreeIdsAsync(Guid studentGroupId);
}