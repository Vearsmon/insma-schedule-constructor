using Dal.Entities;
using Domain.Models;
using Riok.Mapperly.Abstractions;

namespace Dal.Mapping;

[Mapper]
public static partial class LessonPolicyViolationMappingRegister
{
    [UserMapping(Default = true)]
    public static LessonPolicyViolation? MapEntityToModel(DbLessonPolicyViolation? entity)
    {
        var model = AutoMapEntityToModel(entity);
        if (entity == null) return model;
        model!.Payload = new LessonValidationPayload();
        model.Payload.AffectedByAcademicDisciplineId = entity.AffectedByAcademicDisciplineId;
        model.Payload.AffectedByAcademicDiscipline = AcademicDisciplineMappingRegister.MapEntityToModel(entity.AffectedByAcademicDiscipline);
        model.Payload.AffectedByAcademicDisciplineType = entity.AffectedByAcademicDisciplineType;
        model.Payload.AffectedByStudentGroupId = entity.AffectedByStudentGroupId;
        model.Payload.AffectedByStudentGroup = StudentGroupMappingRegister.MapEntityToModel(entity.AffectedByStudentGroup);
        model.Payload.AffectedByLessonId = entity.AffectedByLessonId;
        model.Payload.AffectedByLesson = LessonMappingRegister.MapEntityToModel(entity.AffectedByLesson);
        model.Payload.AffectedByTeacherPreferenceId = entity.AffectedByTeacherPreferenceId;
        model.Payload.AffectedByTeacherPreference = TeacherPreferenceMappingRegister.MapEntityToModel(entity.AffectedByTeacherPreference);
        model.Payload.AffectedByTeacherId = entity.AffectedByTeacherId;
        model.Payload.AffectedByTeacher = TeacherMappingRegister.MapEntityToModel(entity.AffectedByTeacher);
        return model;
    }

    [MapProperty($"{nameof(LessonPolicyViolation.Payload)}.{nameof(LessonPolicyViolation.Payload.AffectedByAcademicDisciplineId)}", nameof(DbLessonPolicyViolation.AffectedByAcademicDisciplineId))]
    [MapProperty($"{nameof(LessonPolicyViolation.Payload)}.{nameof(LessonPolicyViolation.Payload.AffectedByAcademicDiscipline)}", nameof(DbLessonPolicyViolation.AffectedByAcademicDiscipline), Use = nameof(@AcademicDisciplineMappingRegister.MapModelToEntity))]
    [MapProperty($"{nameof(LessonPolicyViolation.Payload)}.{nameof(LessonPolicyViolation.Payload.AffectedByAcademicDisciplineType)}", nameof(DbLessonPolicyViolation.AffectedByAcademicDisciplineType))]
    [MapProperty($"{nameof(LessonPolicyViolation.Payload)}.{nameof(LessonPolicyViolation.Payload.AffectedByStudentGroupId)}", nameof(DbLessonPolicyViolation.AffectedByStudentGroupId))]
    [MapProperty($"{nameof(LessonPolicyViolation.Payload)}.{nameof(LessonPolicyViolation.Payload.AffectedByStudentGroup)}", nameof(DbLessonPolicyViolation.AffectedByStudentGroup), Use = nameof(@StudentGroupMappingRegister.MapModelToEntity))]
    [MapProperty($"{nameof(LessonPolicyViolation.Payload)}.{nameof(LessonPolicyViolation.Payload.AffectedByLessonId)}", nameof(DbLessonPolicyViolation.AffectedByLessonId))]
    [MapProperty($"{nameof(LessonPolicyViolation.Payload)}.{nameof(LessonPolicyViolation.Payload.AffectedByLesson)}", nameof(DbLessonPolicyViolation.AffectedByLesson), Use = nameof(@LessonMappingRegister.MapModelToEntity))]
    [MapProperty($"{nameof(LessonPolicyViolation.Payload)}.{nameof(LessonPolicyViolation.Payload.AffectedByTeacherPreferenceId)}", nameof(DbLessonPolicyViolation.AffectedByTeacherPreferenceId))]
    [MapProperty($"{nameof(LessonPolicyViolation.Payload)}.{nameof(LessonPolicyViolation.Payload.AffectedByTeacherPreference)}", nameof(DbLessonPolicyViolation.AffectedByTeacherPreference), Use = nameof(@TeacherPreferenceMappingRegister.MapModelToEntity))]
    [MapProperty($"{nameof(LessonPolicyViolation.Payload)}.{nameof(LessonPolicyViolation.Payload.AffectedByTeacherId)}", nameof(DbLessonPolicyViolation.AffectedByTeacherId))]
    [MapProperty($"{nameof(LessonPolicyViolation.Payload)}.{nameof(LessonPolicyViolation.Payload.AffectedByTeacher)}", nameof(DbLessonPolicyViolation.AffectedByTeacher), Use = nameof(@TeacherMappingRegister.MapModelToEntity))]
    [MapProperty($"{nameof(LessonPolicyViolation.Payload)}.{nameof(LessonPolicyViolation.Payload.AffectedByRoomId)}", nameof(DbLessonPolicyViolation.AffectedByRoomId))]
    [MapProperty($"{nameof(LessonPolicyViolation.Payload)}.{nameof(LessonPolicyViolation.Payload.AffectedByRoom)}", nameof(DbLessonPolicyViolation.AffectedByRoom), Use = nameof(@RoomMappingRegister.MapModelToEntity))]
    [MapperIgnoreSource(nameof(LessonPolicyViolation.Lesson))]
    [MapperIgnoreTarget(nameof(DbLessonPolicyViolation.Lesson))]
    public static partial DbLessonPolicyViolation? MapModelToEntity(LessonPolicyViolation? model);

