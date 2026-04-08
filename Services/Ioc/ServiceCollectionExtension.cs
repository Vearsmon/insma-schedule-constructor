using Dal.Ioc;
using Domain.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Services.Ioc;

public static class ServiceCollectionExtension
{
    public static IServiceCollection WithServices(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        return serviceCollection
            .WithDal()
            .AddScoped<ICampusService, CampusService>()
            .AddScoped<ILessonService, LessonService>()
            .AddScoped<ILessonValidationService, LessonValidationService>()
            .AddScoped<IRoomService, RoomService>()
            .AddScoped<IScheduleService, ScheduleService>()
            .AddScoped<IStudentGroupService, StudentGroupService>()
            .AddScoped<IStudentService, StudentService>()
            .AddScoped<IAcademicDisciplineService, AcademicDisciplineService>()
            .AddScoped<ITeacherPreferenceService, TeacherPreferenceService>()
            .AddScoped<ITeacherService, TeacherService>()
            .AddScoped<IUserService, UserService>()
            ;
    }
}