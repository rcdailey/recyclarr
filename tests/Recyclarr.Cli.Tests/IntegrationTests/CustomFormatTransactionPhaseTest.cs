using Recyclarr.Cli.Pipelines.CustomFormat;
using Recyclarr.Cli.Pipelines.CustomFormat.Cache;
using Recyclarr.Cli.Pipelines.CustomFormat.Models;
using Recyclarr.Cli.Pipelines.CustomFormat.PipelinePhases;
using Recyclarr.Config;
using Recyclarr.TestLibrary.Cli;
using Recyclarr.TestLibrary.Core;
using Recyclarr.TrashGuide.CustomFormat;

namespace Recyclarr.Cli.Tests.IntegrationTests;

public class CustomFormatTransactionPhaseTest : CliIntegrationFixture
{
    [Test]
    public void Add_new_cf()
    {
        var scopeFactory = Resolve<ConfigurationScopeFactory>();
        using var scope = scopeFactory.Start<TestConfigurationScope>(NewConfig.Radarr());
        var sut = scope.Resolve<CustomFormatTransactionPhase>();

        var context = new CustomFormatPipelineContext
        {
            Cache = CfCache.New(),
            ApiFetchOutput = [],
            ConfigOutput = [NewCf.Data("one", "cf1")],
        };

        sut.Execute(context);

        context
            .TransactionOutput.Should()
            .BeEquivalentTo(
                new CustomFormatTransactionData { NewCustomFormats = { NewCf.Data("one", "cf1") } }
            );
    }

    [Test]
    public void Update_cf_by_matching_name()
    {
        var scopeFactory = Resolve<ConfigurationScopeFactory>();
        using var scope = scopeFactory.Start<TestConfigurationScope>(NewConfig.Radarr());
        var sut = scope.Resolve<CustomFormatTransactionPhase>();

        var context = new CustomFormatPipelineContext
        {
            Cache = CfCache.New(),
            ApiFetchOutput = [new CustomFormatData { Name = "one" }],
            ConfigOutput =
            [
                new CustomFormatData
                {
                    Name = "one",
                    TrashId = "cf1",
                    // Only set the below value to make it different from the service CF
                    IncludeCustomFormatWhenRenaming = true,
                },
            ],
        };

        sut.Execute(context);

        context
            .TransactionOutput.Should()
            .BeEquivalentTo(
                new CustomFormatTransactionData
                {
                    UpdatedCustomFormats =
                    {
                        NewCf.Data("one", "cf1") with
                        {
                            IncludeCustomFormatWhenRenaming = true,
                        },
                    },
                }
            );
    }

    [Test]
    public void Update_cf_by_matching_id_different_names()
    {
        var scopeFactory = Resolve<ConfigurationScopeFactory>();
        using var scope = scopeFactory.Start<TestConfigurationScope>(NewConfig.Radarr());
        var sut = scope.Resolve<CustomFormatTransactionPhase>();

        var context = new CustomFormatPipelineContext
        {
            Cache = CfCache.New(new TrashIdMapping("cf1", "", 2)),
            ApiFetchOutput = [new CustomFormatData { Name = "different2", Id = 2 }],
            ConfigOutput =
            [
                new CustomFormatData
                {
                    Name = "different1",
                    TrashId = "cf1",
                    // Only set the below value to make it different from the service CF
                    IncludeCustomFormatWhenRenaming = true,
                },
            ],
        };

        sut.Execute(context);

        context
            .TransactionOutput.Should()
            .BeEquivalentTo(
                new CustomFormatTransactionData
                {
                    UpdatedCustomFormats =
                    {
                        NewCf.Data("different1", "cf1", 2) with
                        {
                            IncludeCustomFormatWhenRenaming = true,
                        },
                    },
                }
            );
    }

    [Test]
    public void Update_cf_by_matching_id_same_names()
    {
        var scopeFactory = Resolve<ConfigurationScopeFactory>();
        using var scope = scopeFactory.Start<TestConfigurationScope>(
            NewConfig.Radarr() with
            {
                ReplaceExistingCustomFormats = true,
            }
        );

        var sut = scope.Resolve<CustomFormatTransactionPhase>();

        var context = new CustomFormatPipelineContext
        {
            Cache = CfCache.New(),
            ApiFetchOutput = [new CustomFormatData { Name = "different1", Id = 2 }],
            ConfigOutput =
            [
                new CustomFormatData
                {
                    Name = "different1",
                    TrashId = "cf1",
                    // Only set the below value to make it different from the service CF
                    IncludeCustomFormatWhenRenaming = true,
                },
            ],
        };

        sut.Execute(context);

        context
            .TransactionOutput.Should()
            .BeEquivalentTo(
                new CustomFormatTransactionData
                {
                    UpdatedCustomFormats =
                    {
                        NewCf.Data("different1", "cf1", 2) with
                        {
                            IncludeCustomFormatWhenRenaming = true,
                        },
                    },
                }
            );
    }

