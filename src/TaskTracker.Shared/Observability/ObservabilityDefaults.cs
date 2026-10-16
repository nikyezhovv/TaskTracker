using Microsoft.AspNetCore.Http;

namespace TaskTracker.Shared.Observability;

internal static class ObservabilityDefaults
{
    public static readonly PathString[] NoisePaths =
    [
        new("/health"),
        new("/metrics")
    ];
}
