using Domain.Models.Common;
using Domain.Models.Enums;

namespace Domain.Dto.SaveDto;

public class TeacherTimePreferenceSaveDto
{
    public TeacherPreferenceType TeacherPreferenceType { get; set; }
    public DayOfWeekTimeInterval DayOfWeekTimeInterval { get; set; } = null!;
}