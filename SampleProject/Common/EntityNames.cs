using System;

namespace Common
{
    /// <summary>How entity types are named in messages: "user", "A user", "An order".</summary>
    public static class EntityNames
    {
        public static string Of(Type entityType)
        {
            return entityType.Name.ToLowerInvariant();
        }

        public static string WithArticle(Type entityType)
        {
            // "an order", "a user" ('u' as in "you").
            var name = Of(entityType);
            return ("aeio".IndexOf(name[0]) >= 0 ? "An " : "A ") + name;
        }
    }
}