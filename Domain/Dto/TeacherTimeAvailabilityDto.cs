using Domain.Models.Common;
using Domain.Models.Enums;

namespace Domain.Dto;

public class TeacherTimeAvailabilityDto
{
    public TeacherPreferenceType TeacherPreferenceType { get; set; }

    public DayOfWeekTimeInterval DayOfWeekTimeInterval { get; set; } = null!;
}