using Dal.Entities;
using Domain.Models;
using Domain.Models.Common;
using Domain.Models.Enums;
using Domain.Models.RegistryItemModels;
using Riok.Mapperly.Abstractions;

namespace Dal.Mapping;

[Mapper]
public static partial class MappingRegister
{
    #region Campus

    public static partial Campus? Map(DbCampus? campus);
    public static partial DbCampus? Map(Campus? campus);
    public static partial void Update(Campus campus, DbCampus dbCampus);

    public static partial CampusRegistryItem? MapRegistryItem(DbCampus? entity);

    #endregion

    #region Lesson

    [UserMapping(Default = true)]
    public static Lesson? Map(DbLesson? lesson)
    {
        var model = MapDbToModel(lesson);
        if (lesson is not { Date: not null, TimeFrom: not null, TimeTo: not null })
        {
            return model;
        }
        model!.DateWithTimeInterval ??= new DateWithTimeInterval(lesson.Date.Value,
            new TimeInterval(lesson.TimeFrom!.Value, lesson.TimeTo!.Value));

        return model;
    }

    [UserMapping(Default = true)]
    public static LessonRegistryItem? MapRegistryItem(DbLesson? entity)
    {
        var item = MapDbToRegistryItem(entity);
        if (entity is not { Date: not null, TimeFrom: not null, TimeTo: not null })
        {
            return item;
        }
        item!.DateWithTimeInterval ??= new DateWithTimeInterval(entity.Date.Value,
            new TimeInterval(entity.TimeFrom!.Value, entity.TimeTo!.Value));

        return item;
    }

    [MapProperty($"{nameof(Lesson.DateWithTimeInterval)}.{nameof(Lesson.DateWithTimeInterval.Date)}", nameof(DbLesson.Date))]
    [MapProperty($"{nameof(Lesson.DateWithTimeInterval)}.{nameof(Lesson.DateWithTimeInterval.TimeInterval)}.{nameof(Lesson.DateWithTimeInterval.TimeInterval.TimeFrom)}", nameof(DbLesson.TimeFrom))]
    [MapProperty($"{nameof(Lesson.DateWithTimeInterval)}.{nameof(Lesson.DateWithTimeInterval.TimeInterval)}.{nameof(Lesson.DateWithTimeInterval.TimeInterval.TimeTo)}", nameof(DbLesson.TimeTo))]
    public static partial DbLesson? Map(Lesson? lesson);

    [MapProperty($"{nameof(Lesson.DateWithTimeInterval)}.{nameof(Lesson.DateWithTimeInterval.Date)}", nameof(DbLesson.Date))]
    [MapProperty($"{nameof(Lesson.DateWithTimeInterval)}.{nameof(Lesson.DateWithTimeInterval.TimeInterval)}.{nameof(Lesson.DateWithTimeInterval.TimeInterval.TimeFrom)}", nameof(DbLesson.TimeFrom))]
    [MapProperty($"{nameof(Lesson.DateWithTimeInterval)}.{nameof(Lesson.DateWithTimeInterval.TimeInterval)}.{nameof(Lesson.DateWithTimeInterval.TimeInterval.TimeTo)}", nameof(DbLesson.TimeTo))]
    public static partial void Update(Lesson lesson, DbLesson dbLesson);

    [MapperIgnoreSource(nameof(DbLesson.Date))]
    [MapperIgnoreSource(nameof(DbLesson.TimeFrom))]
    [MapperIgnoreSource(nameof(DbLesson.TimeTo))]
    [MapperIgnoreTarget(nameof(Lesson.DateWithTimeInterval))]
    [MapProperty(nameof(DbLesson.ValidationMessages), nameof(Lesson.ValidationMessages), Use = nameof(MapValidationMessagesCollection))]
    private static partial Lesson? MapDbToModel(DbLesson? lesson);

    [MapperIgnoreSource(nameof(DbLesson.ScheduleId))]
    [MapperIgnoreSource(nameof(DbLesson.Schedule))]
    [MapperIgnoreSource(nameof(DbLesson.AcademicDiscipline))]
    [MapperIgnoreSource(nameof(DbLesson.StudentGroup))]
    [MapperIgnoreSource(nameof(DbLesson.Teacher))]
    [MapperIgnoreSource(nameof(DbLesson.Room))]
    [MapperIgnoreSource(nameof(DbLesson.Date))]
    [MapperIgnoreSource(nameof(DbLesson.TimeFrom))]
    [MapperIgnoreSource(nameof(DbLesson.TimeTo))]
    [MapperIgnoreTarget(nameof(LessonRegistryItem.DateWithTimeInterval))]
    [MapProperty(nameof(DbLesson.ValidationMessages), nameof(LessonRegistryItem.ValidationMessages), Use = nameof(MapValidationMessagesCollection))]
    private static partial LessonRegistryItem? MapDbToRegistryItem(DbLesson? entity);

    private static LessonValidationMessage[] MapValidationMessagesCollection(ICollection<DbLessonValidationMessage> collection) => collection.Select(x => Map(x)!).ToArray();

    #endregion

    #region LessonValidationMessage

