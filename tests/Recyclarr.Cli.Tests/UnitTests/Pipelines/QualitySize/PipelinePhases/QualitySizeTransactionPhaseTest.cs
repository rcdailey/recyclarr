using Recyclarr.Cli.Pipelines.QualitySize;
using Recyclarr.Cli.Pipelines.QualitySize.Models;
using Recyclarr.Cli.Pipelines.QualitySize.PipelinePhases;
using Recyclarr.ServarrApi.QualityDefinition;
using Recyclarr.TestLibrary.Core;

namespace Recyclarr.Cli.Tests.UnitTests.Pipelines.QualitySize.PipelinePhases;

public class QualitySizeTransactionPhaseTest
{
    [Test, AutoMockData]
    public void Skip_guide_qualities_that_do_not_exist_in_service(QualitySizeTransactionPhase sut)
    {
        var context = new QualitySizePipelineContext
        {
            ConfigOutput = new ProcessedQualitySizeData(
                "",
                [
                    NewQualitySize.WithLimits("non_existent1", 0, 2, 1),
                    NewQualitySize.WithLimits("non_existent2", 0, 2, 1),
                ]
            ),
            ApiFetchOutput = new List<ServiceQualityDefinitionItem>
            {
                new() { Quality = new ServiceQualityItem { Name = "exists" } },
            },
        };

        sut.Execute(context);

        context.TransactionOutput.Should().BeEmpty();
    }

    [Test, AutoMockData]
    public void Skip_guide_qualities_that_are_not_different_from_service(
        QualitySizeTransactionPhase sut
    )
    {
        var context = new QualitySizePipelineContext
        {
            ConfigOutput = new ProcessedQualitySizeData(
                "",
                [
                    NewQualitySize.WithLimits("same1", 0, 2, 1),
                    NewQualitySize.WithLimits("same2", 0, 2, 1),
                ]
            ),
            ApiFetchOutput = new List<ServiceQualityDefinitionItem>
            {
                new()
                {
                    Quality = new ServiceQualityItem { Name = "same1" },
                    MinSize = 0,
                    MaxSize = 2,
                    PreferredSize = 1,
                },
                new()
                {
                    Quality = new ServiceQualityItem { Name = "same2" },
                    MinSize = 0,
                    MaxSize = 2,
                    PreferredSize = 1,
                },
            },
        };

        sut.Execute(context);

        context.TransactionOutput.Should().BeEmpty();
    }

    [Test, AutoMockData]
    public void Sync_guide_qualities_that_are_different_from_service(
        QualitySizeTransactionPhase sut
    )
    {
        var context = new QualitySizePipelineContext
        {
            ConfigOutput = new ProcessedQualitySizeData(
                "",
                [
                    NewQualitySize.WithLimits("same1", 0, 2, 1),
                    NewQualitySize.WithLimits("different1", 0, 3, 1),
                ]
            ),
            ApiFetchOutput = new List<ServiceQualityDefinitionItem>
            {
                new()
                {
                    Quality = new ServiceQualityItem { Name = "same1" },
                    MinSize = 0,
                    MaxSize = 2,
                    PreferredSize = 1,
                },
                new()
                {
                    Quality = new ServiceQualityItem { Name = "different1" },
                    MinSize = 0,
                    MaxSize = 2,
                    PreferredSize = 1,
                },
            },
        };

        sut.Execute(context);

        context
            .TransactionOutput.Should()
            .BeEquivalentTo(
                new List<ServiceQualityDefinitionItem>
                {
                    new()
                    {
                        Quality = new ServiceQualityItem { Name = "different1" },
                        MinSize = 0,
                        MaxSize = 3,
                        PreferredSize = 1,
                    },
                }
            );
    }
}
