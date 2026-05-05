using System.Diagnostics.CodeAnalysis;
using Dal.Entities;
using Dal.Mapping;
using Domain.Models;

namespace Dal.Repositories.AcademicDisciplines;

public class AcademicDisciplineMapper : IRepositoryMapper<DbAcademicDiscipline, AcademicDiscipline>
{
    [return: NotNullIfNotNull("entity")]
    public AcademicDiscipline? Map(DbAcademicDiscipline? entity) =>
        AcademicDisciplineMappingRegister.MapEntityToModel(entity);

    public void Update(DbAcademicDiscipline entity, AcademicDiscipline model)
    {
        AcademicDisciplineMappingRegister.UpdateEntityWithModel(model, entity);
    }
}