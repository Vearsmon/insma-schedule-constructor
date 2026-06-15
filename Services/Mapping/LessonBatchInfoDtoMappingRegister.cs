using Domain.Dto;
using Domain.Dto.SaveDto;
using Domain.Dto.ShortDto;
using Domain.Models;
using Riok.Mapperly.Abstractions;

namespace Services.Mapping;

[Mapper]
public static partial class LessonBatchInfoDtoMappingRegister
{
    [UserMapping(Default = true)]
    public static LessonBatchInfo? MapSaveDtoToModel(LessonBatchInfoSaveDto? dto)
    {
        var model = AutoMapSaveDtoToModel(dto);
        if (dto == null) return model;
        model!.DayOfWeekTimeIntervals = dto.DayOfWeekTimeIntervals.Select(x => new DayOfWeekTimeIntervalAssignment
        {
            Id = x.Id,
            DayOfWeekTimeInterval = x.DayOfWeekTimeInterval,
        }).ToArray();
        return model;
    }

    [UserMapping(Default = true)]
    public static LessonBatchInfoShortDto? MapModelToShortDto(LessonBatchInfo? model)
    {
        var dto = AutoMapModelToShortDto(model);
        if (model == null) return dto;
        dto!.StudentGroups = model.StudentGroups.Select(StudentGroupDtoMappingRegister.MapModelToShortDto).ToArray()!;
        dto.Teachers = model.Teachers.Select(TeacherDtoMappingRegister.MapModelToShortDto).ToArray()!;
        dto.Rooms = model.Rooms.Select(RoomDtoMappingRegister.MapModelToShortDto).ToArray()!;
        return dto;
    }

    [MapperIgnoreSource(nameof(LessonBatchInfo.AcademicDisciplineId))]
    [MapperIgnoreSource(nameof(LessonBatchInfo.AcademicDiscipline))]
    [MapperIgnoreSource(nameof(LessonBatchInfo.Type))]
    [MapperIgnoreSource(nameof(LessonBatchInfo.Violations))]
    [MapProperty(nameof(LessonBatchInfo.StudentGroups), nameof(LessonBatchInfoDto.StudentGroups), Use = nameof(MapStudentGroupsCollection))]
    [MapProperty(nameof(LessonBatchInfo.Teachers), nameof(LessonBatchInfoDto.TeacherIds), Use = nameof(MapTeachersCollection))]
    [MapProperty(nameof(LessonBatchInfo.Rooms), nameof(LessonBatchInfoDto.RoomIds), Use = nameof(MapRoomsCollection))]
    public static partial LessonBatchInfoDto? MapModelToDto(LessonBatchInfo? model);

    [MapperIgnoreTarget(nameof(LessonBatchInfo.AcademicDisciplineId))]
    [MapperIgnoreTarget(nameof(LessonBatchInfo.AcademicDiscipline))]
    [MapperIgnoreTarget(nameof(LessonBatchInfo.Type))]
    [MapperIgnoreTarget(nameof(LessonBatchInfo.Violations))]
    [MapProperty(nameof(LessonBatchInfoDto.StudentGroups), nameof(LessonBatchInfo.StudentGroups), Use = nameof(MapStudentGroups))]
    [MapProperty(nameof(LessonBatchInfoDto.TeacherIds), nameof(LessonBatchInfo.Teachers), Use = nameof(MapTeacherIds))]
    [MapProperty(nameof(LessonBatchInfoDto.RoomIds), nameof(LessonBatchInfo.Rooms), Use = nameof(MapRoomIds))]
    public static partial LessonBatchInfo? MapDtoToModel(LessonBatchInfoDto? dto);

    [MapperIgnoreSource(nameof(LessonBatchInfoSaveDto.DayOfWeekTimeIntervals))]
    [MapperIgnoreTarget(nameof(LessonBatchInfo.AcademicDisciplineId))]
    [MapperIgnoreTarget(nameof(LessonBatchInfo.AcademicDiscipline))]
    [MapperIgnoreTarget(nameof(LessonBatchInfo.Type))]
    [MapperIgnoreTarget(nameof(LessonBatchInfo.Violations))]
    [MapperIgnoreTarget(nameof(LessonBatchInfo.DayOfWeekTimeIntervals))]
    [MapProperty(nameof(LessonBatchInfoSaveDto.StudentGroupIds), nameof(LessonBatchInfo.StudentGroups), Use = nameof(MapStudentGroupIds))]
    [MapProperty(nameof(LessonBatchInfoSaveDto.TeacherIds), nameof(LessonBatchInfo.Teachers), Use = nameof(MapTeacherIds))]
    [MapProperty(nameof(LessonBatchInfoSaveDto.RoomIds), nameof(LessonBatchInfo.Rooms), Use = nameof(MapRoomIds))]
    private static partial LessonBatchInfo? AutoMapSaveDtoToModel(LessonBatchInfoSaveDto? dto);

    [MapperIgnoreSource(nameof(LessonBatchInfo.StudentGroups))]
    [MapperIgnoreSource(nameof(LessonBatchInfo.Teachers))]
    [MapperIgnoreSource(nameof(LessonBatchInfo.Rooms))]
    [MapperIgnoreSource(nameof(LessonBatchInfo.RepeatType))]
    [MapperIgnoreSource(nameof(LessonBatchInfo.DateInterval))]
    [MapperIgnoreSource(nameof(LessonBatchInfo.Violations))]
    [MapperIgnoreTarget(nameof(LessonBatchInfoShortDto.StudentGroups))]
    [MapperIgnoreTarget(nameof(LessonBatchInfoShortDto.Teachers))]
    [MapperIgnoreTarget(nameof(LessonBatchInfoShortDto.Rooms))]
    [MapperIgnoreTarget(nameof(LessonBatchInfoShortDto.LessonPolicyViolationDescription))]
    [MapperIgnoreTarget(nameof(LessonBatchInfoShortDto.CurrentErrorsMaxLevel))]
    private static partial LessonBatchInfoShortDto? AutoMapModelToShortDto(LessonBatchInfo? model);

    private static StudentGroupShortDto[] MapStudentGroupsCollection(StudentGroup[] collection) => collection.Select(x => new StudentGroupShortDto { Id = x.Id!.Value, Name = x.Name }).ToArray();
    private static Guid[] MapTeachersCollection(Teacher[] collection) => collection.Select(x => x.Id!.Value).ToArray();
    private static Guid[] MapRoomsCollection(Room[] collection) => collection.Select(x => x.Id!.Value).ToArray();
    private static StudentGroup[] MapStudentGroups(StudentGroupShortDto[] shortDto) => shortDto.Select(x => new StudentGroup { Id = x.Id }).ToArray();
    private static StudentGroup[] MapStudentGroupIds(Guid[] ids) => ids.Select(x => new StudentGroup { Id = x }).ToArray();
    private static Teacher[] MapTeacherIds(Guid[] ids) => ids.Select(x => new Teacher { Id = x }).ToArray();
    private static Room[] MapRoomIds(Guid[] ids) => ids.Select(x => new Room { Id = x }).ToArray();
}