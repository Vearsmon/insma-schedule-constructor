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
        builder.Entity<DbDayOfWeekTimeIntervalAssignment>(DayOfWeekTimeIntervalAssignmentConfigure);
        builder.Entity<DbLessonBatchInfo>(LessonBatchInfoConfigure);
        builder.Entity<DbLesson>(LessonConfigure);
        builder.Entity<DbPolicyViolation>(LessonPolicyViolationConfigure);
        builder.Entity<DbPolicyViolationTarget>(LessonPolicyViolationTargetConfigure);

        base.OnModelCreating(builder);
    }

    private void AcademicDisciplineConfigure(EntityTypeBuilder<DbAcademicDiscipline> builder)
    {
        builder.HasOne(x => x.Schedule)
            .WithMany()
            .HasForeignKey(x => x.ScheduleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(e => e.AllowedLessonTypes)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                v => JsonSerializer.Deserialize<AcademicDisciplineType[]>(v, (JsonSerializerOptions)null!)!
            );

        builder.Property(x => x.AcademicDisciplineTargetType)
            .HasConversion(new EnumToStringConverter<AcademicDisciplineTargetType>());
    }

    private void DayOfWeekTimeIntervalAssignmentConfigure(EntityTypeBuilder<DbDayOfWeekTimeIntervalAssignment> builder)
    {
    }

    private void LessonBatchInfoConfigure(EntityTypeBuilder<DbLessonBatchInfo> builder)
    {
        builder.HasMany(x => x.StudentGroups)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>("lesson_batch_info_student_group",
                l => l.HasOne<DbStudentGroup>()
                    .WithMany()
                    .HasForeignKey("student_group_id")
                    .HasConstraintName("fk_lesson_batch_info_student_group_student_group")
                    .OnDelete(DeleteBehavior.Cascade),
                r => r.HasOne<DbLessonBatchInfo>()
                    .WithMany()
                    .HasForeignKey("lesson_batch_info_id")
                    .HasConstraintName("fk_lesson_batch_info_student_group_batch")
                    .OnDelete(DeleteBehavior.Cascade),
                j =>
                {
                    j.HasKey("lesson_batch_info_id", "student_group_id").HasName("pk_lesson_batch_info_student_group");
                    j.HasIndex("lesson_batch_info_id").HasDatabaseName("ix_lesson_batch_info_student_group_batch_id");
                    j.ToTable("lesson_batch_info_student_group");
                });

        builder.HasMany(x => x.Teachers)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>("lesson_batch_info_teacher",
                l => l.HasOne<DbTeacher>()
                    .WithMany()
                    .HasForeignKey("teacher_id")
                    .HasConstraintName("fk_lesson_batch_info_teacher_teacher")
                    .OnDelete(DeleteBehavior.Cascade),
                r => r.HasOne<DbLessonBatchInfo>()
                    .WithMany()
                    .HasForeignKey("lesson_batch_info_id")
                    .HasConstraintName("fk_lesson_batch_info_teacher_batch")
                    .OnDelete(DeleteBehavior.Cascade),
                j =>
                {
                    j.HasKey("lesson_batch_info_id", "teacher_id").HasName("pk_lesson_batch_info_teacher");
                    j.HasIndex("lesson_batch_info_id").HasDatabaseName("ix_lesson_batch_info_teacher_batch_id");
                    j.ToTable("lesson_batch_info_teacher");
                });

        builder.HasMany(x => x.Rooms)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>("lesson_batch_info_room",
                l => l.HasOne<DbRoom>()
                    .WithMany()
                    .HasForeignKey("room_id")
                    .HasConstraintName("fk_lesson_batch_info_room_room")
                    .OnDelete(DeleteBehavior.Cascade),
                r => r.HasOne<DbLessonBatchInfo>()
                    .WithMany()
                    .HasForeignKey("lesson_batch_info_id")
                    .HasConstraintName("fk_lesson_batch_info_room_batch")
                    .OnDelete(DeleteBehavior.Cascade),
                j =>
                {
                    j.HasKey("lesson_batch_info_id", "room_id").HasName("pk_lesson_batch_info_room");
                    j.HasIndex("lesson_batch_info_id").HasDatabaseName("ix_lesson_batch_info_room_batch_id");
                    j.ToTable("lesson_batch_info_room");
                });

        builder.HasMany(x => x.DayOfWeekTimeIntervals)
            .WithOne()
            .HasForeignKey(x => x.LessonBatchInfoId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.RepeatType)
            .HasConversion(new EnumToStringConverter<DisciplineLessonRepeatType>());

        builder.Property(x => x.FlexibilityType)
            .HasConversion(new EnumToStringConverter<LessonFlexibilityType>());

        builder.HasMany(x => x.Violations)
            .WithOne(x => x.LessonBatchInfo)
            .HasForeignKey(x => x.LessonBatchInfoId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private void LessonConfigure(EntityTypeBuilder<DbLesson> builder)
    {
        builder.HasMany(x => x.StudentGroups)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>("lesson_student_group",
                l => l.HasOne<DbStudentGroup>()
                    .WithMany()
                    .HasForeignKey("student_group_id")
                    .HasConstraintName("fk_lesson_student_group_student_group")
                    .OnDelete(DeleteBehavior.Cascade),
                r => r.HasOne<DbLesson>()
                    .WithMany()
                    .HasForeignKey("lesson_id")
                    .HasConstraintName("fk_lesson_student_group_lesson")
                    .OnDelete(DeleteBehavior.Cascade),
                j =>
                {
                    j.HasKey("lesson_id", "student_group_id").HasName("pk_lesson_student_group");
                    j.HasIndex("lesson_id").HasDatabaseName("ix_lesson_student_group_lesson_id");
                    j.ToTable("lesson_student_group");
                });

        builder.HasMany(x => x.Teachers)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>("lesson_teacher",
                l => l.HasOne<DbTeacher>()
                    .WithMany()
                    .HasForeignKey("teacher_id")
                    .HasConstraintName("fk_lesson_teacher_teacher")
                    .OnDelete(DeleteBehavior.Cascade),
                r => r.HasOne<DbLesson>()
                    .WithMany()
                    .HasForeignKey("lesson_id")
                    .HasConstraintName("fk_lesson_teacher_lesson")
                    .OnDelete(DeleteBehavior.Cascade),
                j =>
                {
                    j.HasKey("lesson_id", "teacher_id").HasName("pk_lesson_teacher");
                    j.HasIndex("lesson_id").HasDatabaseName("ix_lesson_teacher_lesson_id");
                    j.ToTable("lesson_teacher");
                });

        builder.HasMany(x => x.Rooms)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>("lesson_room",
                l => l.HasOne<DbRoom>()
                    .WithMany()
                    .HasForeignKey("room_id")
                    .HasConstraintName("fk_lesson_room_room")
                    .OnDelete(DeleteBehavior.Cascade),
                r => r.HasOne<DbLesson>()
                    .WithMany()
                    .HasForeignKey("lesson_id")
                    .HasConstraintName("fk_lesson_room_lesson")
                    .OnDelete(DeleteBehavior.Cascade),
                j =>
                {
                    j.HasKey("lesson_id", "room_id").HasName("pk_lesson_room");
                    j.HasIndex("lesson_id").HasDatabaseName("ix_lesson_room_lesson_id");
                    j.ToTable("lesson_room");
                });

        builder.HasOne(x => x.DayOfWeekTimeIntervalAssignment)
            .WithMany()
            .HasForeignKey(x => x.DayOfWeekTimeIntervalAssignmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.FlexibilityType)
            .HasConversion(new EnumToStringConverter<LessonFlexibilityType>());

        builder.HasOne(x => x.LessonBatchInfo)
            .WithMany()
            .HasForeignKey(x => x.LessonBatchInfoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Violations)
            .WithOne(x => x.Lesson)
            .HasForeignKey(x => x.LessonId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private void LessonPolicyViolationConfigure(EntityTypeBuilder<DbPolicyViolation> builder)
    {
        builder.Property(x => x.ErrorType)
            .HasConversion(new EnumToStringConverter<LessonValidationErrorType>());

        builder.Property(x => x.Code)
            .HasConversion(new EnumToStringConverter<LessonPolicyViolationCode>());

        builder.Property(e => e.DayOfWeekTimeInterval)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                v => JsonSerializer.Deserialize<DayOfWeekTimeInterval?>(v, (JsonSerializerOptions)null!)!
            );

        builder.Property(e => e.Timestamp)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                v => JsonSerializer.Deserialize<DateWithTimeInterval?>(v, (JsonSerializerOptions)null!)!
            );
    }

    private void LessonPolicyViolationTargetConfigure(EntityTypeBuilder<DbPolicyViolationTarget> builder)
    {
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

        builder.HasMany(x => x.Children)
            .WithMany(x => x.Parents)
            .UsingEntity<Dictionary<string, object>>("student_group_link",
                l => l.HasOne<DbStudentGroup>()
                    .WithMany()
                    .HasForeignKey("child_id")
                    .HasConstraintName("fk_student_group_link_child")
                    .OnDelete(DeleteBehavior.Cascade),
                r => r.HasOne<DbStudentGroup>()
                    .WithMany()
                    .HasForeignKey("parent_id")
                    .HasConstraintName("fk_student_group_link_parent")
                    .OnDelete(DeleteBehavior.Cascade),
                j =>
                {
                    j.HasKey("parent_id", "child_id").HasName("pk_student_group_link");
                    j.HasIndex("child_id").HasDatabaseName("ix_student_group_link_child_id");
                    j.ToTable("student_group_link");
                });

        builder.Property(x => x.StudentGroupType)
            .HasConversion(new EnumToStringConverter<StudentGroupType>());
    }

    private void TeacherConfigure(EntityTypeBuilder<DbTeacher> builder)
    {
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