    [MapProperty(nameof(DbLessonValidationMessage.AffectedByAcademicDisciplineId), $"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByAcademicDisciplineId)}")]
    [MapProperty(nameof(DbLessonValidationMessage.AffectedByAcademicDiscipline), $"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByAcademicDiscipline)}")]
    [MapProperty(nameof(DbLessonValidationMessage.AffectedByAcademicDisciplineType), $"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByAcademicDisciplineType)}")]
    [MapProperty(nameof(DbLessonValidationMessage.AffectedByStudentGroupId), $"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByStudentGroupId)}")]
    [MapProperty(nameof(DbLessonValidationMessage.AffectedByStudentGroup), $"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByStudentGroup)}")]
    [MapProperty(nameof(DbLessonValidationMessage.AffectedByLessonId), $"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByLessonId)}")]
    [MapProperty(nameof(DbLessonValidationMessage.AffectedByLesson), $"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByLesson)}")]
    [MapProperty(nameof(DbLessonValidationMessage.AffectedByTeacherPreferenceId), $"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByTeacherPreferenceId)}")]
    [MapProperty(nameof(DbLessonValidationMessage.AffectedByTeacherPreference), $"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByTeacherPreference)}")]
    [MapProperty(nameof(DbLessonValidationMessage.AffectedByTeacherId), $"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByTeacherId)}")]
    [MapProperty(nameof(DbLessonValidationMessage.AffectedByTeacher), $"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByTeacher)}")]
    public static partial LessonValidationMessage? Map(DbLessonValidationMessage? lessonValidationMessage);

    [MapProperty($"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByAcademicDisciplineId)}", nameof(DbLessonValidationMessage.AffectedByAcademicDisciplineId))]
    [MapProperty($"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByAcademicDiscipline)}", nameof(DbLessonValidationMessage.AffectedByAcademicDiscipline))]
    [MapProperty($"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByAcademicDisciplineType)}", nameof(DbLessonValidationMessage.AffectedByAcademicDisciplineType))]
    [MapProperty($"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByStudentGroupId)}", nameof(DbLessonValidationMessage.AffectedByStudentGroupId))]
    [MapProperty($"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByStudentGroup)}", nameof(DbLessonValidationMessage.AffectedByStudentGroup))]
    [MapProperty($"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByLessonId)}", nameof(DbLessonValidationMessage.AffectedByLessonId))]
    [MapProperty($"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByLesson)}", nameof(DbLessonValidationMessage.AffectedByLesson))]
    [MapProperty($"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByTeacherPreferenceId)}", nameof(DbLessonValidationMessage.AffectedByTeacherPreferenceId))]
    [MapProperty($"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByTeacherPreference)}", nameof(DbLessonValidationMessage.AffectedByTeacherPreference))]
    [MapProperty($"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByTeacherId)}", nameof(DbLessonValidationMessage.AffectedByTeacherId))]
    [MapProperty($"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByTeacher)}", nameof(DbLessonValidationMessage.AffectedByTeacher))]
    public static partial DbLessonValidationMessage? Map(LessonValidationMessage? lessonValidationMessage);

    [MapProperty($"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByAcademicDisciplineId)}", nameof(DbLessonValidationMessage.AffectedByAcademicDisciplineId))]
    [MapProperty($"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByAcademicDiscipline)}", nameof(DbLessonValidationMessage.AffectedByAcademicDiscipline))]
    [MapProperty($"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByAcademicDisciplineType)}", nameof(DbLessonValidationMessage.AffectedByAcademicDisciplineType))]
    [MapProperty($"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByStudentGroupId)}", nameof(DbLessonValidationMessage.AffectedByStudentGroupId))]
    [MapProperty($"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByStudentGroup)}", nameof(DbLessonValidationMessage.AffectedByStudentGroup))]
    [MapProperty($"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByLessonId)}", nameof(DbLessonValidationMessage.AffectedByLessonId))]
    [MapProperty($"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByLesson)}", nameof(DbLessonValidationMessage.AffectedByLesson))]
    [MapProperty($"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByTeacherPreferenceId)}", nameof(DbLessonValidationMessage.AffectedByTeacherPreferenceId))]
    [MapProperty($"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByTeacherPreference)}", nameof(DbLessonValidationMessage.AffectedByTeacherPreference))]
    [MapProperty($"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByTeacherId)}", nameof(DbLessonValidationMessage.AffectedByTeacherId))]
    [MapProperty($"{nameof(LessonValidationMessage.Payload)}.{nameof(LessonValidationMessage.Payload.AffectedByTeacher)}", nameof(DbLessonValidationMessage.AffectedByTeacher))]
    public static partial void Update(LessonValidationMessage lessonValidationMessage, DbLessonValidationMessage dbLessonValidationMessage);

    #endregion

    #region Room

    public static partial Room? Map(DbRoom? room);
    public static partial DbRoom? Map(Room? room);
    public static partial void Update(Room room, DbRoom dbRoom);

    #endregion

    #region Schedule

    public static partial Schedule? Map(DbSchedule? schedule);
    public static partial DbSchedule? Map(Schedule? schedule);
    public static partial void Update(Schedule schedule, DbSchedule dbSchedule);

    public static partial ScheduleRegistryItem? MapRegistryItem(DbSchedule? entity);

    #endregion

    #region ScheduleSettings

    public static partial ScheduleSettings? Map(DbScheduleSettings? scheduleSettings);
    public static partial DbScheduleSettings? Map(ScheduleSettings? scheduleSettings);
    public static partial void Update(ScheduleSettings scheduleSettings, DbScheduleSettings dbScheduleSettings);

    #endregion

    #region Student

