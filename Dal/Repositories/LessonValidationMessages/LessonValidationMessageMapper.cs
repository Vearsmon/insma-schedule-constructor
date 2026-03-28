using System.Diagnostics.CodeAnalysis;
using Dal.Entities;
using Dal.Mapping;
using Domain.Models;

namespace Dal.Repositories.LessonValidationMessages;

public class LessonValidationMessageMapper : IRepositoryMapper<DbLessonValidationMessage, LessonValidationMessage>
{
    [return: NotNullIfNotNull("entity")]
    public LessonValidationMessage? Map(DbLessonValidationMessage? entity)
    {
        return MappingRegister.Map(entity);
    }

    public void Update(DbLessonValidationMessage entity, LessonValidationMessage model)
    {
        MappingRegister.Update(model, entity);
    }
}