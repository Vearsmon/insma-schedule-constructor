using System.Data.Common;

namespace Domain.Exceptions;

/// <summary>
/// Исключение, которое генерируется контекстом данных при нарушении уникальности поля в таблице, т.е. при нарушени условия уникальности в индексе СУБД.
/// </summary>
public class UniqueFieldDbException : DbException
{
    public UniqueFieldDbException(string message) : base(message)
    {
    }

    public UniqueFieldDbException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
