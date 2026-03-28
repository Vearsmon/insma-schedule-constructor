using Domain.Models;
using Domain.Models.SearchModels;

namespace Dal.Repositories.TeacherPreferences;

public interface ITeacherPreferenceRepository : IRepository<TeacherPreference>
{
    Task<TeacherPreference[]> SearchAsync(TeacherPreferenceSearchModel searchModel);
}