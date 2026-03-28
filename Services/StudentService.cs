using Dal.Repositories.Students;
using Domain.Services;

namespace Services;

public class StudentService(IStudentRepository studentRepository) : IStudentService
{
}