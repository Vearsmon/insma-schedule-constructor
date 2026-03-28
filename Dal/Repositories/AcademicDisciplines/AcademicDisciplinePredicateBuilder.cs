using System.Linq.Expressions;
using Dal.Entities;
using Dal.Helpers;
using Domain.Models.SearchModels;

namespace Dal.Repositories.AcademicDisciplines;

public class AcademicDisciplinePredicateBuilder : IPredicateBuilder<DbAcademicDiscipline, AcademicDisciplineSearchModel>
{
    public Expression<Func<DbAcademicDiscipline, bool>> Predicate { get; } = PredicateBuilderExtensions.True<DbAcademicDiscipline>();

    public Expression<Func<DbAcademicDiscipline, bool>> Build(AcademicDisciplineSearchModel searchModel)
    {
        return Predicate;
    }
}