    [Test]
    public void Conflicting_cf_when_new_cf_has_name_of_existing()
    {
        var scopeFactory = Resolve<ConfigurationScopeFactory>();
        using var scope = scopeFactory.Start<TestConfigurationScope>(
            NewConfig.Radarr() with
            {
                ReplaceExistingCustomFormats = false,
            }
        );

        var sut = scope.Resolve<CustomFormatTransactionPhase>();

        var context = new CustomFormatPipelineContext
        {
            Cache = CfCache.New(),
            ApiFetchOutput =
            [
                new CustomFormatData { Name = "one", Id = 2 },
                new CustomFormatData { Name = "two", Id = 1 },
            ],
            ConfigOutput = [NewCf.Data("one", "cf1")],
        };

        sut.Execute(context);

        context
            .TransactionOutput.Should()
            .BeEquivalentTo(
                new CustomFormatTransactionData
                {
                    ConflictingCustomFormats =
                    {
                        new ConflictingCustomFormat(context.ConfigOutput[0], 2),
                    },
                }
            );
    }

    [Test]
    public void Conflicting_cf_when_cached_cf_has_name_of_existing()
    {
        var scopeFactory = Resolve<ConfigurationScopeFactory>();
        using var scope = scopeFactory.Start<TestConfigurationScope>(
            NewConfig.Radarr() with
            {
                ReplaceExistingCustomFormats = false,
            }
        );

        var sut = scope.Resolve<CustomFormatTransactionPhase>();

        var context = new CustomFormatPipelineContext
        {
            Cache = CfCache.New(new TrashIdMapping("cf1", "one", 1)),
            ApiFetchOutput =
            [
                new CustomFormatData { Name = "one", Id = 2 },
                new CustomFormatData { Name = "two", Id = 1 },
            ],
            ConfigOutput = [NewCf.Data("one", "cf1")],
        };

        sut.Execute(context);

        context
            .TransactionOutput.Should()
            .BeEquivalentTo(
                new CustomFormatTransactionData
                {
                    ConflictingCustomFormats =
                    {
                        new ConflictingCustomFormat(context.ConfigOutput[0], 2),
                    },
                }
            );
    }

    [Test]
    public void Updated_cf_with_matching_name_and_id()
    {
        var scopeFactory = Resolve<ConfigurationScopeFactory>();
        using var scope = scopeFactory.Start<TestConfigurationScope>(
            NewConfig.Radarr() with
            {
                ReplaceExistingCustomFormats = false,
            }
        );

        var sut = scope.Resolve<CustomFormatTransactionPhase>();

        var context = new CustomFormatPipelineContext
        {
            Cache = CfCache.New(new TrashIdMapping("cf1", "one", 1)),
            ApiFetchOutput =
            [
                new CustomFormatData { Name = "two", Id = 2 },
                new CustomFormatData { Name = "one", Id = 1 },
            ],
            ConfigOutput =
            [
                new CustomFormatData
                {
                    Name = "one",
                    TrashId = "cf1",
                    // Only set the below value to make it different from the service CF
                    IncludeCustomFormatWhenRenaming = true,
                },
            ],
        };

        sut.Execute(context);

        context
            .TransactionOutput.Should()
            .BeEquivalentTo(
                new CustomFormatTransactionData
                {
                    UpdatedCustomFormats =
                    {
                        NewCf.Data("one", "cf1", 1) with
                        {
                            IncludeCustomFormatWhenRenaming = true,
                        },
                    },
                }
            );
    }

    [Test]
    public void Unchanged_cfs_with_replace_enabled()
    {
        var scopeFactory = Resolve<ConfigurationScopeFactory>();
        using var scope = scopeFactory.Start<TestConfigurationScope>(
            NewConfig.Radarr() with
            {
                ReplaceExistingCustomFormats = true,
            }
        );

        var sut = scope.Resolve<CustomFormatTransactionPhase>();

        var context = new CustomFormatPipelineContext
        {
            Cache = CfCache.New(),
            ApiFetchOutput = [new CustomFormatData { Name = "one", Id = 1 }],
            ConfigOutput = [NewCf.Data("one", "cf1")],
        };

        sut.Execute(context);

        context
            .TransactionOutput.Should()
            .BeEquivalentTo(
                new CustomFormatTransactionData
                {
                    UnchangedCustomFormats = { context.ConfigOutput[0] },
                }
            );
    }

