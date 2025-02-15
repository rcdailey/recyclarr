using Recyclarr.Cache;

namespace Recyclarr.Core.Tests.UnitTests.Cache;

[CacheObjectName("test-cache")]
public record TestCacheObject() : CacheObject(LatestVersion)
{
    public new const int LatestVersion = 1;
    public string? ExtraData { get; init; }
}

public class TestCache(TestCacheObject cacheObject) : BaseCache(cacheObject);
