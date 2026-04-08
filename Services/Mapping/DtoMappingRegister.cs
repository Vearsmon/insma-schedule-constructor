using Domain.Dto;
using Domain.Dto.RegistryDto;
using Domain.Dto.SaveDto;
using Domain.Dto.ShortDto;
using Domain.Dto.ViewDto;
using Domain.Models;
using Domain.Models.RegistryItemModels;
using Riok.Mapperly.Abstractions;

namespace Services.Mapping;

[Mapper]
public static partial class DtoMappingRegister
{
    #region AcademicDiscipline

    [UserMapping(Default = true)]
    public static AcademicDisciplineLessonBatchInfoDto? Map(AcademicDisciplineLessonBatchInfo? model)
    {
        var dto = MapModelToDto(model);
        if (dto != null && model != null)
        {
            dto.StudentGroupIds = model.StudentGroups.Select(x => x.Id!.Value).ToArray();
        }
        return dto;
    }

    [UserMapping(Default = true)]
    public static AcademicDisciplineLessonBatchInfo? Map(AcademicDisciplineLessonBatchInfoDto? dto)
    {
        var model = MapDtoToModel(dto);
        if (model != null && dto != null)
        {
            model.StudentGroups = dto.StudentGroupIds.Select(x => new StudentGroup { Id = x }).ToArray();
        }
        return model;
    }

