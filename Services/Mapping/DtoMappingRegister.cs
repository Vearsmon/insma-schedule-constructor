using Domain.Dto;
using Domain.Dto.RegistryDto;
using Domain.Dto.SaveDto;
using Domain.Dto.ShortDto;
using Domain.Dto.ViewDto;
using Domain.Models;
using Domain.Models.Common;
using Domain.Models.Enums;
using Domain.Models.RegistryItemModels;
using Riok.Mapperly.Abstractions;

namespace Services.Mapping;

[Mapper]
public static partial class DtoMappingRegister
{
    #region AcademicDiscipline

    [UserMapping(Default = true)]
    public static LessonBatchInfoDto? Map(LessonBatchInfo? model)
    {
        var dto = MapModelToDto(model);
        if (dto != null && model != null)
        {
            dto.StudentGroupIds = model.StudentGroups.Select(x => x.Id!.Value).ToArray();
        }
        return dto;
    }

    [UserMapping(Default = true)]
    public static LessonBatchInfo? Map(LessonBatchInfoDto? dto)
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

    [MapperIgnoreSource(nameof(AcademicDiscipline.ScheduleId))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.Schedule))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.AssociatedNames))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.SemesterNumber))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.AcademicDisciplineTargetType))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.AllowedLessonTypes))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.LecturePayload))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.PracticePayload))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.LabPayload))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.Comment))]
    public static partial AcademicDisciplineShortDto? MapToRootDto(AcademicDiscipline? item);

    [MapProperty(nameof(LessonBatchInfo.StudentGroups), nameof(LessonBatchInfoDto.StudentGroupIds), Use = nameof(MapStudentGroupsCollection))]
    [MapProperty(nameof(LessonBatchInfo.Teachers), nameof(LessonBatchInfoDto.TeacherIds), Use = nameof(MapTeachersCollection))]
    [MapProperty(nameof(LessonBatchInfo.Rooms), nameof(LessonBatchInfoDto.RoomIds), Use = nameof(MapRoomsCollection))]
    private static partial LessonBatchInfoDto? MapModelToDto(LessonBatchInfo? dto);

    [MapProperty(nameof(LessonBatchInfoDto.StudentGroupIds), nameof(LessonBatchInfo.StudentGroups), Use = nameof(MapStudentGroupIds))]
    [MapProperty(nameof(LessonBatchInfoDto.TeacherIds), nameof(LessonBatchInfo.Teachers), Use = nameof(MapTeacherIds))]
    [MapProperty(nameof(LessonBatchInfoDto.RoomIds), nameof(LessonBatchInfo.Rooms), Use = nameof(MapRoomIds))]
    private static partial LessonBatchInfo? MapDtoToModel(LessonBatchInfoDto? dto);

    private static Guid[] MapStudentGroupsCollection(StudentGroup[] collection) => collection.Select(x => x.Id!.Value).ToArray();
    private static Guid[] MapTeachersCollection(Teacher[] collection) => collection.Select(x => x.Id!.Value).ToArray();
    private static Guid[] MapRoomsCollection(Room[] collection) => collection.Select(x => x.Id!.Value).ToArray();
    private static StudentGroup[] MapStudentGroupIds(Guid[] ids) => ids.Select(x => new StudentGroup { Id = x }).ToArray();
    private static Teacher[] MapTeacherIds(Guid[] ids) => ids.Select(x => new Teacher { Id = x }).ToArray();
    private static Room[] MapRoomIds(Guid[] ids) => ids.Select(x => new Room { Id = x }).ToArray();

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
            dto.TeacherIds = model.Teachers.Select(x => x.Id!.Value).ToArray();
            dto.RoomIds = model.Rooms.Select(x => x.Id!.Value).ToArray();
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

    [MapperIgnoreSource(nameof(Lesson.ScheduleId))]
    [MapperIgnoreSource(nameof(Lesson.Schedule))]
    [MapperIgnoreSource(nameof(Lesson.AcademicDiscipline))]
    [MapProperty(nameof(Lesson.ValidationMessages), nameof(LessonShortDto.CurrentErrorsMaxLevel), Use = nameof(GetErrorsMaxLevel))]
    public static partial LessonShortDto? MapShort(Lesson? model);

    [MapperIgnoreSource(nameof(Lesson.ScheduleId))]
    [MapperIgnoreSource(nameof(Lesson.Schedule))]
    [MapperIgnoreSource(nameof(Lesson.AcademicDiscipline))]
    [MapperIgnoreSource(nameof(Lesson.StudentGroups))]
    [MapperIgnoreSource(nameof(Lesson.Teachers))]
    [MapperIgnoreSource(nameof(Lesson.Rooms))]
    [MapperIgnoreTarget(nameof(LessonViewDto.StudentGroupIds))]
    [MapperIgnoreTarget(nameof(LessonViewDto.TeacherIds))]
    [MapperIgnoreTarget(nameof(LessonViewDto.RoomIds))]
    private static partial LessonViewDto? MapToDto(Lesson? model);

    [MapperIgnoreSource(nameof(SaveLessonDto.StudentGroupIds))]
    [MapperIgnoreSource(nameof(SaveLessonDto.TeacherIds))]
    [MapperIgnoreSource(nameof(SaveLessonDto.RoomIds))]
    [MapperIgnoreTarget(nameof(Lesson.Schedule))]
    [MapperIgnoreTarget(nameof(Lesson.AcademicDiscipline))]
    [MapperIgnoreTarget(nameof(Lesson.StudentGroups))]
    [MapperIgnoreTarget(nameof(Lesson.Teachers))]
    [MapperIgnoreTarget(nameof(Lesson.Rooms))]
    [MapperIgnoreTarget(nameof(Lesson.ValidationMessages))]
    private static partial Lesson? MapToModel(SaveLessonDto? dto);

    private static LessonValidationErrorType? GetErrorsMaxLevel(LessonValidationMessage[] messages) => messages.Length == 0 ? null : messages.Max(x => x.ErrorType);

    #endregion

    #region Room

    [MapperIgnoreSource(nameof(Room.CampusId))]
    public static partial RoomViewDto? Map(Room? model);

    [MapperIgnoreTarget(nameof(Room.Campus))]
    public static partial Room? Map(SaveRoomDto? dto);

    public static partial RoomRegistryItemDto? Map(RoomRegistryItem? item);

    [MapperIgnoreSource(nameof(Room.CampusId))]
    public static partial RoomShortDto? MapShort(Room? model);

    #endregion

    #region Schedule

    [UserMapping(Default = true)]
    public static Schedule? Map(SaveScheduleDto? dto)
    {
        var model = MapToModel(dto);
        if (dto != null && model != null)
        {
            model.DateInterval = new DateInterval { DateFrom = dto.DateFrom, DateTo = dto.DateTo };
        }
        return model;
    }

    public static partial ScheduleRegistryItemDto? Map(ScheduleRegistryItem? item);

    public static partial ScheduleShortDto? MapShort(Schedule? model);

    [MapperIgnoreSource(nameof(SaveScheduleDto.DateFrom))]
    [MapperIgnoreSource(nameof(SaveScheduleDto.DateTo))]
    [MapperIgnoreTarget(nameof(Schedule.DateInterval))]
    private static partial Schedule? MapToModel(SaveScheduleDto? dto);

    #endregion

    #region StudentGroup

    [MapperIgnoreSource(nameof(StudentGroup.ScheduleId))]
    [MapperIgnoreSource(nameof(StudentGroup.Schedule))]
    [MapperIgnoreSource(nameof(StudentGroup.Parents))]
    [MapperIgnoreSource(nameof(StudentGroup.ChildrenFlat))]
    public static partial StudentGroupViewDto? Map(StudentGroup? model);

    [MapperIgnoreSource(nameof(StudentGroup.ScheduleId))]
    [MapperIgnoreSource(nameof(StudentGroup.Schedule))]
    [MapperIgnoreSource(nameof(StudentGroup.Parents))]
    [MapperIgnoreSource(nameof(StudentGroup.Children))]
    [MapperIgnoreSource(nameof(StudentGroup.ChildrenFlat))]
    [MapperIgnoreSource(nameof(StudentGroup.SemesterNumber))]
    [MapperIgnoreSource(nameof(StudentGroup.StudentGroupType))]
    public static partial StudentGroupShortDto? MapShort(StudentGroup? model);

    [MapperIgnoreSource(nameof(SaveStudentGroupDto.ChildIds))]
    [MapperIgnoreSource(nameof(SaveStudentGroupDto.ParentIds))]
    [MapperIgnoreSource(nameof(SaveStudentGroupDto.SemiGroupToCreateNames))]
    [MapperIgnoreTarget(nameof(StudentGroup.Schedule))]
    [MapperIgnoreTarget(nameof(StudentGroup.Parents))]
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
    public static partial TeacherShortDto? MapShort(Teacher? model);

    #endregion

    #region TeacherPreference

    public static partial TeacherPreferenceRegistryItemDto? Map(TeacherPreferenceRegistryItem? item);

    #endregion
}