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
    public static LessonBatchInfoDto? MapModelToDto(LessonBatchInfo? model)
    {
        var dto = AutoMapModelToDto(model);
        if (dto != null && model != null)
        {
            dto.StudentGroups = model.StudentGroups.Select(x => new StudentGroupShortDto { Id = x.Id!.Value, Name = x.Name }).ToArray();
        }
        return dto;
    }

    [UserMapping(Default = true)]
    public static LessonBatchInfo? Map(LessonBatchInfoDto? dto)
    {
        var model = AutoMapDtoToModel(dto);
        if (model != null && dto != null)
        {
            model.StudentGroups = dto.StudentGroups.Select(x => new StudentGroup { Id = x.Id }).ToArray();
        }
        return model;
    }

    [MapperIgnoreSource(nameof(AcademicDiscipline.ScheduleId))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.Schedule))]
    public static partial AcademicDisciplineViewDto? MapModelToViewDto(AcademicDiscipline? model);

    [MapperIgnoreTarget(nameof(AcademicDiscipline.Schedule))]
    public static partial AcademicDiscipline? MapSaveDtoToModel(AcademicDisciplineSaveDto? dto);

    [MapperIgnoreTarget(nameof(AcademicDiscipline.Schedule))]
    public static partial void UpdateModelWithSaveDto(AcademicDisciplineSaveDto? dto, AcademicDiscipline? model);

    public static partial AcademicDisciplineRegistryItemDto? MapItemToItemDto(AcademicDisciplineRegistryItem? item);

    [MapperIgnoreSource(nameof(AcademicDiscipline.ScheduleId))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.Schedule))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.AssociatedNames))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.SemesterNumber))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.AcademicDisciplineTargetType))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.AllowedLessonTypes))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.LectureLessonBatchInfos))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.PracticeLessonBatchInfos))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.LabLessonBatchInfos))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.ExamLessonBatchInfos))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.TestLessonBatchInfos))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.Comment))]
    public static partial AcademicDisciplineShortDto? MapModelToShortDto(AcademicDiscipline? model);

    [MapProperty(nameof(LessonBatchInfo.StudentGroups), nameof(LessonBatchInfoDto.StudentGroups), Use = nameof(MapStudentGroupsCollection))]
    [MapProperty(nameof(LessonBatchInfo.Teachers), nameof(LessonBatchInfoDto.TeacherIds), Use = nameof(MapTeachersCollection))]
    [MapProperty(nameof(LessonBatchInfo.Rooms), nameof(LessonBatchInfoDto.RoomIds), Use = nameof(MapRoomsCollection))]
    private static partial LessonBatchInfoDto? AutoMapModelToDto(LessonBatchInfo? dto);

    [MapProperty(nameof(LessonBatchInfoDto.StudentGroups), nameof(LessonBatchInfo.StudentGroups), Use = nameof(MapStudentGroups))]
    [MapProperty(nameof(LessonBatchInfoDto.TeacherIds), nameof(LessonBatchInfo.Teachers), Use = nameof(MapTeacherIds))]
    [MapProperty(nameof(LessonBatchInfoDto.RoomIds), nameof(LessonBatchInfo.Rooms), Use = nameof(MapRoomIds))]
    private static partial LessonBatchInfo? AutoMapDtoToModel(LessonBatchInfoDto? dto);

    private static StudentGroupShortDto[] MapStudentGroupsCollection(StudentGroup[] collection) => collection.Select(x => new StudentGroupShortDto { Id = x.Id!.Value, Name = x.Name }).ToArray();
    private static Guid[] MapTeachersCollection(Teacher[] collection) => collection.Select(x => x.Id!.Value).ToArray();
    private static Guid[] MapRoomsCollection(Room[] collection) => collection.Select(x => x.Id!.Value).ToArray();
    private static StudentGroup[] MapStudentGroups(StudentGroupShortDto[] shortDto) => shortDto.Select(x => new StudentGroup { Id = x.Id }).ToArray();
    private static Teacher[] MapTeacherIds(Guid[] ids) => ids.Select(x => new Teacher { Id = x }).ToArray();
    private static Room[] MapRoomIds(Guid[] ids) => ids.Select(x => new Room { Id = x }).ToArray();
}