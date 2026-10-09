using System;
using BusinessEntities;
using Common;
using Common.Exceptions;
using Data.Repositories;

namespace Core.Services
{
    /// <summary>Checks for ids chosen by the client when it creates an entity.</summary>
    public static class EntityIds
    {
        public static void EnsureAvailable<T>(IRepository<T> repository, Guid id) where T : IdObject
        {
            if (id == Guid.Empty)
            {
                throw new DomainValidationException(nameof(id), $"A non-empty {EntityNames.Of(typeof(T))} id is required.");
            }

            // A concurrent create racing past this check is still rejected by optimistic concurrency when the UOW commits.
            if (repository.Get(id) != null)
            {
                throw new ConflictException($"{EntityNames.WithArticle(typeof(T))} with id '{id}' already exists.");
            }
        }
    }
}