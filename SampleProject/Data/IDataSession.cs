using System;
using System.Collections.Generic;
using BusinessEntities;

namespace Data
{
    /// <summary>
    /// The request's view of the stored entities. Loaded entities are private copies tracked by the
    /// session; nothing reaches storage until the unit of work commits.
    /// </summary>
    public interface IDataSession
    {
        /// <summary>The entity with the given id, or null. Repeated loads return the same instance.</summary>
        T Load<T>(Guid id) where T : IdObject;

        /// <summary>All committed entities of a type, as tracked instances.</summary>
        IEnumerable<T> Query<T>() where T : IdObject;

        /// <summary>Starts tracking a new entity; it is inserted on commit.</summary>
        void Store<T>(T entity) where T : IdObject;

        /// <summary>Marks a tracked entity for deletion on commit.</summary>
        void Delete<T>(T entity) where T : IdObject;
    }
}