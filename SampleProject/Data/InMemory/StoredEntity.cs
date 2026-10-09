using System;

namespace Data.InMemory
{
    /// <summary>An entity's committed state (immutable snapshot) + a version for optimistic concurrency.</summary>
    public sealed class StoredEntity
    {
        public StoredEntity(Guid id, string snapshot, long version)
        {
            Id = id;
            Snapshot = snapshot;
            Version = version;
        }

        public Guid Id { get; }
        public string Snapshot { get; }
        public long Version { get; }
    }
}