    public static partial Student? Map(DbStudent? student);
    public static partial DbStudent? Map(Student? student);
    public static partial void Update(Student student, DbStudent dbStudent);

    #endregion

    #region StudentGroup

    [MapperIgnoreTarget(nameof(StudentGroup.ChildrenFlat))]
    [MapProperty(nameof(DbStudentGroup.Children), nameof(StudentGroup.Children), Use = nameof(MapChildrenCollection))]
    public static partial StudentGroup? Map(DbStudentGroup? studentGroup);

    [MapperIgnoreSource(nameof(StudentGroup.ChildrenFlat))]
    public static partial DbStudentGroup? Map(StudentGroup? studentGroup);

    [MapperIgnoreSource(nameof(StudentGroup.ChildrenFlat))]
    public static partial void Update(StudentGroup studentGroup, DbStudentGroup dbStudentGroup);

    [MapperIgnoreSource(nameof(DbStudentGroup.ScheduleId))]
    [MapperIgnoreSource(nameof(DbStudentGroup.Schedule))]
    [MapperIgnoreSource(nameof(DbStudentGroup.ParentId))]
    [MapperIgnoreSource(nameof(DbStudentGroup.Parent))]
    [MapperIgnoreSource(nameof(DbStudentGroup.Children))]
    public static partial StudentGroupRegistryItem? MapRegistryItem(DbStudentGroup? entity);

    private static StudentGroup[] MapChildrenCollection(ICollection<DbStudentGroup> collection) => collection.Select(x => Map(x)!).ToArray();

    #endregion

    #region AcademicDiscipline

    [UserMapping(Default = true)]
    public static AcademicDiscipline? Map(DbAcademicDiscipline? entity)
    {
        var model = MapDbToModel(entity);
        var allowedLessonTypes = new List<AcademicDisciplineType>();
        if (model == null || entity == null)
        {
            return null;
        }
        if (entity.IsLectureLessonsAllowed) allowedLessonTypes.Add(AcademicDisciplineType.Lecture);
        if (entity.IsLabLessonsAllowed) allowedLessonTypes.Add(AcademicDisciplineType.Lab);
        if (entity.IsPracticeLessonsAllowed) allowedLessonTypes.Add(AcademicDisciplineType.Practice);
        if (entity.HasExam) allowedLessonTypes.Add(AcademicDisciplineType.Exam);
        if (entity.HasTest) allowedLessonTypes.Add(AcademicDisciplineType.Test);
        model.AllowedLessonTypes = allowedLessonTypes.ToArray();
        return model;
    }

    [UserMapping(Default = true)]
    public static DbAcademicDiscipline? Map(AcademicDiscipline? model)
    {
        var entity = MapModelToDb(model);
        if (model == null || entity == null)
        {
            return null;
        }
        entity.IsLectureLessonsAllowed = model.AllowedLessonTypes.Contains(AcademicDisciplineType.Lecture);
        entity.IsLabLessonsAllowed = model.AllowedLessonTypes.Contains(AcademicDisciplineType.Lab);
        entity.IsPracticeLessonsAllowed = model.AllowedLessonTypes.Contains(AcademicDisciplineType.Practice);
        entity.HasExam = model.AllowedLessonTypes.Contains(AcademicDisciplineType.Exam);
        entity.HasTest = model.AllowedLessonTypes.Contains(AcademicDisciplineType.Test);
        return entity;
    }

    [UserMapping(Default = true)]
    public static void Update(AcademicDiscipline model, DbAcademicDiscipline entity)
    {
        UpdateModelToDb(model, entity);
        entity.IsLectureLessonsAllowed = model.AllowedLessonTypes.Contains(AcademicDisciplineType.Lecture);
        entity.IsLabLessonsAllowed = model.AllowedLessonTypes.Contains(AcademicDisciplineType.Lab);
        entity.IsPracticeLessonsAllowed = model.AllowedLessonTypes.Contains(AcademicDisciplineType.Practice);
        entity.HasExam = model.AllowedLessonTypes.Contains(AcademicDisciplineType.Exam);
        entity.HasTest = model.AllowedLessonTypes.Contains(AcademicDisciplineType.Test);
    }

    [UserMapping(Default = true)]
    public static AcademicDisciplineRegistryItem? MapRegistryItem(DbAcademicDiscipline? entity)
    {
        var item = MapDbToRegistryItem(entity);
        var allowedLessonTypes = new List<AcademicDisciplineType>();
        if (item == null || entity == null)
        {
            return null;
        }
        if (entity.IsLectureLessonsAllowed) allowedLessonTypes.Add(AcademicDisciplineType.Lecture);
        if (entity.IsLabLessonsAllowed) allowedLessonTypes.Add(AcademicDisciplineType.Lab);
        if (entity.IsPracticeLessonsAllowed) allowedLessonTypes.Add(AcademicDisciplineType.Practice);
        if (entity.HasExam) allowedLessonTypes.Add(AcademicDisciplineType.Exam);
        if (entity.HasTest) allowedLessonTypes.Add(AcademicDisciplineType.Test);
        item.AllowedLessonTypes = allowedLessonTypes.ToArray();
        return item;
    }

