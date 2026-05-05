using System.Diagnostics.CodeAnalysis;
using Dal.Entities;
using Dal.Mapping;
using Domain.Models;

namespace Dal.Repositories.TeacherPreferences;

public class TeacherPreferenceMapper : IRepositoryMapper<DbTeacherPreference, TeacherPreference>
{
    [return: NotNullIfNotNull("entity")]
    public TeacherPreference? Map(DbTeacherPreference? entity) => TeacherPreferenceMappingRegister.MapEntityToModel(entity);

    public void Update(DbTeacherPreference entity, TeacherPreference model)
    {
        TeacherPreferenceMappingRegister.UpdateEntityWithModel(model, entity);
    }
}