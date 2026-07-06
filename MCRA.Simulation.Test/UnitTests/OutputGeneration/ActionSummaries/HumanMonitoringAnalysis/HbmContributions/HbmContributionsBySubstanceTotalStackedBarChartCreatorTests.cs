using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.HumanMonitoringAnalysis {

    [TestClass]
    public class HbmContributionsBySubstanceTotalStackedBarChartCreatorTests : ChartCreatorTestBase {

        [TestMethod]
        public void HbmContributionsBySubstanceUpperStackedBarChartCreator_TestCreate() {
            var stratifiedRecords = new List<HbmContributionBySubstanceRecord>() {
                new() {
                    Stratification = "Stratification1",
                    Contribution = 0.10,
                    SubstanceCode = "Substance1",
                    SubstanceName = "Substance1"
                },
                new() {
                    Stratification = "Stratification1",
                    Contribution = 0.20,
                    SubstanceCode = "Substance2",
                    SubstanceName = "Substance2"
                },
                new() {
                    Stratification = "Stratification2",
                    Contribution = 0.20,
                    SubstanceCode = "Substance1",
                    SubstanceName = "Substance1"
                },
                new() {
                    Stratification = "Stratification2",
                    Contribution = .50,
                    SubstanceCode = "Substance2",
                    SubstanceName = "Substance2"
                }
            };
            var chart = new HbmContributionsBySubstanceTotalStackedBarChartCreator("Model.SectionId", stratifiedRecords, false);
            RenderChart(chart, "TestTotalDistribution");
        }
    }
}
