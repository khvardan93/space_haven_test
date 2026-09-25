using System;

namespace Offline
{
    public interface ITimeProvider
    {
        DateTime UtcNow { get; }
    }
}
