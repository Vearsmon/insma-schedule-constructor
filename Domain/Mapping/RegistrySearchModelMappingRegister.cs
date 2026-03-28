using Domain.Models.RegistrySearchModels;
using Riok.Mapperly.Abstractions;

namespace Domain.Mapping;

[Mapper]
public static partial class RegistrySearchModelMappingRegister
{
    #region AcademicDiscipline

    public static partial AcademicDisciplineRegistryInternalSearchModel Map(AcademicDisciplineRegistrySearchModel searchModel);

    #endregion

    #region Campus

    public static partial CampusRegistryInternalSearchModel Map(CampusRegistrySearchModel searchModel);

    #endregion

    #region Schedule

    public static partial ScheduleRegistryInternalSearchModel Map(ScheduleRegistrySearchModel searchModel);

    #endregion

    #region StudentGroup

    public static partial StudentGroupRegistryInternalSearchModel Map(StudentGroupRegistrySearchModel searchModel);

    #endregion

    #region Teacher

    public static partial TeacherRegistryInternalSearchModel Map(TeacherRegistrySearchModel searchModel);

    #endregion

    #region TeacherPreference

    public static partial TeacherPreferenceRegistryInternalSearchModel Map(TeacherPreferenceRegistrySearchModel searchModel);

    #endregion
}