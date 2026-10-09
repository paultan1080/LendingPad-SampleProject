using Common.Exceptions;

namespace Core.Services
{
    public static class Paging
    {
        public const int DefaultPageSize = 50;

        // Upper bound for a single page
        public const int MaxPageSize = 1024;

        public static void EnsureValid(int skip, int take)
        {
            if (skip < 0)
            {
                throw new DomainValidationException(nameof(skip), "Skip cannot be negative.");
            }
            if (take < 1 || take > MaxPageSize)
            {
                throw new DomainValidationException(nameof(take), $"Take must be between 1 and {MaxPageSize}.");
            }
        }
    }
}