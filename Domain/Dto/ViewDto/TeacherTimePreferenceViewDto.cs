using Domain.Models.Common;
using Domain.Models.Enums;

namespace Domain.Dto.ViewDto;

public class TeacherTimePreferenceViewDto
{
    public TeacherPreferenceType TeacherPreferenceType { get; set; }
    public DayOfWeekTimeInterval DayOfWeekTimeInterval { get; set; } = null!;
}