using System.Linq.Expressions;
using Dal.Entities;
using Dal.Helpers;
using Domain.Models.SearchModels;

namespace Dal.Repositories.StudentGroups;

public class StudentGroupPredicateBuilder : IPredicateBuilder<DbStudentGroup, StudentGroupSearchModel>
{
    public Expression<Func<DbStudentGroup, bool>> Predicate { get; } = PredicateBuilderExtensions.True<DbStudentGroup>();

    public Expression<Func<DbStudentGroup, bool>> Build(StudentGroupSearchModel searchModel)
    {
        return Predicate;
    }
}