using MCRA.Utils.Statistics;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MCRA.General;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.HazardCharacterisations {
    /// <summary>
    /// OutputGeneration, ActionSummaries, HazardCharacterisations
    /// </summary>
    [TestClass]
    public class HazardCharacterisationsChartCreatorTests : ChartCreatorTestBase {

        /// <summary>
        /// Create chart and test HazardCharacterisationsSummarySectionview
        /// </summary>
        [TestMethod]
        public void HazardCharacterisationsChartCreator_TestCreateNominal() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(10);
            var records = substances
                .Select((r, ix) => new HazardCharacterisationsSummaryRecord() {
                    CompoundCode = r.Code,
                    CompoundName = r.Name,
                    HazardCharacterisation = LogNormalDistribution.Draw(random, 5, 2),
                    TargetDoseUncertaintyValues = new List<double>(),
                })
                .ToList();

            var section = new HazardCharacterisationsSummarySection() {
                Records = records,
            };
            var chart = new HazardCharacterisationsChartCreator(section.SectionId, ExposureTarget.DietaryExposureTarget, records, "unit");
            RenderChart(chart, $"TestCreateNominal");
            AssertIsValidView(section);
        }

        /// <summary>
        /// Create chart and test TargetDosesSummarySection view
        /// </summary>
        [TestMethod]
        public void TargetDosesChartCreator_TestCreateUncertain() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(10);
            var records = substances
                .Select((r, ix) => {
                    var dose = LogNormalDistribution.Draw(random, 5, 2);
                    return new HazardCharacterisationsSummaryRecord() {
                        CompoundCode = r.Code,
                        CompoundName = r.Name,
                        HazardCharacterisation = dose,
                        TargetDoseUncertaintyValues = ContinuousUniformDistribution.Samples(random, dose * .5, dose * 1.5, 100)
                    };
                })
                .ToList();

            var section = new HazardCharacterisationsSummarySection() {
                Records = records,
            };

            var chart = new HazardCharacterisationsChartCreator(section.SectionId, ExposureTarget.DietaryExposureTarget, records, "unit");
            RenderChart(chart, $"TestCreateUncertain");
            AssertIsValidView(section);
        }
    }
}
