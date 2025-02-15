using Recyclarr.Config.Secrets;

namespace Recyclarr.Core.Tests.UnitTests.Config.Secrets;

public class SecretNotFoundExceptionTest
{
    [Test]
    public void Properties_get_initialized()
    {
        var sut = new SecretNotFoundException(15, "key");
        sut.Line.Should().Be(15);
        sut.SecretKey.Should().Be("key");
    }
}