    [MapperIgnoreSource(nameof(AcademicDiscipline.ScheduleId))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.Schedule))]
    public static partial AcademicDisciplineViewDto? Map(AcademicDiscipline? model);

    [MapperIgnoreTarget(nameof(AcademicDiscipline.Schedule))]
    public static partial AcademicDiscipline? Map(SaveAcademicDisciplineDto? dto);

    public static partial AcademicDisciplinePayload? Map(AcademicDisciplinePayloadDto? dto);

    public static partial AcademicDisciplineRegistryItemDto? Map(AcademicDisciplineRegistryItem? item);

    [MapperIgnoreSource(nameof(AcademicDisciplineLessonBatchInfo.Teacher))]
    [MapperIgnoreSource(nameof(AcademicDisciplineLessonBatchInfo.Room))]
    [MapperIgnoreSource(nameof(AcademicDisciplineLessonBatchInfo.StudentGroups))]
    [MapperIgnoreTarget(nameof(AcademicDisciplineLessonBatchInfoDto.StudentGroupIds))]
    private static partial AcademicDisciplineLessonBatchInfoDto? MapModelToDto(AcademicDisciplineLessonBatchInfo? dto);

    [MapperIgnoreSource(nameof(AcademicDisciplineLessonBatchInfoDto.StudentGroupIds))]
    [MapperIgnoreTarget(nameof(AcademicDisciplineLessonBatchInfo.Teacher))]
    [MapperIgnoreTarget(nameof(AcademicDisciplineLessonBatchInfo.Room))]
    [MapperIgnoreTarget(nameof(AcademicDisciplineLessonBatchInfo.StudentGroups))]
    private static partial AcademicDisciplineLessonBatchInfo? MapDtoToModel(AcademicDisciplineLessonBatchInfoDto? dto);

    [MapperIgnoreSource(nameof(AcademicDiscipline.ScheduleId))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.Schedule))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.Cypher))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.SemesterNumber))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.AcademicDisciplineTargetType))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.AllowedLessonTypes))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.LecturePayload))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.PracticePayload))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.LabPayload))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.Comment))]
    public static partial AcademicDisciplineShortDto? MapToRootDto(AcademicDiscipline? item);

    #endregion

    #region Campus

    public static partial Campus? Map(SaveCampusDto? dto);

    public static partial CampusRegistryItemDto? Map(CampusRegistryItem? item);

    public static partial CampusShortDto? MapShort(Campus? model);

    #endregion

    #region Lesson

    [UserMapping(Default = true)]
    public static LessonViewDto? Map(Lesson? model)
    {
        var dto = MapToDto(model);
        if (model != null && dto != null)
        {
            dto.StudentGroupIds = model.StudentGroups.Select(x => x.Id!.Value).ToArray();
        }
        return dto;
    }

    [UserMapping(Default = true)]
    public static Lesson? Map(SaveLessonDto? dto)
    {
        var model = MapToModel(dto);
        if (dto != null && model != null)
        {
            model.StudentGroups = dto.StudentGroupIds.Select(x => new StudentGroup { Id = x }).ToArray();
        }
        return model;
    }

    public static partial LessonRegistryItemDto? Map(LessonRegistryItem? item);

    public static partial LessonShortDto? MapShort(Lesson? model);

    [MapperIgnoreSource(nameof(Lesson.ScheduleId))]
    [MapperIgnoreSource(nameof(Lesson.Schedule))]
    [MapperIgnoreSource(nameof(Lesson.AcademicDiscipline))]
    [MapperIgnoreSource(nameof(Lesson.Teacher))]
    [MapperIgnoreSource(nameof(Lesson.Room))]
    [MapperIgnoreSource(nameof(Lesson.StudentGroups))]
    [MapperIgnoreTarget(nameof(LessonViewDto.StudentGroupIds))]
    private static partial LessonViewDto? MapToDto(Lesson? model);

    [MapperIgnoreSource(nameof(SaveLessonDto.StudentGroupIds))]
    [MapperIgnoreTarget(nameof(Lesson.Schedule))]
    [MapperIgnoreTarget(nameof(Lesson.AcademicDiscipline))]
    [MapperIgnoreTarget(nameof(Lesson.Teacher))]
    [MapperIgnoreTarget(nameof(Lesson.Room))]
    [MapperIgnoreTarget(nameof(Lesson.ValidationMessages))]
    [MapperIgnoreTarget(nameof(Lesson.StudentGroups))]
    private static partial Lesson? MapToModel(SaveLessonDto? dto);

    #endregion

    #region Room

    [MapperIgnoreSource(nameof(Room.Campus))]
    public static partial RoomViewDto? Map(Room? model);

    [MapperIgnoreTarget(nameof(Room.Campus))]
    public static partial Room? Map(SaveRoomDto? dto);

    public static partial RoomRegistryItemDto? Map(RoomRegistryItem? item);

    #endregion

    #region Schedule

    public static partial Schedule? Map(SaveScheduleDto? dto);

    public static partial ScheduleRegistryItemDto? Map(ScheduleRegistryItem? item);

    public static partial ScheduleShortDto? MapShort(Schedule? model);

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
    public static partial StudentGroupShortDto? MapShort(StudentGroup? model);

    [MapperIgnoreSource(nameof(SaveStudentGroupDto.ChildIds))]
    [MapperIgnoreTarget(nameof(StudentGroup.Schedule))]
    [MapperIgnoreTarget(nameof(StudentGroup.Parent))]
    [MapperIgnoreTarget(nameof(StudentGroup.Children))]
    public static partial StudentGroup? Map(SaveStudentGroupDto? dto);

    public static partial StudentGroupRegistryItemDto? Map(StudentGroupRegistryItem? item);

    #endregion

    #region Teacher

    [MapperIgnoreSource(nameof(Teacher.UserId))]
    [MapperIgnoreSource(nameof(Teacher.User))]
    public static partial TeacherViewDto? Map(Teacher? model);

    [MapperIgnoreTarget(nameof(Teacher.UserId))]
    [MapperIgnoreTarget(nameof(Teacher.User))]
    public static partial Teacher? Map(SaveTeacherDto? dto);

    public static partial TeacherRegistryItemDto? Map(TeacherRegistryItem? item);

    [MapperIgnoreSource(nameof(Teacher.UserId))]
    [MapperIgnoreSource(nameof(Teacher.User))]
    [MapperIgnoreSource(nameof(Teacher.Contacts))]
    public static partial TeacherShortDto? MapShort(Teacher? model);

    #endregion

    #region TeacherPreference

    public static partial TeacherPreferenceRegistryItemDto? Map(TeacherPreferenceRegistryItem? item);

    #endregion
}