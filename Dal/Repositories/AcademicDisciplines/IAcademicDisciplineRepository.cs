using Domain.Models;
using Domain.Models.SearchModels;

namespace Dal.Repositories.AcademicDisciplines;

public interface IAcademicDisciplineRepository : IRepository<AcademicDiscipline>
{
    Task<AcademicDiscipline[]> SearchAsync(AcademicDisciplineSearchModel searchModel);

    Task<bool> ExistsAsync(Guid id);
}