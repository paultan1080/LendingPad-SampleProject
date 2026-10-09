using System;
using System.Collections.Generic;
using System.Linq;
using BusinessEntities;
using Common;

namespace Data.InMemory
{
    /// <summary>
    /// The request's unit of work over the in-memory store, with some RavenDB semantics that the services 
    /// relied on: an identity map of private copies of the committed state, automatic change
    /// tracking of loaded entities, and an atomic commit guarded by optimistic concurrency on what the
    /// request writes (reads not re-checked at commit).
    /// </summary>
    [AutoRegister]
    public class InMemorySession : IDataSession, IUnitOfWork
    {
        private readonly IInMemoryStore _store;
        private readonly Dictionary<(Type, Guid), TrackedEntity> _tracked = new Dictionary<(Type, Guid), TrackedEntity>();

        public InMemorySession(IInMemoryStore store)
        {
            _store = store;
        }

        public T Load<T>(Guid id) where T : IdObject
        {
            if (_tracked.TryGetValue((typeof(T), id), out var tracked))
            {
                return tracked.IsDeleted ? null : (T)tracked.Entity;
            }

            var stored = _store.Find(typeof(T), id);
            return stored == null ? null : (T)Track(typeof(T), stored).Entity;
        }

        public IEnumerable<T> Query<T>() where T : IdObject
        {
            return _store.FindAll(typeof(T))
                         .Select(stored => _tracked.TryGetValue((typeof(T), stored.Id), out var tracked) ? tracked : Track(typeof(T), stored))
                         .Where(tracked => !tracked.IsDeleted)
                         .Select(tracked => (T)tracked.Entity)
                         .ToList();
        }

        public void Store<T>(T entity) where T : IdObject
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            var key = (typeof(T), entity.Id);
            if (_tracked.TryGetValue(key, out var tracked))
            {
                if (!ReferenceEquals(tracked.Entity, entity))
                {
                    throw new InvalidOperationException($"Another instance of {typeof(T).Name} '{entity.Id}' is already tracked by this session.");
                }
                tracked.IsDeleted = false;
                return;
            }

            _tracked.Add(key, new TrackedEntity(entity, null, 0));
        }

        public void Delete<T>(T entity) where T : IdObject
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            var key = (typeof(T), entity.Id);
            if (!_tracked.TryGetValue(key, out var tracked) || !ReferenceEquals(tracked.Entity, entity))
            {
                throw new InvalidOperationException($"{typeof(T).Name} '{entity.Id}' was not loaded or stored by this session.");
            }

            if (tracked.IsNew)
            {
                _tracked.Remove(key);
            }
            else
            {
                tracked.IsDeleted = true;
            }
        }

        public void Commit()
        {
            var changes = new List<StoreChange>();
            foreach (var pair in _tracked)
            {
                var entityType = pair.Key.Item1;
                var tracked = pair.Value;
                if (tracked.IsDeleted)
                {
                    changes.Add(StoreChange.Delete(entityType, tracked.Entity.Id, tracked.Version));
                    continue;
                }

                var snapshot = EntitySnapshots.Serialize(tracked.Entity);
                if (tracked.IsNew)
                {
                    changes.Add(StoreChange.Insert(entityType, tracked.Entity.Id, snapshot));
                }
                else if (snapshot != tracked.Snapshot)
                {
                    changes.Add(StoreChange.Update(entityType, tracked.Entity.Id, tracked.Version, snapshot));
                }
            }

            if (changes.Count == 0)
            {
                return;
            }

            var version = _store.Apply(changes);

            foreach (var change in changes)
            {
                var key = (change.EntityType, change.Id);
                if (change.Kind == StoreChangeKind.Delete)
                {
                    _tracked.Remove(key);
                }
                else
                {
                    _tracked[key].Committed(change.Snapshot, version);
                }
            }
        }

        private TrackedEntity Track(Type entityType, StoredEntity stored)
        {
            var entity = (IdObject)EntitySnapshots.Deserialize(stored.Snapshot, entityType);
            var tracked = new TrackedEntity(entity, stored.Snapshot, stored.Version);
            _tracked.Add((entityType, stored.Id), tracked);
            return tracked;
        }

        private class TrackedEntity
        {
            public TrackedEntity(IdObject entity, string snapshot, long version)
            {
                Entity = entity;
                Snapshot = snapshot;
                Version = version;
            }

            public IdObject Entity { get; }

            /// <summary>The state last read from or written to the store; null while the entity is new.</summary>
            public string Snapshot { get; private set; }

            public long Version { get; private set; }
            public bool IsNew => Snapshot == null;
            public bool IsDeleted { get; set; }

            public void Committed(string snapshot, long version)
            {
                Snapshot = snapshot;
                Version = version;
            }
        }
    }
}