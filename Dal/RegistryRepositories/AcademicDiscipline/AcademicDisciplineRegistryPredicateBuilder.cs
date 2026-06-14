using System.Linq.Expressions;
using Dal.Entities;
using Dal.Helpers;
using Domain.Models.RegistrySearchModels;

namespace Dal.RegistryRepositories.AcademicDiscipline;

public class AcademicDisciplineRegistryPredicateBuilder
    : IPredicateBuilder<DbAcademicDiscipline, AcademicDisciplineRegistryInternalSearchModel>
{
    public Expression<Func<DbAcademicDiscipline, bool>> Predicate { get; } =
        PredicateBuilderExtensions.True<DbAcademicDiscipline>();

    public Expression<Func<DbAcademicDiscipline, bool>> Build(
        AcademicDisciplineRegistryInternalSearchModel searchModel)
    {
        var nameLowerCaseTrimmed = string.IsNullOrEmpty(searchModel.Name) ? null : searchModel.Name!.ToLower().Trim().Replace("  ", " ");

        return Predicate
                .AndIf(!string.IsNullOrEmpty(nameLowerCaseTrimmed),
                    f => f.Name.ToLower().Contains(nameLowerCaseTrimmed!)
                         || f.AssociatedNames.Any(x => x.ToLower().Contains(nameLowerCaseTrimmed!)))
            ;
    }
}