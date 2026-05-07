using System.Text.Json;
using Dal.Entities;
using Domain.Models.Common;
using Domain.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Dal;

public class InsmaScheduleContext(DbContextOptions options) : DbContextBase(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<DbUser>(UserConfigure);
        builder.Entity<DbCampus>(CampusConfigure);
        builder.Entity<DbSchedule>(ScheduleConfigure);
        builder.Entity<DbRoom>(RoomConfigure);
        builder.Entity<DbTeacher>(TeacherConfigure);
        builder.Entity<DbTeacherPreference>(TeacherPreferenceConfigure);
        builder.Entity<DbStudentGroup>(StudentGroupConfigure);
        builder.Entity<DbStudentGroupLink>(StudentGroupLinkConfigure);
        builder.Entity<DbStudent>(StudentConfigure);
        builder.Entity<DbAcademicDiscipline>(AcademicDisciplineConfigure);
        builder.Entity<DbLessonBatchInfo>(LessonBatchInfoConfigure);
        builder.Entity<DbLessonBatchInfoStudentGroup>(LessonBatchInfoStudentGroupConfigure);
        builder.Entity<DbLessonBatchInfoTeacher>(LessonBatchInfoTeacherConfigure);
        builder.Entity<DbLessonBatchInfoRoom>(LessonBatchInfoRoomConfigure);
        builder.Entity<DbLesson>(LessonConfigure);
        builder.Entity<DbLessonStudentGroup>(LessonStudentGroupConfigure);
        builder.Entity<DbLessonTeacher>(LessonTeacherConfigure);
        builder.Entity<DbLessonRoom>(LessonRoomConfigure);
        builder.Entity<DbLessonValidationMessage>(LessonValidationMessageConfigure);

        base.OnModelCreating(builder);
    }

    private void AcademicDisciplineConfigure(EntityTypeBuilder<DbAcademicDiscipline> builder)
    {
        builder.HasOne(x => x.Schedule)
            .WithMany()
            .HasForeignKey(x => x.ScheduleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.AcademicDisciplineLectureLessonBatchInfos)
            .WithOne()
            .HasConstraintName("fk_lesson_batch_info_lecture")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.AcademicDisciplinePracticeLessonBatchInfos)
            .WithOne()
            .HasConstraintName("fk_lesson_batch_info_practice")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.AcademicDisciplineLabLessonBatchInfos)
            .WithOne()
            .HasConstraintName("fk_lesson_batch_info_lab")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.AcademicDisciplineExamLessonBatchInfos)
            .WithOne()
            .HasConstraintName("fk_lesson_batch_info_exam")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.AcademicDisciplineTestLessonBatchInfos)
            .WithOne()
            .HasConstraintName("fk_lesson_batch_info_test")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.AcademicDisciplineTargetType)
            .HasConversion(new EnumToStringConverter<AcademicDisciplineTargetType>());
    }

    private void LessonBatchInfoConfigure(EntityTypeBuilder<DbLessonBatchInfo> builder)
    {
        // builder.HasMany(x => x.StudentGroups)
        //     .WithMany()
        //     .UsingEntity(
        //         "lesson_batch_info_student_group",
        //         r => r.HasOne(typeof(DbStudentGroup)).WithMany().HasForeignKey("student_group_id").HasPrincipalKey(nameof(DbStudentGroup.Id)).HasConstraintName("fk_lesson_batch_info_student_group_student_group"),
        //         l => l.HasOne(typeof(DbLessonBatchInfo)).WithMany().HasForeignKey("lesson_batch_info_id").HasPrincipalKey(nameof(DbLessonBatchInfo.Id)).HasConstraintName("fk_lesson_batch_info_student_group_lesson_batch_info"),
        //         j => j.HasKey("student_group_id", "lesson_batch_info_id"));
        //
        // builder.HasMany(x => x.Teachers)
        //     .WithMany()
        //     .UsingEntity(
        //         "lesson_batch_info_teacher",
        //         r => r.HasOne(typeof(DbTeacher)).WithMany().HasForeignKey("teacher_id").HasPrincipalKey(nameof(DbTeacher.Id)).HasConstraintName("fk_lesson_batch_info_teacher_teacher"),
        //         l => l.HasOne(typeof(DbLessonBatchInfo)).WithMany().HasForeignKey("lesson_batch_info_id").HasPrincipalKey(nameof(DbLessonBatchInfo.Id)).HasConstraintName("fk_lesson_batch_info_teacher_lesson_batch_info"),
        //         j => j.HasKey("teacher_id", "lesson_batch_info_id"));
        //
        // builder.HasMany(x => x.Rooms)
        //     .WithMany()
        //     .UsingEntity(
        //         "lesson_batch_info_room",
        //         r => r.HasOne(typeof(DbRoom)).WithMany().HasForeignKey("room_id").HasPrincipalKey(nameof(DbRoom.Id)).HasConstraintName("fk_lesson_batch_info_room_room"),
        //         l => l.HasOne(typeof(DbLessonBatchInfo)).WithMany().HasForeignKey("lesson_batch_info_id").HasPrincipalKey(nameof(DbLessonBatchInfo.Id)).HasConstraintName("fk_lesson_batch_info_room_lesson_batch_info"),
        //         j => j.HasKey("room_id", "lesson_batch_info_id"));

        builder.Property(e => e.DayOfWeekTimeIntervals)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                v => JsonSerializer.Deserialize<DayOfWeekTimeInterval[]>(v, (JsonSerializerOptions)null!)!
            );

        builder.Property(x => x.RepeatType)
            .HasConversion(new EnumToStringConverter<DisciplineLessonRepeatType>());
    }

    private void LessonBatchInfoStudentGroupConfigure(EntityTypeBuilder<DbLessonBatchInfoStudentGroup> builder)
    {
        builder.HasKey(x => new { x.LessonBatchInfoId, x.StudentGroupId });

        builder.HasOne(lr => lr.LessonBatchInfo)
            .WithMany(l => l.StudentGroups)
            .HasForeignKey(lr => lr.LessonBatchInfoId);

        builder.HasOne(lr => lr.StudentGroup)
            .WithMany()
            .HasForeignKey(lr => lr.StudentGroupId);
    }

    private void LessonBatchInfoTeacherConfigure(EntityTypeBuilder<DbLessonBatchInfoTeacher> builder)
    {
        builder.HasKey(x => new { x.LessonBatchInfoId, x.TeacherId });

        builder.HasOne(lr => lr.LessonBatchInfo)
            .WithMany(l => l.Teachers)
            .HasForeignKey(lr => lr.LessonBatchInfoId);

        builder.HasOne(lr => lr.Teacher)
            .WithMany()
            .HasForeignKey(lr => lr.TeacherId);
    }

    private void LessonBatchInfoRoomConfigure(EntityTypeBuilder<DbLessonBatchInfoRoom> builder)
    {
        builder.HasKey(x => new { x.LessonBatchInfoId, x.RoomId });

        builder.HasOne(lr => lr.LessonBatchInfo)
            .WithMany(l => l.Rooms)
            .HasForeignKey(lr => lr.LessonBatchInfoId);

        builder.HasOne(lr => lr.Room)
            .WithMany()
            .HasForeignKey(lr => lr.RoomId);
    }

    private void LessonConfigure(EntityTypeBuilder<DbLesson> builder)
    {
        builder.HasOne(x => x.Schedule)
            .WithMany()
            .HasForeignKey(x => x.ScheduleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.AcademicDiscipline)
            .WithMany()
            .HasForeignKey(x => x.AcademicDisciplineId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.AcademicDisciplineType)
            .HasConversion(new EnumToStringConverter<AcademicDisciplineType>());

        // builder.HasMany(x => x.StudentGroups)
        //     .WithMany()
        //     .UsingEntity(
        //         "lesson_student_group",
        //         r => r.HasOne(typeof(DbStudentGroup)).WithMany().HasForeignKey("student_group_id").HasPrincipalKey(nameof(DbStudentGroup.Id)).HasConstraintName("fk_lesson_student_group_student_group"),
        //         l => l.HasOne(typeof(DbLesson)).WithMany().HasForeignKey("lesson_id").HasPrincipalKey(nameof(DbLesson.Id)).HasConstraintName("fk_lesson_student_group_lesson"),
        //         j => j.HasKey("student_group_id", "lesson_id"));
        //
        // builder.HasMany(x => x.Teachers)
        //     .WithMany()
        //     .UsingEntity(
        //         "lesson_teacher",
        //         r => r.HasOne(typeof(DbTeacher)).WithMany().HasForeignKey("teacher_id").HasPrincipalKey(nameof(DbTeacher.Id)).HasConstraintName("fk_lesson_teacher_teacher"),
        //         l => l.HasOne(typeof(DbLesson)).WithMany().HasForeignKey("lesson_id").HasPrincipalKey(nameof(DbLesson.Id)).HasConstraintName("fk_lesson_teacher_lesson"),
        //         j => j.HasKey("teacher_id", "lesson_id"));
        //
        // builder.HasMany(x => x.Rooms)
        //     .WithMany()
        //     .UsingEntity(
        //         "lesson_room",
        //         r => r.HasOne(typeof(DbRoom)).WithMany().HasForeignKey("room_id").HasPrincipalKey(nameof(DbRoom.Id)).HasConstraintName("fk_lesson_room_room"),
        //         l => l.HasOne(typeof(DbLesson)).WithMany().HasForeignKey("lesson_id").HasPrincipalKey(nameof(DbLesson.Id)).HasConstraintName("fk_lesson_room_lesson"),
        //         j => j.HasKey("room_id", "lesson_id"));

        builder.Property(x => x.FlexibilityType)
            .HasConversion(new EnumToStringConverter<LessonFlexibilityType>());

        builder.HasMany(x => x.ValidationMessages)
            .WithOne(x => x.Lesson)
            .HasForeignKey(x => x.LessonId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private void LessonStudentGroupConfigure(EntityTypeBuilder<DbLessonStudentGroup> builder)
    {
        builder.HasKey(x => new { x.LessonId, x.StudentGroupId });

        builder.HasOne(lr => lr.Lesson)
            .WithMany(l => l.StudentGroups)
            .HasForeignKey(lr => lr.LessonId);

        builder.HasOne(lr => lr.StudentGroup)
            .WithMany()
            .HasForeignKey(lr => lr.StudentGroupId);
    }

    private void LessonTeacherConfigure(EntityTypeBuilder<DbLessonTeacher> builder)
    {
        builder.HasKey(x => new { x.LessonId, x.TeacherId });

        builder.HasOne(lr => lr.Lesson)
            .WithMany(l => l.Teachers)
            .HasForeignKey(lr => lr.LessonId);

        builder.HasOne(lr => lr.Teacher)
            .WithMany()
            .HasForeignKey(lr => lr.TeacherId);
    }

    private void LessonRoomConfigure(EntityTypeBuilder<DbLessonRoom> builder)
    {
        builder.HasKey(x => new { x.LessonId, x.RoomId });

        builder.HasOne(lr => lr.Lesson)
            .WithMany(l => l.Rooms)
            .HasForeignKey(lr => lr.LessonId);

        builder.HasOne(lr => lr.Room)
            .WithMany()
            .HasForeignKey(lr => lr.RoomId);
    }

    private void LessonValidationMessageConfigure(EntityTypeBuilder<DbLessonValidationMessage> builder)
    {
        builder.Property(x => x.ErrorType)
            .HasConversion(new EnumToStringConverter<LessonValidationErrorType>());

        builder.Property(x => x.Code)
            .HasConversion(new EnumToStringConverter<LessonValidationCode>());

        builder.HasOne(x => x.AffectedByAcademicDiscipline)
            .WithMany()
            .HasForeignKey(x => x.AffectedByAcademicDisciplineId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.AffectedByAcademicDisciplineType)
            .HasConversion(new EnumToStringConverter<AcademicDisciplineType>());

        builder.HasOne(x => x.AffectedByLesson)
            .WithMany()
            .HasForeignKey(x => x.AffectedByLessonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AffectedByStudentGroup)
            .WithMany()
            .HasForeignKey(x => x.AffectedByStudentGroupId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AffectedByTeacher)
            .WithMany()
            .HasForeignKey(x => x.AffectedByTeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AffectedByTeacherPreference)
            .WithMany()
            .HasForeignKey(x => x.AffectedByTeacherPreferenceId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private void RoomConfigure(EntityTypeBuilder<DbRoom> builder)
    {
        builder.HasOne(x => x.Campus)
            .WithMany()
            .HasForeignKey(x => x.CampusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.RoomType)
            .HasConversion(new EnumToStringConverter<RoomType>());

        builder.Property(x => x.RoomBoardType)
            .HasConversion(new EnumToStringConverter<RoomBoardType>());
    }

    private void ScheduleConfigure(EntityTypeBuilder<DbSchedule> builder)
    {
    }

    private void StudentConfigure(EntityTypeBuilder<DbStudent> builder)
    {
        // builder.HasOne(x => x.User)
        //     .WithOne()
        //     .HasForeignKey<DbStudent>(x => x.UserId)
        //     .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.StudentGroup)
            .WithMany()
            .HasForeignKey(x => x.StudentGroupId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private void StudentGroupConfigure(EntityTypeBuilder<DbStudentGroup> builder)
    {
        builder.HasOne(x => x.Schedule)
            .WithMany()
            .HasForeignKey(x => x.ScheduleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.StudentGroupType)
            .HasConversion(new EnumToStringConverter<StudentGroupType>());

        // builder.HasMany(x => x.Parents)
        //     .WithMany()
        //     .UsingEntity(
        //         "student_group_hierarchy",
        //         r => r.HasOne(typeof(DbStudentGroup)).WithMany().HasForeignKey("parent_id").HasPrincipalKey(nameof(DbStudentGroup.Id)).HasConstraintName("fk_student_group_hierarchy_parent"),
        //         l => l.HasOne(typeof(DbStudentGroup)).WithMany().HasForeignKey("child_id").HasPrincipalKey(nameof(DbStudentGroup.Id)).HasConstraintName("fk_student_group_hierarchy_child"),
        //         j => j.HasKey("parent_id", "child_id"));
        //
        // builder.HasMany(x => x.Children)
        //     .WithMany()
        //     .UsingEntity(
        //         "student_group_hierarchy",
        //         r => r.HasOne(typeof(DbStudentGroup)).WithMany().HasForeignKey("child_id").HasPrincipalKey(nameof(DbStudentGroup.Id)).HasConstraintName("fk_student_group_hierarchy_child"),
        //         l => l.HasOne(typeof(DbStudentGroup)).WithMany().HasForeignKey("parent_id").HasPrincipalKey(nameof(DbStudentGroup.Id)).HasConstraintName("fk_student_group_hierarchy_parent"),
        //         j => j.HasKey("parent_id", "child_id"));
    }

    private void StudentGroupLinkConfigure(EntityTypeBuilder<DbStudentGroupLink> builder)
    {
        builder.HasKey(x => new { x.ChildStudentGroupId, x.ParentStudentGroupId });

        builder.HasOne(x => x.ParentStudentGroup)
            .WithMany(g => g.Children)
            .HasForeignKey(x => x.ParentStudentGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ChildStudentGroup)
            .WithMany(g => g.Parents)
            .HasForeignKey(x => x.ChildStudentGroupId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private void TeacherConfigure(EntityTypeBuilder<DbTeacher> builder)
    {
        // builder.HasOne(x => x.User)
        //     .WithOne()
        //     .HasForeignKey<DbTeacher>(x => x.UserId)
        //     .OnDelete(DeleteBehavior.Cascade);
    }

    private void TeacherPreferenceConfigure(EntityTypeBuilder<DbTeacherPreference> builder)
    {
        builder.HasOne(x => x.Schedule)
            .WithMany()
            .HasForeignKey(x => x.ScheduleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Teacher)
            .WithMany()
            .HasForeignKey(x => x.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Room)
            .WithMany()
            .HasForeignKey(x => x.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.DayOfWeek)
            .HasConversion(new EnumToStringConverter<DayOfWeek>());

        builder.Property(x => x.TeacherPreferenceType)
            .HasConversion(new EnumToStringConverter<TeacherPreferenceType>());
    }

    private void UserConfigure(EntityTypeBuilder<DbUser> builder)
    {
    }

    private void CampusConfigure(EntityTypeBuilder<DbCampus> builder)
    {
        builder.HasData(new List<DbCampus>
        {
            new()
            {
                Id = new Guid("f68b22ca-dc97-4aed-ab4f-db709e670d36"),
                Name = "Куйбышева",
            },
            new()
            {
                Id = new Guid("453addd1-7fc7-4028-9e1c-bf042c2164a3"),
                Name = "Тургенева",
            },
        });
    }
}