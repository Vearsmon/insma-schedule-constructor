using System.Diagnostics.CodeAnalysis;
using Dal.Entities;
using Dal.Mapping;

namespace Dal.Repositories.LessonBatchInfo;

public class LessonBatchInfoMapper : IRepositoryMapper<DbLessonBatchInfo, Domain.Models.LessonBatchInfo>
{
    [return: NotNullIfNotNull("entity")]
    public Domain.Models.LessonBatchInfo? Map(DbLessonBatchInfo? entity) => LessonBatchInfoMappingRegister.MapEntityToModel(entity);

    public void Update(DbLessonBatchInfo entity, Domain.Models.LessonBatchInfo model)
    {
        LessonBatchInfoMappingRegister.UpdateEntityWithModel(model, entity);
    }
}