using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Exceptions;

namespace Data.InMemory
{
    /// <summary>
    /// Process-wide storage replacing RavenDB. Holds snapshots of entity state.
    /// Data lives within the application process.
    /// </summary>
    [AutoRegister(AutoRegisterTypes.Singleton)]
    public class InMemoryStore : IInMemoryStore
    {
        private readonly object _sync = new object();
        private readonly Dictionary<Type, Dictionary<Guid, StoredEntity>> _tables = new Dictionary<Type, Dictionary<Guid, StoredEntity>>();

        // Store-wide and never reused (like RavenDB's etags): an entity deleted and re-created with the
        // same id gets a new version (so a req still holding the old one cannot overwrite it).
        private long _lastVersion;

        public StoredEntity Find(Type entityType, Guid id)
        {
            lock (_sync)
            {
                return Table(entityType).TryGetValue(id, out var entity) ? entity : null;
            }
        }

        public IReadOnlyList<StoredEntity> FindAll(Type entityType)
        {
            lock (_sync)
            {
                return Table(entityType).Values.ToList();
            }
        }

        public long Apply(IReadOnlyCollection<StoreChange> changes)
        {
            lock (_sync)
            {
                foreach (var change in changes)
                {
                    EnsureApplicable(change);
                }

                var version = ++_lastVersion;
                foreach (var change in changes)
                {
                    Execute(change, version);
                }
                return version;
            }
        }

        private void EnsureApplicable(StoreChange change)
        {
            Table(change.EntityType).TryGetValue(change.Id, out var current);
            if (change.Kind == StoreChangeKind.Insert)
            {
                if (current != null)
                {
                    throw new ConflictException($"{EntityNames.WithArticle(change.EntityType)} with id '{change.Id}' already exists.");
                }
            }
            else if (current == null || current.Version != change.ExpectedVersion)
            {
                throw new ConflictException($"The {EntityNames.Of(change.EntityType)} '{change.Id}' was changed or deleted by another request. Reload it and try again.");
            }
        }

        private void Execute(StoreChange change, long version)
        {
            var table = Table(change.EntityType);
            if (change.Kind == StoreChangeKind.Delete)
            {
                table.Remove(change.Id);
            }
            else
            {
                table[change.Id] = new StoredEntity(change.Id, change.Snapshot, version);
            }
        }

        private Dictionary<Guid, StoredEntity> Table(Type entityType)
        {
            if (!_tables.TryGetValue(entityType, out var table))
            {
                table = new Dictionary<Guid, StoredEntity>();
                _tables.Add(entityType, table);
            }
            return table;
        }
    }
}