    [MapperIgnoreSource(nameof(DbAcademicDiscipline.IsLectureLessonsAllowed))]
    [MapperIgnoreSource(nameof(DbAcademicDiscipline.IsLabLessonsAllowed))]
    [MapperIgnoreSource(nameof(DbAcademicDiscipline.IsPracticeLessonsAllowed))]
    [MapperIgnoreSource(nameof(DbAcademicDiscipline.HasExam))]
    [MapperIgnoreSource(nameof(DbAcademicDiscipline.HasTest))]
    [MapperIgnoreTarget(nameof(AcademicDiscipline.AllowedLessonTypes))]
    [MapProperty(nameof(DbAcademicDiscipline.LectureTotalHoursCount), $"{nameof(AcademicDiscipline.LecturePayload)}.{nameof(AcademicDiscipline.LecturePayload.TotalHoursCount)}")]
    [MapProperty(nameof(DbAcademicDiscipline.LectureStudyWeeksCount), $"{nameof(AcademicDiscipline.LecturePayload)}.{nameof(AcademicDiscipline.LecturePayload.StudyWeeksCount)}")]
    [MapProperty(nameof(DbAcademicDiscipline.LectureLessonsPerWeekCount), $"{nameof(AcademicDiscipline.LecturePayload)}.{nameof(AcademicDiscipline.LecturePayload.LessonsPerWeekCount)}")]
    [MapProperty(nameof(DbAcademicDiscipline.AcademicDisciplineLectureLessonBatchInfoId), $"{nameof(AcademicDiscipline.LecturePayload)}.{nameof(AcademicDiscipline.LecturePayload.LessonBatchInfo)}.{nameof(AcademicDiscipline.LecturePayload.LessonBatchInfo.Id)}")]
    [MapProperty(nameof(DbAcademicDiscipline.AcademicDisciplineLectureLessonBatchInfo), $"{nameof(AcademicDiscipline.LecturePayload)}.{nameof(AcademicDiscipline.LecturePayload.LessonBatchInfo)}")]
    [MapProperty(nameof(DbAcademicDiscipline.LabTotalHoursCount), $"{nameof(AcademicDiscipline.LabPayload)}.{nameof(AcademicDiscipline.LabPayload.TotalHoursCount)}")]
    [MapProperty(nameof(DbAcademicDiscipline.LabStudyWeeksCount), $"{nameof(AcademicDiscipline.LabPayload)}.{nameof(AcademicDiscipline.LabPayload.StudyWeeksCount)}")]
    [MapProperty(nameof(DbAcademicDiscipline.LabLessonsPerWeekCount), $"{nameof(AcademicDiscipline.LabPayload)}.{nameof(AcademicDiscipline.LabPayload.LessonsPerWeekCount)}")]
    [MapProperty(nameof(DbAcademicDiscipline.AcademicDisciplineLabLessonBatchInfo), $"{nameof(AcademicDiscipline.LabPayload)}.{nameof(AcademicDiscipline.LabPayload.LessonBatchInfo)}")]
    [MapProperty(nameof(DbAcademicDiscipline.AcademicDisciplineLabLessonBatchInfoId), $"{nameof(AcademicDiscipline.LabPayload)}.{nameof(AcademicDiscipline.LabPayload.LessonBatchInfo)}.{nameof(AcademicDiscipline.LabPayload.LessonBatchInfo.Id)}")]
    [MapProperty(nameof(DbAcademicDiscipline.PracticeTotalHoursCount), $"{nameof(AcademicDiscipline.PracticePayload)}.{nameof(AcademicDiscipline.PracticePayload.TotalHoursCount)}")]
    [MapProperty(nameof(DbAcademicDiscipline.PracticeStudyWeeksCount), $"{nameof(AcademicDiscipline.PracticePayload)}.{nameof(AcademicDiscipline.PracticePayload.StudyWeeksCount)}")]
    [MapProperty(nameof(DbAcademicDiscipline.PracticeLessonsPerWeekCount), $"{nameof(AcademicDiscipline.PracticePayload)}.{nameof(AcademicDiscipline.PracticePayload.LessonsPerWeekCount)}")]
    [MapProperty(nameof(DbAcademicDiscipline.AcademicDisciplinePracticeLessonBatchInfo), $"{nameof(AcademicDiscipline.PracticePayload)}.{nameof(AcademicDiscipline.PracticePayload.LessonBatchInfo)}")]
    [MapProperty(nameof(DbAcademicDiscipline.AcademicDisciplinePracticeLessonBatchInfoId), $"{nameof(AcademicDiscipline.PracticePayload)}.{nameof(AcademicDiscipline.PracticePayload.LessonBatchInfo)}.{nameof(AcademicDiscipline.PracticePayload.LessonBatchInfo.Id)}")]
    private static partial AcademicDiscipline? MapDbToModel(DbAcademicDiscipline? academicDiscipline);

