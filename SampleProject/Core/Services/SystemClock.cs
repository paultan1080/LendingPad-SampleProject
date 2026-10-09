using System;
using Common;

namespace Core.Services
{
    [AutoRegister(AutoRegisterTypes.Singleton)]
    public class SystemClock : IClock
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}