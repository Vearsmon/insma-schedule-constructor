using Dal.Entities;
using Dal.RegistryRepositories;
using Dal.RegistryRepositories.AcademicDiscipline;
using Dal.RegistryRepositories.Campus;
using Dal.RegistryRepositories.Lesson;
using Dal.RegistryRepositories.Schedule;
using Dal.RegistryRepositories.StudentGroup;
using Dal.RegistryRepositories.Teacher;
using Dal.RegistryRepositories.TeacherPreference;
using Dal.Repositories;
using Dal.Repositories.AcademicDisciplines;
using Dal.Repositories.Campuses;
using Dal.Repositories.Lessons;
using Dal.Repositories.LessonValidationMessages;
using Dal.Repositories.Rooms;
using Dal.Repositories.Schedules;
using Dal.Repositories.ScheduleSettings;
using Dal.Repositories.StudentGroups;
using Dal.Repositories.Students;
using Dal.Repositories.TeacherPreferences;
using Dal.Repositories.Teachers;
using Dal.Repositories.Users;
using Dal.Transactions;
using Domain.Models;
using Domain.Models.RegistryItemModels;
using Domain.Models.RegistrySearchModels;
using Domain.Models.SearchModels;
using Microsoft.Extensions.DependencyInjection;

namespace Dal.Ioc;