    [MapperIgnoreSource(nameof(AcademicDiscipline.AllowedLessonTypes))]
    [MapperIgnoreTarget(nameof(DbAcademicDiscipline.IsLectureLessonsAllowed))]
    [MapperIgnoreTarget(nameof(DbAcademicDiscipline.IsLabLessonsAllowed))]
    [MapperIgnoreTarget(nameof(DbAcademicDiscipline.IsPracticeLessonsAllowed))]
    [MapperIgnoreTarget(nameof(DbAcademicDiscipline.HasExam))]
    [MapperIgnoreTarget(nameof(DbAcademicDiscipline.HasTest))]
    [MapProperty($"{nameof(AcademicDiscipline.LecturePayload)}.{nameof(AcademicDiscipline.LecturePayload.TotalHoursCount)}", nameof(DbAcademicDiscipline.LectureTotalHoursCount))]
    [MapProperty($"{nameof(AcademicDiscipline.LecturePayload)}.{nameof(AcademicDiscipline.LecturePayload.StudyWeeksCount)}", nameof(DbAcademicDiscipline.LectureStudyWeeksCount))]
    [MapProperty($"{nameof(AcademicDiscipline.LecturePayload)}.{nameof(AcademicDiscipline.LecturePayload.LessonsPerWeekCount)}", nameof(DbAcademicDiscipline.LectureLessonsPerWeekCount))]
    [MapProperty($"{nameof(AcademicDiscipline.LecturePayload)}.{nameof(AcademicDiscipline.LecturePayload.LessonBatchInfo)}", nameof(DbAcademicDiscipline.AcademicDisciplineLectureLessonBatchInfo))]
    [MapProperty($"{nameof(AcademicDiscipline.LecturePayload)}.{nameof(AcademicDiscipline.LecturePayload.LessonBatchInfo)}.{nameof(AcademicDiscipline.LecturePayload.LessonBatchInfo.Id)}", nameof(DbAcademicDiscipline.AcademicDisciplineLectureLessonBatchInfoId))]
    [MapProperty($"{nameof(AcademicDiscipline.LabPayload)}.{nameof(AcademicDiscipline.LabPayload.TotalHoursCount)}", nameof(DbAcademicDiscipline.LabTotalHoursCount))]
    [MapProperty($"{nameof(AcademicDiscipline.LabPayload)}.{nameof(AcademicDiscipline.LabPayload.StudyWeeksCount)}", nameof(DbAcademicDiscipline.LabStudyWeeksCount))]
    [MapProperty($"{nameof(AcademicDiscipline.LabPayload)}.{nameof(AcademicDiscipline.LabPayload.LessonsPerWeekCount)}", nameof(DbAcademicDiscipline.LabLessonsPerWeekCount))]
    [MapProperty($"{nameof(AcademicDiscipline.LabPayload)}.{nameof(AcademicDiscipline.LabPayload.LessonBatchInfo)}", nameof(DbAcademicDiscipline.AcademicDisciplineLabLessonBatchInfo))]
    [MapProperty($"{nameof(AcademicDiscipline.LabPayload)}.{nameof(AcademicDiscipline.LabPayload.LessonBatchInfo)}.{nameof(AcademicDiscipline.LabPayload.LessonBatchInfo.Id)}", nameof(DbAcademicDiscipline.AcademicDisciplineLabLessonBatchInfoId))]
    [MapProperty($"{nameof(AcademicDiscipline.PracticePayload)}.{nameof(AcademicDiscipline.PracticePayload.TotalHoursCount)}", nameof(DbAcademicDiscipline.PracticeTotalHoursCount))]
    [MapProperty($"{nameof(AcademicDiscipline.PracticePayload)}.{nameof(AcademicDiscipline.PracticePayload.StudyWeeksCount)}", nameof(DbAcademicDiscipline.PracticeStudyWeeksCount))]
    [MapProperty($"{nameof(AcademicDiscipline.PracticePayload)}.{nameof(AcademicDiscipline.PracticePayload.LessonsPerWeekCount)}", nameof(DbAcademicDiscipline.PracticeLessonsPerWeekCount))]
    [MapProperty($"{nameof(AcademicDiscipline.PracticePayload)}.{nameof(AcademicDiscipline.PracticePayload.LessonBatchInfo)}", nameof(DbAcademicDiscipline.AcademicDisciplinePracticeLessonBatchInfo))]
    [MapProperty($"{nameof(AcademicDiscipline.PracticePayload)}.{nameof(AcademicDiscipline.PracticePayload.LessonBatchInfo)}.{nameof(AcademicDiscipline.PracticePayload.LessonBatchInfo.Id)}", nameof(DbAcademicDiscipline.AcademicDisciplinePracticeLessonBatchInfoId))]
    private static partial DbAcademicDiscipline? MapModelToDb(AcademicDiscipline? academicDiscipline);

