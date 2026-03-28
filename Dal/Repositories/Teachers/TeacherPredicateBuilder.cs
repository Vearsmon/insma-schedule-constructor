using System.Linq.Expressions;
using Dal.Entities;
using Dal.Helpers;
using Domain.Models.SearchModels;

namespace Dal.Repositories.Teachers;

public class TeacherPredicateBuilder : IPredicateBuilder<DbTeacher, TeacherSearchModel>
{
    public Expression<Func<DbTeacher, bool>> Predicate { get; } = PredicateBuilderExtensions.True<DbTeacher>();

    public Expression<Func<DbTeacher, bool>> Build(TeacherSearchModel searchModel)
    {
        return Predicate;
    }
}