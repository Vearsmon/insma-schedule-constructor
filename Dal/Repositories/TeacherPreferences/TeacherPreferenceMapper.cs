using System.Diagnostics.CodeAnalysis;
using Dal.Entities;
using Dal.Mapping;
using Domain.Models;

namespace Dal.Repositories.TeacherPreferences;

public class TeacherPreferenceMapper : IRepositoryMapper<DbTeacherPreference, TeacherPreference>
{
    [return: NotNullIfNotNull("entity")]
    public TeacherPreference? Map(DbTeacherPreference? entity)
    {
        return MappingRegister.Map(entity);
    }

    public void Update(DbTeacherPreference entity, TeacherPreference model)
    {
        MappingRegister.Update(model, entity);
    }
}