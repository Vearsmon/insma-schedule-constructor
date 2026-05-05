using Dal.Entities;
using Domain.Models;
using Riok.Mapperly.Abstractions;

namespace Dal.Mapping;

[Mapper]
public static partial class LessonValidationMessageMappingRegister
{
    [MapProperty(nameof(DbLessonValidationMessage.AffectedByAcademicDisciplineId), $"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByAcademicDisciplineId)}")]
    [MapProperty(nameof(DbLessonValidationMessage.AffectedByAcademicDiscipline), $"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByAcademicDiscipline)}", Use = nameof(@AcademicDisciplineMappingRegister.MapEntityToModel))]
    [MapProperty(nameof(DbLessonValidationMessage.AffectedByAcademicDisciplineType), $"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByAcademicDisciplineType)}")]
    [MapProperty(nameof(DbLessonValidationMessage.AffectedByStudentGroupId), $"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByStudentGroupId)}")]
    [MapProperty(nameof(DbLessonValidationMessage.AffectedByStudentGroup), $"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByStudentGroup)}", Use = nameof(@StudentGroupMappingRegister.MapEntityToModel))]
    [MapProperty(nameof(DbLessonValidationMessage.AffectedByLessonId), $"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByLessonId)}")]
    [MapProperty(nameof(DbLessonValidationMessage.AffectedByLesson), $"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByLesson)}", Use = nameof(@LessonMappingRegister.MapEntityToModel))]
    [MapProperty(nameof(DbLessonValidationMessage.AffectedByTeacherPreferenceId), $"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByTeacherPreferenceId)}")]
    [MapProperty(nameof(DbLessonValidationMessage.AffectedByTeacherPreference), $"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByTeacherPreference)}", Use = nameof(@TeacherPreferenceMappingRegister.MapEntityToModel))]
    [MapProperty(nameof(DbLessonValidationMessage.AffectedByTeacherId), $"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByTeacherId)}")]
    [MapProperty(nameof(DbLessonValidationMessage.AffectedByTeacher), $"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByTeacher)}")]
    [MapperIgnoreSource(nameof(DbLessonValidationMessage.Lesson))]
    [MapperIgnoreTarget(nameof(LessonValidationMessage.Lesson))]
    public static partial LessonValidationMessage? MapEntityToModel(DbLessonValidationMessage? entity);

    [MapProperty($"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByAcademicDisciplineId)}", nameof(DbLessonValidationMessage.AffectedByAcademicDisciplineId))]
    [MapProperty($"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByAcademicDiscipline)}", nameof(DbLessonValidationMessage.AffectedByAcademicDiscipline), Use = nameof(@AcademicDisciplineMappingRegister.MapModelToEntity))]
    [MapProperty($"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByAcademicDisciplineType)}", nameof(DbLessonValidationMessage.AffectedByAcademicDisciplineType))]
    [MapProperty($"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByStudentGroupId)}", nameof(DbLessonValidationMessage.AffectedByStudentGroupId))]
    [MapProperty($"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByStudentGroup)}", nameof(DbLessonValidationMessage.AffectedByStudentGroup), Use = nameof(@StudentGroupMappingRegister.MapModelToEntity))]
    [MapProperty($"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByLessonId)}", nameof(DbLessonValidationMessage.AffectedByLessonId))]
    [MapProperty($"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByLesson)}", nameof(DbLessonValidationMessage.AffectedByLesson), Use = nameof(@LessonMappingRegister.MapModelToEntity))]
    [MapProperty($"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByTeacherPreferenceId)}", nameof(DbLessonValidationMessage.AffectedByTeacherPreferenceId))]
    [MapProperty($"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByTeacherPreference)}", nameof(DbLessonValidationMessage.AffectedByTeacherPreference), Use = nameof(@TeacherPreferenceMappingRegister.MapModelToEntity))]
    [MapProperty($"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByTeacherId)}", nameof(DbLessonValidationMessage.AffectedByTeacherId))]
    [MapProperty($"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByTeacher)}", nameof(DbLessonValidationMessage.AffectedByTeacher))]
    [MapperIgnoreSource(nameof(LessonValidationMessage.Lesson))]
    [MapperIgnoreTarget(nameof(DbLessonValidationMessage.Lesson))]
    public static partial DbLessonValidationMessage? MapModelToEntity(LessonValidationMessage? model);

    [MapProperty($"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByAcademicDisciplineId)}", nameof(DbLessonValidationMessage.AffectedByAcademicDisciplineId))]
    [MapProperty($"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByAcademicDiscipline)}", nameof(DbLessonValidationMessage.AffectedByAcademicDiscipline), Use = nameof(@AcademicDisciplineMappingRegister.MapModelToEntity))]
    [MapProperty($"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByAcademicDisciplineType)}", nameof(DbLessonValidationMessage.AffectedByAcademicDisciplineType))]
    [MapProperty($"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByStudentGroupId)}", nameof(DbLessonValidationMessage.AffectedByStudentGroupId))]
    [MapProperty($"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByStudentGroup)}", nameof(DbLessonValidationMessage.AffectedByStudentGroup), Use = nameof(@StudentGroupMappingRegister.MapModelToEntity))]
    [MapProperty($"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByLessonId)}", nameof(DbLessonValidationMessage.AffectedByLessonId))]
    [MapProperty($"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByLesson)}", nameof(DbLessonValidationMessage.AffectedByLesson), Use = nameof(@LessonMappingRegister.MapModelToEntity))]
    [MapProperty($"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByTeacherPreferenceId)}", nameof(DbLessonValidationMessage.AffectedByTeacherPreferenceId))]
    [MapProperty($"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByTeacherPreference)}", nameof(DbLessonValidationMessage.AffectedByTeacherPreference), Use = nameof(@TeacherPreferenceMappingRegister.MapModelToEntity))]
    [MapProperty($"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByTeacherId)}", nameof(DbLessonValidationMessage.AffectedByTeacherId))]
    [MapProperty($"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByTeacher)}", nameof(DbLessonValidationMessage.AffectedByTeacher))]
    [MapperIgnoreSource(nameof(LessonValidationMessage.Lesson))]
    [MapperIgnoreTarget(nameof(DbLessonValidationMessage.Lesson))]
    public static partial void UpdateEntityWithModel(LessonValidationMessage? model, DbLessonValidationMessage? entity);
}