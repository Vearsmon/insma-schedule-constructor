using Domain.Models;
using Domain.Models.SearchModels;

namespace Dal.Repositories.Teachers;

public interface ITeacherRepository : IRepository<Teacher>
{
    Task<Teacher[]> SearchAsync(TeacherSearchModel searchModel);

    Task<bool> ExistsAsync(Guid id);
}