using Domain.Dto;
using Domain.Dto.RegistryDto;
using Domain.Dto.SaveDto;
using Domain.Dto.ViewDto;
using Domain.Models;
using Domain.Models.RegistryItemModels;
using Riok.Mapperly.Abstractions;

namespace Services.Mapping;

[Mapper]
public static partial class DtoMappingRegister
{
    #region AcademicDiscipline

    [MapperIgnoreSource(nameof(AcademicDiscipline.ScheduleId))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.Schedule))]
    public static partial AcademicDisciplineViewDto? Map(AcademicDiscipline? model);

    [MapperIgnoreTarget(nameof(AcademicDiscipline.Schedule))]
    public static partial AcademicDiscipline? Map(SaveAcademicDisciplineDto dto);

    public static partial AcademicDisciplinePayload? Map(AcademicDisciplinePayloadDto dto);

    [MapperIgnoreSource(nameof(AcademicDisciplineLessonBatchInfo.StudentGroup))]
    [MapperIgnoreSource(nameof(AcademicDisciplineLessonBatchInfo.Teacher))]
    [MapperIgnoreSource(nameof(AcademicDisciplineLessonBatchInfo.Room))]
    public static partial AcademicDisciplineLessonBatchInfoDto? Map(AcademicDisciplineLessonBatchInfo dto);

    [MapperIgnoreTarget(nameof(AcademicDisciplineLessonBatchInfo.StudentGroup))]
    [MapperIgnoreTarget(nameof(AcademicDisciplineLessonBatchInfo.Teacher))]
    [MapperIgnoreTarget(nameof(AcademicDisciplineLessonBatchInfo.Room))]
    public static partial AcademicDisciplineLessonBatchInfo? Map(AcademicDisciplineLessonBatchInfoDto dto);

    public static partial AcademicDisciplineRegistryItemDto Map(AcademicDisciplineRegistryItem item);

    #endregion

    #region Campus

    public static partial Campus? Map(SaveCampusDto? dto);

    public static partial CampusRegistryItemDto Map(CampusRegistryItem item);

    #endregion

    #region Lesson

    [MapperIgnoreSource(nameof(Lesson.ScheduleId))]
    [MapperIgnoreSource(nameof(Lesson.Schedule))]
    [MapperIgnoreSource(nameof(Lesson.AcademicDiscipline))]
    [MapperIgnoreSource(nameof(Lesson.StudentGroup))]
    [MapperIgnoreSource(nameof(Lesson.Teacher))]
    [MapperIgnoreSource(nameof(Lesson.Room))]
    public static partial LessonViewDto? Map(Lesson? model);

    [MapperIgnoreTarget(nameof(Lesson.Schedule))]
    [MapperIgnoreTarget(nameof(Lesson.AcademicDiscipline))]
    [MapperIgnoreTarget(nameof(Lesson.StudentGroup))]
    [MapperIgnoreTarget(nameof(Lesson.Teacher))]
    [MapperIgnoreTarget(nameof(Lesson.Room))]
    [MapperIgnoreTarget(nameof(Lesson.ValidationMessages))]
    [MapValue(nameof(Lesson.CreatedFromDiscipline), false)]
    public static partial Lesson? Map(SaveLessonDto? dto);

    #endregion

    #region Room

    [MapperIgnoreSource(nameof(Room.Campus))]
    public static partial RoomViewDto? Map(Room? model);

    [MapperIgnoreTarget(nameof(Room.Campus))]
    public static partial Room? Map(SaveRoomDto? dto);

    #endregion

    #region Schedule

    public static partial Schedule? Map(SaveScheduleDto? dto);

    public static partial ScheduleRegistryItemDto Map(ScheduleRegistryItem item);

    #endregion

    #region StudentGroup

    [MapperIgnoreSource(nameof(StudentGroup.ScheduleId))]
    [MapperIgnoreSource(nameof(StudentGroup.Schedule))]
    [MapperIgnoreSource(nameof(StudentGroup.Parent))]
    [MapperIgnoreSource(nameof(StudentGroup.ParentId))]
    [MapperIgnoreSource(nameof(StudentGroup.ChildrenFlat))]
    public static partial StudentGroupViewDto? Map(StudentGroup? model);

    [MapperIgnoreSource(nameof(StudentGroup.ScheduleId))]
    [MapperIgnoreSource(nameof(StudentGroup.Schedule))]
    [MapperIgnoreSource(nameof(StudentGroup.Parent))]
    [MapperIgnoreSource(nameof(StudentGroup.ParentId))]
    [MapperIgnoreSource(nameof(StudentGroup.Children))]
    [MapperIgnoreSource(nameof(StudentGroup.ChildrenFlat))]
    [MapperIgnoreSource(nameof(StudentGroup.SemesterNumber))]
    [MapperIgnoreSource(nameof(StudentGroup.Cypher))]
    [MapperIgnoreSource(nameof(StudentGroup.StudentGroupType))]
    public static partial StudentGroupShortViewDto? MapShort(StudentGroup? model);

    [MapperIgnoreSource(nameof(SaveStudentGroupDto.ChildIds))]
    [MapperIgnoreTarget(nameof(StudentGroup.Schedule))]
    [MapperIgnoreTarget(nameof(StudentGroup.Parent))]
    [MapperIgnoreTarget(nameof(StudentGroup.Children))]
    public static partial StudentGroup? Map(SaveStudentGroupDto? dto);

    public static partial StudentGroupRegistryItemDto Map(StudentGroupRegistryItem item);

    #endregion

    #region Teacher

    [MapperIgnoreSource(nameof(Teacher.UserId))]
    [MapperIgnoreSource(nameof(Teacher.User))]
    public static partial TeacherViewDto? Map(Teacher? model);

    [MapperIgnoreTarget(nameof(Teacher.UserId))]
    [MapperIgnoreTarget(nameof(Teacher.User))]
    public static partial Teacher? Map(SaveTeacherDto? dto);

    public static partial TeacherRegistryItemDto Map(TeacherRegistryItem item);

    #endregion

    #region TeacherPreference

    public static partial TeacherPreferenceRegistryItemDto Map(TeacherPreferenceRegistryItem item);

    #endregion
}