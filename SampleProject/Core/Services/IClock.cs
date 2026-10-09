using System;

namespace Core.Services
{
    public interface IClock
    {
        DateTime UtcNow { get; }
    }
}