    [MapperIgnoreSource(nameof(AcademicDiscipline.AllowedLessonTypes))]
    [MapperIgnoreTarget(nameof(DbAcademicDiscipline.IsLectureLessonsAllowed))]
    [MapperIgnoreTarget(nameof(DbAcademicDiscipline.IsLabLessonsAllowed))]
    [MapperIgnoreTarget(nameof(DbAcademicDiscipline.IsPracticeLessonsAllowed))]
    [MapperIgnoreTarget(nameof(DbAcademicDiscipline.HasExam))]
    [MapperIgnoreTarget(nameof(DbAcademicDiscipline.HasTest))]
    [MapProperty($"{nameof(AcademicDiscipline.LecturePayload)}.{nameof(AcademicDiscipline.LecturePayload.TotalHoursCount)}", nameof(DbAcademicDiscipline.LectureTotalHoursCount))]
    [MapProperty($"{nameof(AcademicDiscipline.LecturePayload)}.{nameof(AcademicDiscipline.LecturePayload.StudyWeeksCount)}", nameof(DbAcademicDiscipline.LectureStudyWeeksCount))]
    [MapProperty($"{nameof(AcademicDiscipline.LecturePayload)}.{nameof(AcademicDiscipline.LecturePayload.LessonsPerWeekCount)}", nameof(DbAcademicDiscipline.LectureLessonsPerWeekCount))]
    [MapProperty($"{nameof(AcademicDiscipline.LecturePayload)}.{nameof(AcademicDiscipline.LecturePayload.LessonBatchInfo)}", nameof(DbAcademicDiscipline.AcademicDisciplineLectureLessonBatchInfo))]
    [MapProperty($"{nameof(AcademicDiscipline.LecturePayload)}.{nameof(AcademicDiscipline.LecturePayload.LessonBatchInfo)}.{nameof(AcademicDiscipline.LecturePayload.LessonBatchInfo.Id)}", nameof(DbAcademicDiscipline.AcademicDisciplineLectureLessonBatchInfoId))]
    [MapProperty($"{nameof(AcademicDiscipline.LabPayload)}.{nameof(AcademicDiscipline.LabPayload.TotalHoursCount)}", nameof(DbAcademicDiscipline.LabTotalHoursCount))]
    [MapProperty($"{nameof(AcademicDiscipline.LabPayload)}.{nameof(AcademicDiscipline.LabPayload.StudyWeeksCount)}", nameof(DbAcademicDiscipline.LabStudyWeeksCount))]
    [MapProperty($"{nameof(AcademicDiscipline.LabPayload)}.{nameof(AcademicDiscipline.LabPayload.LessonsPerWeekCount)}", nameof(DbAcademicDiscipline.LabLessonsPerWeekCount))]
    [MapProperty($"{nameof(AcademicDiscipline.LabPayload)}.{nameof(AcademicDiscipline.LabPayload.LessonBatchInfo)}", nameof(DbAcademicDiscipline.AcademicDisciplineLabLessonBatchInfo))]
    [MapProperty($"{nameof(AcademicDiscipline.LabPayload)}.{nameof(AcademicDiscipline.LabPayload.LessonBatchInfo)}.{nameof(AcademicDiscipline.LabPayload.LessonBatchInfo.Id)}", nameof(DbAcademicDiscipline.AcademicDisciplineLabLessonBatchInfoId))]
    [MapProperty($"{nameof(AcademicDiscipline.PracticePayload)}.{nameof(AcademicDiscipline.PracticePayload.TotalHoursCount)}", nameof(DbAcademicDiscipline.PracticeTotalHoursCount))]
    [MapProperty($"{nameof(AcademicDiscipline.PracticePayload)}.{nameof(AcademicDiscipline.PracticePayload.StudyWeeksCount)}", nameof(DbAcademicDiscipline.PracticeStudyWeeksCount))]
    [MapProperty($"{nameof(AcademicDiscipline.PracticePayload)}.{nameof(AcademicDiscipline.PracticePayload.LessonsPerWeekCount)}", nameof(DbAcademicDiscipline.PracticeLessonsPerWeekCount))]
    [MapProperty($"{nameof(AcademicDiscipline.PracticePayload)}.{nameof(AcademicDiscipline.PracticePayload.LessonBatchInfo)}", nameof(DbAcademicDiscipline.AcademicDisciplinePracticeLessonBatchInfo))]
    [MapProperty($"{nameof(AcademicDiscipline.PracticePayload)}.{nameof(AcademicDiscipline.PracticePayload.LessonBatchInfo)}.{nameof(AcademicDiscipline.PracticePayload.LessonBatchInfo.Id)}", nameof(DbAcademicDiscipline.AcademicDisciplinePracticeLessonBatchInfoId))]
    private static partial void UpdateModelToDb(AcademicDiscipline academicDiscipline, DbAcademicDiscipline dbAcademicDiscipline);

