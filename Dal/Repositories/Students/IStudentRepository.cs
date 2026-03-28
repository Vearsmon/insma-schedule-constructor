using Domain.Models;
using Domain.Models.SearchModels;

namespace Dal.Repositories.Students;

public interface IStudentRepository : IRepository<Student>
{
    Task<Student[]> SearchAsync(StudentSearchModel searchModel);
}