using System;

namespace Data.InMemory
{
    public enum StoreChangeKind
    {
        Insert = 1,
        Update = 2,
        Delete = 3
    }

    /// <summary>One write per commit. Updates/deletes carry version they were based on.</summary>
    public sealed class StoreChange
    {
        private StoreChange(StoreChangeKind kind, Type entityType, Guid id, long expectedVersion, string snapshot)
        {
            Kind = kind;
            EntityType = entityType;
            Id = id;
            ExpectedVersion = expectedVersion;
            Snapshot = snapshot;
        }

        public StoreChangeKind Kind { get; }
        public Type EntityType { get; }
        public Guid Id { get; }
        public long ExpectedVersion { get; }
        public string Snapshot { get; }

        public static StoreChange Insert(Type entityType, Guid id, string snapshot)
        {
            return new StoreChange(StoreChangeKind.Insert, entityType, id, 0, snapshot);
        }

        public static StoreChange Update(Type entityType, Guid id, long expectedVersion, string snapshot)
        {
            return new StoreChange(StoreChangeKind.Update, entityType, id, expectedVersion, snapshot);
        }

        public static StoreChange Delete(Type entityType, Guid id, long expectedVersion)
        {
            return new StoreChange(StoreChangeKind.Delete, entityType, id, expectedVersion, null);
        }
    }
}