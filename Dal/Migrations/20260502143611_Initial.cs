using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Dal.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "campus",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_campus", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "schedule",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    date_from = table.Column<DateOnly>(type: "Date", nullable: false),
                    date_to = table.Column<DateOnly>(type: "Date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_schedule", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "teacher",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    fullname = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    contacts = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_teacher", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    username = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    role = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "TimestampTz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "room",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    campus_id = table.Column<Guid>(type: "uuid", nullable: false),
                    room_type = table.Column<string>(type: "text", nullable: true),
                    capacity = table.Column<int>(type: "integer", nullable: true),
                    room_board_type = table.Column<string>(type: "text", nullable: true),
                    has_projector = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_room", x => x.id);
                    table.ForeignKey(
                        name: "fk_room_campus_campus_id",
                        column: x => x.campus_id,
                        principalTable: "campus",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "academic_discipline",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    schedule_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    associated_names = table.Column<string[]>(type: "text[]", nullable: false),
                    semester_number = table.Column<int>(type: "integer", nullable: true),
                    academic_discipline_target_type = table.Column<string>(type: "text", nullable: false),
                    is_lecture_lessons_allowed = table.Column<bool>(type: "boolean", nullable: false),
                    lecture_total_hours_count = table.Column<int>(type: "integer", nullable: true),
                    is_practice_lessons_allowed = table.Column<bool>(type: "boolean", nullable: false),
                    practice_total_hours_count = table.Column<int>(type: "integer", nullable: true),
                    is_lab_lessons_allowed = table.Column<bool>(type: "boolean", nullable: false),
                    lab_total_hours_count = table.Column<int>(type: "integer", nullable: true),
                    is_exam_lessons_allowed = table.Column<bool>(type: "boolean", nullable: false),
                    is_test_lessons_allowed = table.Column<bool>(type: "boolean", nullable: false),
                    comment = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_academic_discipline", x => x.id);
                    table.ForeignKey(
                        name: "fk_academic_discipline_db_schedule_schedule_id",
                        column: x => x.schedule_id,
                        principalTable: "schedule",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "student_group",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    schedule_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    semester_number = table.Column<int>(type: "integer", nullable: true),
                    students_count = table.Column<int>(type: "integer", nullable: true),
                    student_group_type = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_student_group", x => x.id);
                    table.ForeignKey(
                        name: "fk_student_group_schedule_schedule_id",
                        column: x => x.schedule_id,
                        principalTable: "schedule",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "teacher_preference",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    schedule_id = table.Column<Guid>(type: "uuid", nullable: false),
                    teacher_id = table.Column<Guid>(type: "uuid", nullable: false),
                    room_id = table.Column<Guid>(type: "uuid", nullable: true),
                    day_of_week = table.Column<string>(type: "text", nullable: true),
                    time_from = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    time_to = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    teacher_preference_type = table.Column<string>(type: "text", nullable: true),
                    comment = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_teacher_preference", x => x.id);
                    table.ForeignKey(
                        name: "fk_teacher_preference_room_room_id",
                        column: x => x.room_id,
                        principalTable: "room",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_teacher_preference_schedule_schedule_id",
                        column: x => x.schedule_id,
                        principalTable: "schedule",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_teacher_preference_teacher_teacher_id",
                        column: x => x.teacher_id,
                        principalTable: "teacher",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "lesson",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    schedule_id = table.Column<Guid>(type: "uuid", nullable: false),
                    academic_discipline_id = table.Column<Guid>(type: "uuid", nullable: true),
                    academic_discipline_type = table.Column<string>(type: "text", nullable: true),
                    date = table.Column<DateOnly>(type: "Date", nullable: true),
                    time_from = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    time_to = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    flexibility_type = table.Column<string>(type: "text", nullable: false),
                    hours_cost = table.Column<int>(type: "integer", nullable: true),
                    allow_combining = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lesson", x => x.id);
                    table.ForeignKey(
                        name: "fk_lesson_academic_discipline_academic_discipline_id",
                        column: x => x.academic_discipline_id,
                        principalTable: "academic_discipline",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_lesson_db_schedule_schedule_id",
                        column: x => x.schedule_id,
                        principalTable: "schedule",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "lesson_batch_info",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    day_of_week_time_intervals = table.Column<string>(type: "text", nullable: false),
                    repeat_type = table.Column<string>(type: "text", nullable: false),
                    date_from = table.Column<DateOnly>(type: "Date", nullable: false),
                    date_to = table.Column<DateOnly>(type: "Date", nullable: false),
                    allow_combining = table.Column<bool>(type: "boolean", nullable: false),
                    hours_cost = table.Column<int>(type: "integer", nullable: true),
                    academic_discipline_id = table.Column<Guid>(type: "uuid", nullable: true),
                    academic_discipline_id1 = table.Column<Guid>(type: "uuid", nullable: true),
                    academic_discipline_id2 = table.Column<Guid>(type: "uuid", nullable: true),
                    academic_discipline_id3 = table.Column<Guid>(type: "uuid", nullable: true),
                    academic_discipline_id4 = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lesson_batch_info", x => x.id);
                    table.ForeignKey(
                        name: "fk_lesson_batch_info_exam",
                        column: x => x.academic_discipline_id,
                        principalTable: "academic_discipline",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_lesson_batch_info_lab",
                        column: x => x.academic_discipline_id1,
                        principalTable: "academic_discipline",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_lesson_batch_info_lecture",
                        column: x => x.academic_discipline_id2,
                        principalTable: "academic_discipline",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_lesson_batch_info_practice",
                        column: x => x.academic_discipline_id3,
                        principalTable: "academic_discipline",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_lesson_batch_info_test",
                        column: x => x.academic_discipline_id4,
                        principalTable: "academic_discipline",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "student",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fullname = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    student_group_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_student", x => x.id);
                    table.ForeignKey(
                        name: "fk_student_db_student_group_student_group_id",
                        column: x => x.student_group_id,
                        principalTable: "student_group",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_student_db_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "student_group_hierarchy (_dictionary<string, object>)",
                columns: table => new
                {
                    parent_id = table.Column<Guid>(type: "uuid", nullable: false),
                    child_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_student_group_hierarchy_dictionary_string_object", x => new { x.parent_id, x.child_id });
                    table.ForeignKey(
                        name: "fk_student_group_hierarchy_child",
                        column: x => x.child_id,
                        principalTable: "student_group",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_student_group_hierarchy_parent",
                        column: x => x.parent_id,
                        principalTable: "student_group",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lesson_room (_dictionary<string, object>)",
                columns: table => new
                {
                    room_id = table.Column<Guid>(type: "uuid", nullable: false),
                    lesson_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lesson_room_dictionary_string_object", x => new { x.room_id, x.lesson_id });
                    table.ForeignKey(
                        name: "fk_lesson_room_lesson",
                        column: x => x.lesson_id,
                        principalTable: "lesson",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_lesson_room_room",
                        column: x => x.room_id,
                        principalTable: "room",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lesson_student_group (_dictionary<string, object>)",
                columns: table => new
                {
                    student_group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    lesson_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lesson_student_group_dictionary_string_object", x => new { x.student_group_id, x.lesson_id });
                    table.ForeignKey(
                        name: "fk_lesson_student_group_lesson",
                        column: x => x.lesson_id,
                        principalTable: "lesson",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_lesson_student_group_student_group",
                        column: x => x.student_group_id,
                        principalTable: "student_group",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lesson_teacher (_dictionary<string, object>)",
                columns: table => new
                {
                    teacher_id = table.Column<Guid>(type: "uuid", nullable: false),
                    lesson_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lesson_teacher_dictionary_string_object", x => new { x.teacher_id, x.lesson_id });
                    table.ForeignKey(
                        name: "fk_lesson_teacher_lesson",
                        column: x => x.lesson_id,
                        principalTable: "lesson",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_lesson_teacher_teacher",
                        column: x => x.teacher_id,
                        principalTable: "teacher",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lesson_validation_message",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    lesson_id = table.Column<Guid>(type: "uuid", nullable: false),
                    error_type = table.Column<string>(type: "text", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    affected_by_academic_discipline_id = table.Column<Guid>(type: "uuid", nullable: true),
                    affected_by_academic_discipline_type = table.Column<string>(type: "text", nullable: true),
                    affected_by_lesson_id = table.Column<Guid>(type: "uuid", nullable: true),
                    affected_by_student_group_id = table.Column<Guid>(type: "uuid", nullable: true),
                    affected_by_teacher_id = table.Column<Guid>(type: "uuid", nullable: true),
                    affected_by_teacher_preference_id = table.Column<Guid>(type: "uuid", nullable: true),
                    message = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lesson_validation_message", x => x.id);
                    table.ForeignKey(
                        name: "fk_lesson_validation_message_academic_discipline_affected_by_a",
                        column: x => x.affected_by_academic_discipline_id,
                        principalTable: "academic_discipline",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_lesson_validation_message_db_student_group_affected_by_stud",
                        column: x => x.affected_by_student_group_id,
                        principalTable: "student_group",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_lesson_validation_message_db_teacher_affected_by_teacher_id",
                        column: x => x.affected_by_teacher_id,
                        principalTable: "teacher",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_lesson_validation_message_db_teacher_preference_affected_by",
                        column: x => x.affected_by_teacher_preference_id,
                        principalTable: "teacher_preference",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_lesson_validation_message_lesson_affected_by_lesson_id",
                        column: x => x.affected_by_lesson_id,
                        principalTable: "lesson",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_lesson_validation_message_lesson_lesson_id",
                        column: x => x.lesson_id,
                        principalTable: "lesson",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lesson_batch_info_room (_dictionary<string, object>)",
                columns: table => new
                {
                    room_id = table.Column<Guid>(type: "uuid", nullable: false),
                    lesson_batch_info_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lesson_batch_info_room_dictionary_string_object", x => new { x.room_id, x.lesson_batch_info_id });
                    table.ForeignKey(
                        name: "fk_lesson_batch_info_room_lesson_batch_info",
                        column: x => x.lesson_batch_info_id,
                        principalTable: "lesson_batch_info",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_lesson_batch_info_room_room",
                        column: x => x.room_id,
                        principalTable: "room",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lesson_batch_info_student_group (_dictionary<string, object>)",
                columns: table => new
                {
                    student_group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    lesson_batch_info_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lesson_batch_info_student_group_dictionary_string_object", x => new { x.student_group_id, x.lesson_batch_info_id });
                    table.ForeignKey(
                        name: "fk_lesson_batch_info_student_group_lesson_batch_info",
                        column: x => x.lesson_batch_info_id,
                        principalTable: "lesson_batch_info",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_lesson_batch_info_student_group_student_group",
                        column: x => x.student_group_id,
                        principalTable: "student_group",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lesson_batch_info_teacher (_dictionary<string, object>)",
                columns: table => new
                {
                    teacher_id = table.Column<Guid>(type: "uuid", nullable: false),
                    lesson_batch_info_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lesson_batch_info_teacher_dictionary_string_object", x => new { x.teacher_id, x.lesson_batch_info_id });
                    table.ForeignKey(
                        name: "fk_lesson_batch_info_teacher_lesson_batch_info",
                        column: x => x.lesson_batch_info_id,
                        principalTable: "lesson_batch_info",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_lesson_batch_info_teacher_teacher",
                        column: x => x.teacher_id,
                        principalTable: "teacher",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "campus",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { new Guid("453addd1-7fc7-4028-9e1c-bf042c2164a3"), "Тургенева" },
                    { new Guid("f68b22ca-dc97-4aed-ab4f-db709e670d36"), "Куйбышева" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_academic_discipline_schedule_id",
                table: "academic_discipline",
                column: "schedule_id");

            migrationBuilder.CreateIndex(
                name: "ix_lesson_academic_discipline_id",
                table: "lesson",
                column: "academic_discipline_id");

            migrationBuilder.CreateIndex(
                name: "ix_lesson_schedule_id",
                table: "lesson",
                column: "schedule_id");

            migrationBuilder.CreateIndex(
                name: "ix_lesson_batch_info_db_academic_discipline_id",
                table: "lesson_batch_info",
                column: "academic_discipline_id");

            migrationBuilder.CreateIndex(
                name: "ix_lesson_batch_info_db_academic_discipline_id1",
                table: "lesson_batch_info",
                column: "academic_discipline_id1");

            migrationBuilder.CreateIndex(
                name: "ix_lesson_batch_info_db_academic_discipline_id2",
                table: "lesson_batch_info",
                column: "academic_discipline_id2");

            migrationBuilder.CreateIndex(
                name: "ix_lesson_batch_info_db_academic_discipline_id3",
                table: "lesson_batch_info",
                column: "academic_discipline_id3");

            migrationBuilder.CreateIndex(
                name: "ix_lesson_batch_info_db_academic_discipline_id4",
                table: "lesson_batch_info",
                column: "academic_discipline_id4");

            migrationBuilder.CreateIndex(
                name: "ix_lesson_batch_info_room_dictionary_string_object_lesson",
                table: "lesson_batch_info_room (_dictionary<string, object>)",
                column: "lesson_batch_info_id");

            migrationBuilder.CreateIndex(
                name: "ix_lesson_batch_info_student_group_dictionary_string_object",
                table: "lesson_batch_info_student_group (_dictionary<string, object>)",
                column: "lesson_batch_info_id");

            migrationBuilder.CreateIndex(
                name: "ix_lesson_batch_info_teacher_dictionary_string_object_les",
                table: "lesson_batch_info_teacher (_dictionary<string, object>)",
                column: "lesson_batch_info_id");

            migrationBuilder.CreateIndex(
                name: "ix_lesson_room_dictionary_string_object_lesson_id",
                table: "lesson_room (_dictionary<string, object>)",
                column: "lesson_id");

            migrationBuilder.CreateIndex(
                name: "ix_lesson_student_group_dictionary_string_object_lesson_id",
                table: "lesson_student_group (_dictionary<string, object>)",
                column: "lesson_id");

            migrationBuilder.CreateIndex(
                name: "ix_lesson_teacher_dictionary_string_object_lesson_id",
                table: "lesson_teacher (_dictionary<string, object>)",
                column: "lesson_id");

            migrationBuilder.CreateIndex(
                name: "ix_lesson_validation_message_affected_by_academic_discipline_id",
                table: "lesson_validation_message",
                column: "affected_by_academic_discipline_id");

            migrationBuilder.CreateIndex(
                name: "ix_lesson_validation_message_affected_by_lesson_id",
                table: "lesson_validation_message",
                column: "affected_by_lesson_id");

            migrationBuilder.CreateIndex(
                name: "ix_lesson_validation_message_affected_by_student_group_id",
                table: "lesson_validation_message",
                column: "affected_by_student_group_id");

            migrationBuilder.CreateIndex(
                name: "ix_lesson_validation_message_affected_by_teacher_id",
                table: "lesson_validation_message",
                column: "affected_by_teacher_id");

            migrationBuilder.CreateIndex(
                name: "ix_lesson_validation_message_affected_by_teacher_preference_id",
                table: "lesson_validation_message",
                column: "affected_by_teacher_preference_id");

            migrationBuilder.CreateIndex(
                name: "ix_lesson_validation_message_lesson_id",
                table: "lesson_validation_message",
                column: "lesson_id");

            migrationBuilder.CreateIndex(
                name: "ix_room_campus_id",
                table: "room",
                column: "campus_id");

            migrationBuilder.CreateIndex(
                name: "ix_student_student_group_id",
                table: "student",
                column: "student_group_id");

            migrationBuilder.CreateIndex(
                name: "ix_student_user_id",
                table: "student",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_student_group_schedule_id",
                table: "student_group",
                column: "schedule_id");

            migrationBuilder.CreateIndex(
                name: "ix_student_group_hierarchy_dictionary_string_object_child",
                table: "student_group_hierarchy (_dictionary<string, object>)",
                column: "child_id");

            migrationBuilder.CreateIndex(
                name: "ix_teacher_preference_room_id",
                table: "teacher_preference",
                column: "room_id");

            migrationBuilder.CreateIndex(
                name: "ix_teacher_preference_schedule_id",
                table: "teacher_preference",
                column: "schedule_id");

            migrationBuilder.CreateIndex(
                name: "ix_teacher_preference_teacher_id",
                table: "teacher_preference",
                column: "teacher_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "lesson_batch_info_room (_dictionary<string, object>)");

            migrationBuilder.DropTable(
                name: "lesson_batch_info_student_group (_dictionary<string, object>)");

            migrationBuilder.DropTable(
                name: "lesson_batch_info_teacher (_dictionary<string, object>)");

            migrationBuilder.DropTable(
                name: "lesson_room (_dictionary<string, object>)");

            migrationBuilder.DropTable(
                name: "lesson_student_group (_dictionary<string, object>)");

            migrationBuilder.DropTable(
                name: "lesson_teacher (_dictionary<string, object>)");

            migrationBuilder.DropTable(
                name: "lesson_validation_message");

            migrationBuilder.DropTable(
                name: "student");

            migrationBuilder.DropTable(
                name: "student_group_hierarchy (_dictionary<string, object>)");

            migrationBuilder.DropTable(
                name: "lesson_batch_info");

            migrationBuilder.DropTable(
                name: "teacher_preference");

            migrationBuilder.DropTable(
                name: "lesson");

            migrationBuilder.DropTable(
                name: "user");

            migrationBuilder.DropTable(
                name: "student_group");

            migrationBuilder.DropTable(
                name: "room");

            migrationBuilder.DropTable(
                name: "teacher");

            migrationBuilder.DropTable(
                name: "academic_discipline");

            migrationBuilder.DropTable(
                name: "campus");

            migrationBuilder.DropTable(
                name: "schedule");
        }
    }
}
