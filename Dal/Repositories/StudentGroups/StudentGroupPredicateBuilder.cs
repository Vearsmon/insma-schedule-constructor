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
        return Predicate
                .AndIf(searchModel.Id.HasValue, f => f.Id == searchModel.Id)
                .AndIf(searchModel.ScheduleId.HasValue, f => f.ScheduleId == searchModel.ScheduleId)
                .AndIf(searchModel.StudentGroupTypes.Length > 0, f => searchModel.StudentGroupTypes.Contains(f.StudentGroupType))
                .AndIf(searchModel.IsEmptyParents, f => f.Parents.Count == 0)
            ;
    }
}