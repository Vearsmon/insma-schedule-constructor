using System.Linq.Expressions;
using Dal.Entities;
using Dal.Helpers;
using Domain.Models.SearchModels;

namespace Dal.Repositories.Students;

public class StudentPredicateBuilder : IPredicateBuilder<DbStudent, StudentSearchModel>
{
    public Expression<Func<DbStudent, bool>> Predicate { get; } = PredicateBuilderExtensions.True<DbStudent>();

    public Expression<Func<DbStudent, bool>> Build(StudentSearchModel searchModel)
    {
        return Predicate;
    }
}