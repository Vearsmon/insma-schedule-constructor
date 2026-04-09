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
        builder.Entity<DbStudent>(StudentConfigure);
        builder.Entity<DbAcademicDiscipline>(AcademicDisciplineConfigure);
        builder.Entity<DbAcademicDisciplineLessonBatchInfo>(AcademicDisciplineLessonBatchInfoConfigure);
        builder.Entity<DbLesson>(LessonConfigure);
        builder.Entity<DbLessonValidationMessage>(LessonValidationMessageConfigure);

        base.OnModelCreating(builder);
    }

    private void AcademicDisciplineConfigure(EntityTypeBuilder<DbAcademicDiscipline> builder)
    {
        builder.HasOne(x => x.Schedule)
            .WithMany()
            .HasForeignKey(x => x.ScheduleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AcademicDisciplineLectureLessonBatchInfo)
            .WithOne()
            .HasForeignKey<DbAcademicDiscipline>(x => x.AcademicDisciplineLectureLessonBatchInfoId)
            .HasConstraintName("fk_academic_discipline_lecture_batch_info_id")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.AcademicDisciplinePracticeLessonBatchInfo)
            .WithOne()
            .HasForeignKey<DbAcademicDiscipline>(x => x.AcademicDisciplinePracticeLessonBatchInfoId)
            .HasConstraintName("fk_academic_discipline_practice_batch_info_id")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.AcademicDisciplineLabLessonBatchInfo)
            .WithOne()
            .HasForeignKey<DbAcademicDiscipline>(x => x.AcademicDisciplineLabLessonBatchInfoId)
            .HasConstraintName("fk_academic_discipline_lab_batch_info_id")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.AcademicDisciplineTargetType)
            .HasConversion(new EnumToStringConverter<AcademicDisciplineTargetType>());
    }

    private void AcademicDisciplineLessonBatchInfoConfigure(EntityTypeBuilder<DbAcademicDisciplineLessonBatchInfo> builder)
    {
        builder.HasMany(x => x.StudentGroups)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "lesson_batch_info_student_group",
                j => j
                    .HasOne<DbStudentGroup>()
                    .WithMany()
                    .HasForeignKey("student_groups_id")
                    .HasConstraintName("fk_lesson_batch_info_student_group_student_group"),
                j => j
                    .HasOne<DbAcademicDisciplineLessonBatchInfo>()
                    .WithMany()
                    .HasForeignKey("lesson_batch_info_id")
                    .HasConstraintName("fk_lesson_batch_info_student_group_lesson_batch_info")
            );

        builder.HasOne(x => x.Teacher)
            .WithMany()
            .HasForeignKey(x => x.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Room)
            .WithMany()
            .HasForeignKey(x => x.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(e => e.DayOfWeekTimeIntervals)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                v => JsonSerializer.Deserialize<DayOfWeekTimeInterval[]>(v, (JsonSerializerOptions)null!)!
            );

        builder.Property(x => x.RepeatType)
            .HasConversion(new EnumToStringConverter<DisciplineLessonRepeatType>());
    }

    private void CampusConfigure(EntityTypeBuilder<DbCampus> builder)
    {
    }

    private void LessonConfigure(EntityTypeBuilder<DbLesson> builder)
    {
        builder.HasOne(x => x.Schedule)
            .WithMany()
            .HasForeignKey(x => x.ScheduleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AcademicDiscipline)
            .WithMany()
            .HasForeignKey(x => x.AcademicDisciplineId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.AcademicDisciplineType)
            .HasConversion(new EnumToStringConverter<AcademicDisciplineType>());

        builder.HasMany(x => x.StudentGroups)
            .WithMany();

        builder.HasOne(x => x.Teacher)
            .WithMany()
            .HasForeignKey(x => x.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Room)
            .WithMany()
            .HasForeignKey(x => x.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.FlexibilityType)
            .HasConversion(new EnumToStringConverter<LessonFlexibilityType>());

        builder.HasMany(x => x.ValidationMessages)
            .WithOne(x => x.Lesson)
            .HasForeignKey(x => x.LessonId)
            .OnDelete(DeleteBehavior.Cascade);
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
        builder.HasOne(x => x.User)
            .WithOne()
            .HasForeignKey<DbStudent>(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

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

        builder.HasOne(x => x.Parent)
            .WithMany()
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Children)
            .WithOne(x => x.Parent)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private void TeacherConfigure(EntityTypeBuilder<DbTeacher> builder)
    {
        builder.HasOne(x => x.User)
            .WithOne()
            .HasForeignKey<DbTeacher>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private void TeacherPreferenceConfigure(EntityTypeBuilder<DbTeacherPreference> builder)
    {
        builder.HasOne(x => x.Schedule)
            .WithMany()
            .HasForeignKey(x => x.ScheduleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Teacher)
            .WithOne()
            .HasForeignKey<DbTeacherPreference>(x => x.TeacherId)
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
}