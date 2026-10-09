using System;
using System.Collections.Generic;

namespace Data.InMemory
{
    public interface IInMemoryStore
    {
        /// <summary>The committed state of an entity, or null.</summary>
        StoredEntity Find(Type entityType, Guid id);

        /// <summary>The committed state of every entity of a type.</summary>
        IReadOnlyList<StoredEntity> FindAll(Type entityType);

        /// <summary>
        /// Applies all changes atomically: either every change is applied or none is on a conflict
        /// (ie. insert of an existing id, update or delete of a changed or missing entity).
        /// Returns the version now carried by every entity the commit wrote.
        /// </summary>
        long Apply(IReadOnlyCollection<StoreChange> changes);
    }
}