    [MapProperty($"{nameof(LessonPolicyViolation.Payload)}.{nameof(LessonPolicyViolation.Payload.AffectedByAcademicDisciplineId)}", nameof(DbLessonPolicyViolation.AffectedByAcademicDisciplineId))]
    [MapProperty($"{nameof(LessonPolicyViolation.Payload)}.{nameof(LessonPolicyViolation.Payload.AffectedByAcademicDiscipline)}", nameof(DbLessonPolicyViolation.AffectedByAcademicDiscipline), Use = nameof(@AcademicDisciplineMappingRegister.MapModelToEntity))]
    [MapProperty($"{nameof(LessonPolicyViolation.Payload)}.{nameof(LessonPolicyViolation.Payload.AffectedByAcademicDisciplineType)}", nameof(DbLessonPolicyViolation.AffectedByAcademicDisciplineType))]
    [MapProperty($"{nameof(LessonPolicyViolation.Payload)}.{nameof(LessonPolicyViolation.Payload.AffectedByStudentGroupId)}", nameof(DbLessonPolicyViolation.AffectedByStudentGroupId))]
    [MapProperty($"{nameof(LessonPolicyViolation.Payload)}.{nameof(LessonPolicyViolation.Payload.AffectedByStudentGroup)}", nameof(DbLessonPolicyViolation.AffectedByStudentGroup), Use = nameof(@StudentGroupMappingRegister.MapModelToEntity))]
    [MapProperty($"{nameof(LessonPolicyViolation.Payload)}.{nameof(LessonPolicyViolation.Payload.AffectedByLessonId)}", nameof(DbLessonPolicyViolation.AffectedByLessonId))]
    [MapProperty($"{nameof(LessonPolicyViolation.Payload)}.{nameof(LessonPolicyViolation.Payload.AffectedByLesson)}", nameof(DbLessonPolicyViolation.AffectedByLesson), Use = nameof(@LessonMappingRegister.MapModelToEntity))]
    [MapProperty($"{nameof(LessonPolicyViolation.Payload)}.{nameof(LessonPolicyViolation.Payload.AffectedByTeacherPreferenceId)}", nameof(DbLessonPolicyViolation.AffectedByTeacherPreferenceId))]
    [MapProperty($"{nameof(LessonPolicyViolation.Payload)}.{nameof(LessonPolicyViolation.Payload.AffectedByTeacherPreference)}", nameof(DbLessonPolicyViolation.AffectedByTeacherPreference), Use = nameof(@TeacherPreferenceMappingRegister.MapModelToEntity))]
    [MapProperty($"{nameof(LessonPolicyViolation.Payload)}.{nameof(LessonPolicyViolation.Payload.AffectedByTeacherId)}", nameof(DbLessonPolicyViolation.AffectedByTeacherId))]
    [MapProperty($"{nameof(LessonPolicyViolation.Payload)}.{nameof(LessonPolicyViolation.Payload.AffectedByTeacher)}", nameof(DbLessonPolicyViolation.AffectedByTeacher), Use = nameof(@TeacherMappingRegister.MapModelToEntity))]
    [MapProperty($"{nameof(LessonPolicyViolation.Payload)}.{nameof(LessonPolicyViolation.Payload.AffectedByRoomId)}", nameof(DbLessonPolicyViolation.AffectedByRoomId))]
    [MapProperty($"{nameof(LessonPolicyViolation.Payload)}.{nameof(LessonPolicyViolation.Payload.AffectedByRoom)}", nameof(DbLessonPolicyViolation.AffectedByRoom), Use = nameof(@RoomMappingRegister.MapModelToEntity))]
    [MapperIgnoreSource(nameof(LessonPolicyViolation.Lesson))]
    [MapperIgnoreTarget(nameof(DbLessonPolicyViolation.Lesson))]
    public static partial void UpdateEntityWithModel(LessonPolicyViolation? model, DbLessonPolicyViolation? entity);

    [MapperIgnoreSource(nameof(DbLessonPolicyViolation.AffectedByAcademicDisciplineId))]
    [MapperIgnoreSource(nameof(DbLessonPolicyViolation.AffectedByAcademicDiscipline))]
    [MapperIgnoreSource(nameof(DbLessonPolicyViolation.AffectedByAcademicDisciplineType))]
    [MapperIgnoreSource(nameof(DbLessonPolicyViolation.AffectedByStudentGroupId))]
    [MapperIgnoreSource(nameof(DbLessonPolicyViolation.AffectedByStudentGroup))]
    [MapperIgnoreSource(nameof(DbLessonPolicyViolation.AffectedByLessonId))]
    [MapperIgnoreSource(nameof(DbLessonPolicyViolation.AffectedByLesson))]
    [MapperIgnoreSource(nameof(DbLessonPolicyViolation.AffectedByTeacherPreferenceId))]
    [MapperIgnoreSource(nameof(DbLessonPolicyViolation.AffectedByTeacherPreference))]
    [MapperIgnoreSource(nameof(DbLessonPolicyViolation.AffectedByTeacherId))]
    [MapperIgnoreSource(nameof(DbLessonPolicyViolation.AffectedByTeacher))]
    [MapperIgnoreSource(nameof(DbLessonPolicyViolation.AffectedByRoomId))]
    [MapperIgnoreSource(nameof(DbLessonPolicyViolation.AffectedByRoom))]
    [MapperIgnoreSource(nameof(DbLessonPolicyViolation.Lesson))]
    [MapperIgnoreTarget(nameof(LessonPolicyViolation.Payload))]
    [MapperIgnoreTarget(nameof(LessonPolicyViolation.Lesson))]
    private static partial LessonPolicyViolation? AutoMapEntityToModel(DbLessonPolicyViolation? entity);
}