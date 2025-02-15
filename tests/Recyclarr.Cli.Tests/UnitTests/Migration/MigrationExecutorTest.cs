using System.Diagnostics.CodeAnalysis;
using Autofac.Extras.Ordering;
using Recyclarr.Cli.Migration;
using Recyclarr.Cli.Migration.Steps;
using Spectre.Console.Testing;

namespace Recyclarr.Cli.Tests.UnitTests.Migration;

public class MigrationExecutorTest
{
    [Test]
    public void Step_not_executed_if_check_returns_false()
    {
        using var console = new TestConsole();
        var step = Substitute.For<IMigrationStep>();
        var executor = new MigrationExecutor(new[] { step }.AsOrdered(), console);

        step.CheckIfNeeded().Returns(false);

        executor.PerformAllMigrationSteps(false);

        step.Received().CheckIfNeeded();
        step.DidNotReceive().Execute(null);
    }

    [Test]
    public void Step_executed_if_check_returns_true()
    {
        using var console = new TestConsole();
        var step = Substitute.For<IMigrationStep>();
        var executor = new MigrationExecutor(new[] { step }.AsOrdered(), console);

        step.CheckIfNeeded().Returns(true);

        executor.PerformAllMigrationSteps(false);

        step.Received().CheckIfNeeded();
        step.Received().Execute(null);
    }

    [Test]
    public void Steps_executed_in_ascending_order()
    {
        using var console = new TestConsole();

        var steps = new[]
        {
            Substitute.For<IMigrationStep>(),
            Substitute.For<IMigrationStep>(),
            Substitute.For<IMigrationStep>(),
        };

        var executor = new MigrationExecutor(steps.AsOrdered(), console);

        executor.PerformAllMigrationSteps(false);

        Received.InOrder(() =>
        {
            steps[0].CheckIfNeeded();
            steps[1].CheckIfNeeded();
            steps[2].CheckIfNeeded();
        });
    }

    [Test]
    public void Exception_converted_to_migration_exception()
    {
        using var console = new TestConsole();
        var step = Substitute.For<IMigrationStep>();
        var executor = new MigrationExecutor(new[] { step }.AsOrdered(), console);

        step.CheckIfNeeded().Returns(true);
        step.When(x => x.Execute(null)).Throw(new ArgumentException("test message"));

        var act = () => executor.PerformAllMigrationSteps(false);

        act.Should()
            .Throw<MigrationException>()
            .Which.OriginalException.Message.Should()
            .Be("test message");
    }

    [Test]
    [SuppressMessage(
        "SonarLint",
        "S3928:Parameter names used into ArgumentException constructors should match an existing one",
        Justification = "Used in unit test only"
    )]
    public void Migration_exceptions_are_not_converted()
    {
        using var console = new TestConsole();
        var step = Substitute.For<IMigrationStep>();
        var executor = new MigrationExecutor(new[] { step }.AsOrdered(), console);
        var exception = new MigrationException(new ArgumentException(), "a", ["b"]);

        step.CheckIfNeeded().Returns(true);
        step.When(x => x.Execute(null)).Throw(exception);

        var act = () => executor.PerformAllMigrationSteps(false);

        act.Should().Throw<MigrationException>().Which.Should().Be(exception);
    }
}