    [MapperIgnoreSource(nameof(DbAcademicDiscipline.ScheduleId))]
    [MapperIgnoreSource(nameof(DbAcademicDiscipline.Schedule))]
    [MapperIgnoreSource(nameof(DbAcademicDiscipline.IsLectureLessonsAllowed))]
    [MapperIgnoreSource(nameof(DbAcademicDiscipline.IsLabLessonsAllowed))]
    [MapperIgnoreSource(nameof(DbAcademicDiscipline.IsPracticeLessonsAllowed))]
    [MapperIgnoreSource(nameof(DbAcademicDiscipline.HasExam))]
    [MapperIgnoreSource(nameof(DbAcademicDiscipline.HasTest))]
    [MapperIgnoreTarget(nameof(AcademicDiscipline.AllowedLessonTypes))]
    [MapProperty(nameof(DbAcademicDiscipline.LectureTotalHoursCount), $"{nameof(AcademicDisciplineRegistryItem.LecturePayload)}.{nameof(AcademicDisciplineRegistryItem.LecturePayload.TotalHoursCount)}")]
    [MapProperty(nameof(DbAcademicDiscipline.LectureStudyWeeksCount), $"{nameof(AcademicDisciplineRegistryItem.LecturePayload)}.{nameof(AcademicDisciplineRegistryItem.LecturePayload.StudyWeeksCount)}")]
    [MapProperty(nameof(DbAcademicDiscipline.LectureLessonsPerWeekCount), $"{nameof(AcademicDisciplineRegistryItem.LecturePayload)}.{nameof(AcademicDisciplineRegistryItem.LecturePayload.LessonsPerWeekCount)}")]
    [MapProperty(nameof(DbAcademicDiscipline.AcademicDisciplineLectureLessonBatchInfoId), $"{nameof(AcademicDisciplineRegistryItem.LecturePayload)}.{nameof(AcademicDisciplineRegistryItem.LecturePayload.LessonBatchInfo)}.{nameof(AcademicDisciplineRegistryItem.LecturePayload.LessonBatchInfo.Id)}")]
    [MapProperty(nameof(DbAcademicDiscipline.AcademicDisciplineLectureLessonBatchInfo), $"{nameof(AcademicDisciplineRegistryItem.LecturePayload)}.{nameof(AcademicDisciplineRegistryItem.LecturePayload.LessonBatchInfo)}")]
    [MapProperty(nameof(DbAcademicDiscipline.LabTotalHoursCount), $"{nameof(AcademicDisciplineRegistryItem.LabPayload)}.{nameof(AcademicDisciplineRegistryItem.LabPayload.TotalHoursCount)}")]
    [MapProperty(nameof(DbAcademicDiscipline.LabStudyWeeksCount), $"{nameof(AcademicDisciplineRegistryItem.LabPayload)}.{nameof(AcademicDisciplineRegistryItem.LabPayload.StudyWeeksCount)}")]
    [MapProperty(nameof(DbAcademicDiscipline.LabLessonsPerWeekCount), $"{nameof(AcademicDisciplineRegistryItem.LabPayload)}.{nameof(AcademicDisciplineRegistryItem.LabPayload.LessonsPerWeekCount)}")]
    [MapProperty(nameof(DbAcademicDiscipline.AcademicDisciplineLabLessonBatchInfo), $"{nameof(AcademicDisciplineRegistryItem.LabPayload)}.{nameof(AcademicDisciplineRegistryItem.LabPayload.LessonBatchInfo)}")]
    [MapProperty(nameof(DbAcademicDiscipline.AcademicDisciplineLabLessonBatchInfoId), $"{nameof(AcademicDisciplineRegistryItem.LabPayload)}.{nameof(AcademicDisciplineRegistryItem.LabPayload.LessonBatchInfo)}.{nameof(AcademicDisciplineRegistryItem.LabPayload.LessonBatchInfo.Id)}")]
    [MapProperty(nameof(DbAcademicDiscipline.PracticeTotalHoursCount), $"{nameof(AcademicDisciplineRegistryItem.PracticePayload)}.{nameof(AcademicDisciplineRegistryItem.PracticePayload.TotalHoursCount)}")]
    [MapProperty(nameof(DbAcademicDiscipline.PracticeStudyWeeksCount), $"{nameof(AcademicDisciplineRegistryItem.PracticePayload)}.{nameof(AcademicDisciplineRegistryItem.PracticePayload.StudyWeeksCount)}")]
    [MapProperty(nameof(DbAcademicDiscipline.PracticeLessonsPerWeekCount), $"{nameof(AcademicDisciplineRegistryItem.PracticePayload)}.{nameof(AcademicDisciplineRegistryItem.PracticePayload.LessonsPerWeekCount)}")]
    [MapProperty(nameof(DbAcademicDiscipline.AcademicDisciplinePracticeLessonBatchInfo), $"{nameof(AcademicDisciplineRegistryItem.PracticePayload)}.{nameof(AcademicDisciplineRegistryItem.PracticePayload.LessonBatchInfo)}")]
    [MapProperty(nameof(DbAcademicDiscipline.AcademicDisciplinePracticeLessonBatchInfoId), $"{nameof(AcademicDisciplineRegistryItem.PracticePayload)}.{nameof(AcademicDisciplineRegistryItem.PracticePayload.LessonBatchInfo)}.{nameof(AcademicDisciplineRegistryItem.PracticePayload.LessonBatchInfo.Id)}")]
    private static partial AcademicDisciplineRegistryItem? MapDbToRegistryItem(DbAcademicDiscipline? entity);

    #endregion

    #region AcademicDisciplineBatchInfo

    [UserMapping(Default = true)]
    public static AcademicDisciplineLessonBatchInfo? Map(DbAcademicDisciplineLessonBatchInfo? entity)
    {
        var model = MapDbToModel(entity);
        if (model != null && entity != null)
        {
            model.DateInterval = new DateInterval(entity.DateFrom, entity.DateTo);
        }

        return model;
    }

    [MapProperty($"{nameof(AcademicDisciplineLessonBatchInfo.DateInterval)}.{nameof(AcademicDisciplineLessonBatchInfo.DateInterval.DateFrom)}", nameof(DbAcademicDisciplineLessonBatchInfo.DateFrom))]
    [MapProperty($"{nameof(AcademicDisciplineLessonBatchInfo.DateInterval)}.{nameof(AcademicDisciplineLessonBatchInfo.DateInterval.DateTo)}", nameof(DbAcademicDisciplineLessonBatchInfo.DateTo))]
    public static partial DbAcademicDisciplineLessonBatchInfo? Map(AcademicDisciplineLessonBatchInfo? academicDisciplineLessonBatchInfo);

    [MapProperty($"{nameof(AcademicDisciplineLessonBatchInfo.DateInterval)}.{nameof(AcademicDisciplineLessonBatchInfo.DateInterval.DateFrom)}", nameof(DbAcademicDisciplineLessonBatchInfo.DateFrom))]
    [MapProperty($"{nameof(AcademicDisciplineLessonBatchInfo.DateInterval)}.{nameof(AcademicDisciplineLessonBatchInfo.DateInterval.DateTo)}", nameof(DbAcademicDisciplineLessonBatchInfo.DateTo))]
    public static partial void Update(AcademicDisciplineLessonBatchInfo academicDisciplineLessonBatchInfo, DbAcademicDisciplineLessonBatchInfo dbAcademicDisciplineLessonBatchInfo);