public static class ServiceCollectionExtension
{
    public static IServiceCollection WithDal(this IServiceCollection services)
    {
        return services
            .AddTransient<InsmaScheduleContext>()
            .AddScoped<ITransactionalService, TransactionalService>()

            .AddScoped<IAcademicDisciplineRepository, AcademicDisciplineRepository>()
            .AddScoped<IRepositoryMapper<DbAcademicDiscipline, AcademicDiscipline>, AcademicDisciplineMapper>()
            .AddScoped<IPredicateBuilder<DbAcademicDiscipline, AcademicDisciplineSearchModel>, AcademicDisciplinePredicateBuilder>()
            .AddScoped<IAcademicDisciplineRegistryRepository, AcademicDisciplineRegistryRepository>()
            .AddScoped<IReadonlyRepositoryMapper<DbAcademicDiscipline, AcademicDisciplineRegistryItem>, AcademicDisciplineRegistryMapper>()
            .AddScoped<IRegistryRepositoryOrderer<DbAcademicDiscipline, AcademicDisciplineRegistryInternalSearchModel>, AcademicDisciplineRegistryOrderer>()
            .AddScoped<IPredicateBuilder<DbAcademicDiscipline, AcademicDisciplineRegistryInternalSearchModel>, AcademicDisciplineRegistryPredicateBuilder>()

            .AddScoped<ICampusRepository, CampusRepository>()
            .AddScoped<IRepositoryMapper<DbCampus, Campus>, CampusMapper>()
            .AddScoped<IPredicateBuilder<DbCampus, CampusSearchModel>, CampusPredicateBuilder>()
            .AddScoped<ICampusRegistryRepository, CampusRegistryRepository>()
            .AddScoped<IReadonlyRepositoryMapper<DbCampus, CampusRegistryItem>, CampusRegistryMapper>()
            .AddScoped<IRegistryRepositoryOrderer<DbCampus, CampusRegistryInternalSearchModel>, CampusRegistryOrderer>()
            .AddScoped<IPredicateBuilder<DbCampus, CampusRegistryInternalSearchModel>, CampusRegistryPredicateBuilder>()

            .AddScoped<ILessonRepository, LessonRepository>()
            .AddScoped<IRepositoryMapper<DbLesson, Lesson>, LessonMapper>()
            .AddScoped<IPredicateBuilder<DbLesson, LessonSearchModel>, LessonPredicateBuilder>()
            .AddScoped<ILessonRegistryRepository, LessonRegistryRepository>()
            .AddScoped<IReadonlyRepositoryMapper<DbLesson, LessonRegistryItem>, LessonRegistryMapper>()
            .AddScoped<IRegistryRepositoryOrderer<DbLesson, LessonRegistryInternalSearchModel>, LessonRegistryOrderer>()
            .AddScoped<IPredicateBuilder<DbLesson, LessonRegistryInternalSearchModel>, LessonRegistryPredicateBuilder>()

            .AddScoped<ILessonValidationMessageRepository, LessonValidationMessageRepository>()
            .AddScoped<IRepositoryMapper<DbLessonValidationMessage, LessonValidationMessage>, LessonValidationMessageMapper>()
            .AddScoped<IPredicateBuilder<DbLessonValidationMessage, LessonValidationMessageSearchModel>, LessonValidationMessagePredicateBuilder>()

            .AddScoped<IRoomRepository, RoomRepository>()
            .AddScoped<IRepositoryMapper<DbRoom, Room>, RoomMapper>()
            .AddScoped<IPredicateBuilder<DbRoom, RoomSearchModel>, RoomPredicateBuilder>()

            .AddScoped<IScheduleRepository, ScheduleRepository>()
            .AddScoped<IRepositoryMapper<DbSchedule, Schedule>, ScheduleMapper>()
            .AddScoped<IPredicateBuilder<DbSchedule, ScheduleSearchModel>, SchedulePredicateBuilder>()
            .AddScoped<IScheduleRegistryRepository, ScheduleRegistryRepository>()
            .AddScoped<IReadonlyRepositoryMapper<DbSchedule, ScheduleRegistryItem>, ScheduleRegistryMapper>()
            .AddScoped<IRegistryRepositoryOrderer<DbSchedule, ScheduleRegistryInternalSearchModel>, ScheduleRegistryOrderer>()
            .AddScoped<IPredicateBuilder<DbSchedule, ScheduleRegistryInternalSearchModel>, ScheduleRegistryPredicateBuilder>()

            .AddScoped<IScheduleSettingsRepository, ScheduleSettingsRepository>()
            .AddScoped<IRepositoryMapper<DbScheduleSettings, ScheduleSettings>, ScheduleSettingsMapper>()
            .AddScoped<IPredicateBuilder<DbScheduleSettings, ScheduleSettingsSearchModel>, ScheduleSettingsPredicateBuilder>()

            .AddScoped<IStudentGroupRepository, StudentGroupRepository>()
            .AddScoped<IRepositoryMapper<DbStudentGroup, StudentGroup>, StudentGroupMapper>()
            .AddScoped<IPredicateBuilder<DbStudentGroup, StudentGroupSearchModel>, StudentGroupPredicateBuilder>()
            .AddScoped<IStudentGroupRegistryRepository, StudentGroupRegistryRepository>()
            .AddScoped<IReadonlyRepositoryMapper<DbStudentGroup, StudentGroupRegistryItem>, StudentGroupRegistryMapper>()
            .AddScoped<IRegistryRepositoryOrderer<DbStudentGroup, StudentGroupRegistryInternalSearchModel>, StudentGroupRegistryOrderer>()
            .AddScoped<IPredicateBuilder<DbStudentGroup, StudentGroupRegistryInternalSearchModel>, StudentGroupRegistryPredicateBuilder>()

            .AddScoped<IStudentRepository, StudentRepository>()
            .AddScoped<IRepositoryMapper<DbStudent, Student>, StudentMapper>()
            .AddScoped<IPredicateBuilder<DbStudent, StudentSearchModel>, StudentPredicateBuilder>()

            .AddScoped<ITeacherPreferenceRepository, TeacherPreferenceRepository>()
            .AddScoped<IRepositoryMapper<DbTeacherPreference, TeacherPreference>, TeacherPreferenceMapper>()
            .AddScoped<IPredicateBuilder<DbTeacherPreference, TeacherPreferenceSearchModel>, TeacherPreferencePredicateBuilder>()
            .AddScoped<ITeacherPreferenceRegistryRepository, TeacherPreferenceRegistryRepository>()
            .AddScoped<IReadonlyRepositoryMapper<DbTeacherPreference, TeacherPreferenceRegistryItem>, TeacherPreferenceRegistryMapper>()
            .AddScoped<IRegistryRepositoryOrderer<DbTeacherPreference, TeacherPreferenceRegistryInternalSearchModel>, TeacherPreferenceRegistryOrderer>()
            .AddScoped<IPredicateBuilder<DbTeacherPreference, TeacherPreferenceRegistryInternalSearchModel>, TeacherPreferenceRegistryPredicateBuilder>()

            .AddScoped<ITeacherRepository, TeacherRepository>()
            .AddScoped<IRepositoryMapper<DbTeacher, Teacher>, TeacherMapper>()
            .AddScoped<IPredicateBuilder<DbTeacher, TeacherSearchModel>, TeacherPredicateBuilder>()
            .AddScoped<ITeacherRegistryRepository, TeacherRegistryRepository>()
            .AddScoped<IReadonlyRepositoryMapper<DbTeacher, TeacherRegistryItem>, TeacherRegistryMapper>()
            .AddScoped<IRegistryRepositoryOrderer<DbTeacher, TeacherRegistryInternalSearchModel>, TeacherRegistryOrderer>()
            .AddScoped<IPredicateBuilder<DbTeacher, TeacherRegistryInternalSearchModel>, TeacherRegistryPredicateBuilder>()

            .AddScoped<IUserRepository, UserRepository>()
            .AddScoped<IRepositoryMapper<DbUser, User>, UserMapper>()
            .AddScoped<IPredicateBuilder<DbUser, UserSearchModel>, UserPredicateBuilder>()
            ;
    }
}