    [Test]
    public void Unchanged_cfs_without_replace()
    {
        var scopeFactory = Resolve<ConfigurationScopeFactory>();
        using var scope = scopeFactory.Start<TestConfigurationScope>(
            NewConfig.Radarr() with
            {
                ReplaceExistingCustomFormats = false,
            }
        );

        var sut = scope.Resolve<CustomFormatTransactionPhase>();

        var context = new CustomFormatPipelineContext
        {
            Cache = CfCache.New(new TrashIdMapping("cf1", "one", 1)),
            ApiFetchOutput = [new CustomFormatData { Name = "one", Id = 1 }],
            ConfigOutput = [NewCf.Data("one", "cf1")],
        };

        sut.Execute(context);

        context
            .TransactionOutput.Should()
            .BeEquivalentTo(
                new CustomFormatTransactionData
                {
                    UnchangedCustomFormats = { context.ConfigOutput[0] },
                }
            );
    }

    [Test]
    public void Deleted_cfs_when_enabled()
    {
        var scopeFactory = Resolve<ConfigurationScopeFactory>();
        using var scope = scopeFactory.Start<TestConfigurationScope>(
            NewConfig.Radarr() with
            {
                DeleteOldCustomFormats = true,
            }
        );

        var sut = scope.Resolve<CustomFormatTransactionPhase>();

        var context = new CustomFormatPipelineContext
        {
            Cache = CfCache.New(new TrashIdMapping("cf2", "two", 2)),
            ApiFetchOutput = [new CustomFormatData { Name = "two", Id = 2 }],
            ConfigOutput = [],
        };

        sut.Execute(context);

        context
            .TransactionOutput.Should()
            .BeEquivalentTo(
                new CustomFormatTransactionData
                {
                    DeletedCustomFormats = { new TrashIdMapping("cf2", "two", 2) },
                }
            );
    }

    [Test]
    public void No_deleted_cfs_when_disabled()
    {
        var scopeFactory = Resolve<ConfigurationScopeFactory>();
        using var scope = scopeFactory.Start<TestConfigurationScope>(
            NewConfig.Radarr() with
            {
                DeleteOldCustomFormats = false,
            }
        );

        var sut = scope.Resolve<CustomFormatTransactionPhase>();

        var context = new CustomFormatPipelineContext
        {
            Cache = CfCache.New(new TrashIdMapping("cf2", "two", 2)),
            ApiFetchOutput = [new CustomFormatData { Name = "two", Id = 2 }],
            ConfigOutput = [],
        };

        sut.Execute(context);

        context.TransactionOutput.Should().BeEquivalentTo(new CustomFormatTransactionData());
    }

    [Test]
    public void Do_not_delete_cfs_in_config()
    {
        var scopeFactory = Resolve<ConfigurationScopeFactory>();
        using var scope = scopeFactory.Start<TestConfigurationScope>(NewConfig.Radarr());

        var sut = scope.Resolve<CustomFormatTransactionPhase>();

        var context = new CustomFormatPipelineContext
        {
            Cache = CfCache.New(new TrashIdMapping("cf2", "two", 2)),
            ApiFetchOutput = [new CustomFormatData { Name = "two", Id = 2 }],
            ConfigOutput = [NewCf.Data("two", "cf2", 2)],
        };

        sut.Execute(context);

        context.TransactionOutput.DeletedCustomFormats.Should().BeEmpty();
    }

    [Test]
    public void Add_new_cf_when_in_cache_but_not_in_service()
    {
        var scopeFactory = Resolve<ConfigurationScopeFactory>();
        using var scope = scopeFactory.Start<TestConfigurationScope>(NewConfig.Radarr());

        var sut = scope.Resolve<CustomFormatTransactionPhase>();

        var context = new CustomFormatPipelineContext
        {
            Cache = CfCache.New(new TrashIdMapping("cf2", "two", 200)),
            ApiFetchOutput = [],
            ConfigOutput = [NewCf.Data("two", "cf2", 2)],
        };

        sut.Execute(context);

        context
            .TransactionOutput.Should()
            .BeEquivalentTo(
                new CustomFormatTransactionData
                {
                    NewCustomFormats =
                    {
                        new CustomFormatData
                        {
                            Name = "two",
                            TrashId = "cf2",
                            Id = 200,
                        },
                    },
                }
            );
    }
}
