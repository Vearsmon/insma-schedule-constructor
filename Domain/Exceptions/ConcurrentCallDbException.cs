using System.Data.Common;

namespace Domain.Exceptions;

/// <summary>
/// An exception that is thrown when a concurrency violation is encountered while saving to the database.A concurrency violation
/// occurs when an unexpected number of rows are affected during save.This is usually because the data in the database has
/// been modified since it was loaded into memory.
/// </summary>
public class ConcurrentCallDbException : DbException
{
    public ConcurrentCallDbException(string message) : base(message)
    {
    }

    public ConcurrentCallDbException(Exception innerException) : base("Concurrent call", innerException)
    {
    }
}