    [MapperIgnoreSource(nameof(DbAcademicDisciplineLessonBatchInfo.DateFrom))]
    [MapperIgnoreSource(nameof(DbAcademicDisciplineLessonBatchInfo.DateTo))]
    [MapperIgnoreTarget(nameof(AcademicDisciplineLessonBatchInfo.DateInterval))]
    private static partial AcademicDisciplineLessonBatchInfo? MapDbToModel(DbAcademicDisciplineLessonBatchInfo? academicDisciplineLessonBatchInfo);

    #endregion

    #region Teacher

    public static partial Teacher? Map(DbTeacher? teacher);
    public static partial DbTeacher? Map(Teacher? teacher);
    public static partial void Update(Teacher teacher, DbTeacher dbTeacher);

    [MapperIgnoreSource(nameof(DbTeacher.UserId))]
    [MapperIgnoreSource(nameof(DbTeacher.User))]
    public static partial TeacherRegistryItem? MapRegistryItem(DbTeacher? entity);

    #endregion

    #region TeacherPreference

    [UserMapping(Default = true)]
    public static TeacherPreference? Map(DbTeacherPreference? teacherPreference)
    {
        var model = MapDbToModel(teacherPreference);
        if (teacherPreference is { DayOfWeek: not null, TimeFrom: not null, TimeTo: not null })
        {
            model!.DayOfWeekTimeInterval ??= new DayOfWeekTimeInterval(teacherPreference.DayOfWeek.Value,
                new TimeInterval(teacherPreference.TimeFrom!.Value, teacherPreference.TimeTo!.Value));
        }

        return model;
    }

    [MapProperty($"{nameof(TeacherPreference.DayOfWeekTimeInterval)}.{nameof(TeacherPreference.DayOfWeekTimeInterval.DayOfWeek)}", nameof(DbTeacherPreference.DayOfWeek))]
    [MapProperty($"{nameof(TeacherPreference.DayOfWeekTimeInterval)}.{nameof(TeacherPreference.DayOfWeekTimeInterval.TimeInterval)}.{nameof(TeacherPreference.DayOfWeekTimeInterval.TimeInterval.TimeFrom)}", nameof(DbTeacherPreference.TimeFrom))]
    [MapProperty($"{nameof(TeacherPreference.DayOfWeekTimeInterval)}.{nameof(TeacherPreference.DayOfWeekTimeInterval.TimeInterval)}.{nameof(TeacherPreference.DayOfWeekTimeInterval.TimeInterval.TimeTo)}", nameof(DbTeacherPreference.TimeTo))]
    public static partial DbTeacherPreference? Map(TeacherPreference? teacherPreference);

    [MapProperty($"{nameof(TeacherPreference.DayOfWeekTimeInterval)}.{nameof(TeacherPreference.DayOfWeekTimeInterval.DayOfWeek)}", nameof(DbTeacherPreference.DayOfWeek))]
    [MapProperty($"{nameof(TeacherPreference.DayOfWeekTimeInterval)}.{nameof(TeacherPreference.DayOfWeekTimeInterval.TimeInterval)}.{nameof(TeacherPreference.DayOfWeekTimeInterval.TimeInterval.TimeFrom)}", nameof(DbTeacherPreference.TimeFrom))]
    [MapProperty($"{nameof(TeacherPreference.DayOfWeekTimeInterval)}.{nameof(TeacherPreference.DayOfWeekTimeInterval.TimeInterval)}.{nameof(TeacherPreference.DayOfWeekTimeInterval.TimeInterval.TimeTo)}", nameof(DbTeacherPreference.TimeTo))]
    public static partial void Update(TeacherPreference teacherPreference, DbTeacherPreference dbTeacherPreference);

    [MapperIgnoreSource(nameof(DbTeacherPreference.ScheduleId))]
    [MapperIgnoreSource(nameof(DbTeacherPreference.Schedule))]
    [MapperIgnoreSource(nameof(DbTeacherPreference.TeacherId))]
    [MapperIgnoreSource(nameof(DbTeacherPreference.Teacher))]
    [MapperIgnoreSource(nameof(DbTeacherPreference.RoomId))]
    [MapperIgnoreSource(nameof(DbTeacherPreference.Room))]
    [MapperIgnoreSource(nameof(DbTeacherPreference.DayOfWeek))]
    [MapperIgnoreSource(nameof(DbTeacherPreference.TimeFrom))]
    [MapperIgnoreSource(nameof(DbTeacherPreference.TimeTo))]
    [MapperIgnoreSource(nameof(DbTeacherPreference.TeacherPreferenceType))]
    [MapperIgnoreSource(nameof(DbTeacherPreference.Comment))]
    public static partial TeacherPreferenceRegistryItem? MapRegistryItem(DbTeacherPreference? entity);

    [MapperIgnoreSource(nameof(DbTeacherPreference.DayOfWeek))]
    [MapperIgnoreSource(nameof(DbTeacherPreference.TimeFrom))]
    [MapperIgnoreSource(nameof(DbTeacherPreference.TimeTo))]
    [MapperIgnoreTarget(nameof(TeacherPreference.DayOfWeekTimeInterval))]
    private static partial TeacherPreference? MapDbToModel(DbTeacherPreference? teacherPreference);

    #endregion

    #region User

    public static partial User? Map(DbUser? user);
    public static partial DbUser? Map(User? user);
    public static partial void Update(User user, DbUser dbUser);

    #endregion
}
    