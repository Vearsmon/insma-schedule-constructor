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
public static partial class AcademicDisciplineDtoMappingRegister
{
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
    public static partial AcademicDisciplineViewDto? MapModelToViewDto(AcademicDiscipline? model);

    [MapperIgnoreTarget(nameof(AcademicDiscipline.Schedule))]
    public static partial AcademicDiscipline? MapSaveDtoToModel(AcademicDisciplineSaveDto? dto);

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
    [MapperIgnoreSource(nameof(AcademicDiscipline.ExamPayload))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.TestPayload))]
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
}