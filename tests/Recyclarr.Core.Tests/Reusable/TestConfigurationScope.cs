using Autofac;
using Recyclarr.Config;

namespace Recyclarr.Core.Tests.Reusable;

public class TestConfigurationScope(ILifetimeScope scope) : ConfigurationScope(scope)
{
    public T Resolve<T>()
        where T : notnull
    {
        return Scope.Resolve<T>();
    }
}
