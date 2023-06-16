using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Risk {
    /// <summary>
    /// OutputGeneration, ActionSummaries, Risk, HazardExposure
    /// </summary>
    [TestClass]
    public class HazardExposureSectionTests : SectionTestBase {

        /// <summary>
        /// Summarize nominal, create chart, test HazardExposureSection view.
        /// </summary>
        [TestMethod]
        public void HazardExposureSection_TestSummarizeMoeNominal() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(5);
            var individuals = MockIndividualsGenerator.Create(100, 1, random);

            var hazardCharacterisations = MockHazardCharacterisationModelsGenerator.Create(new Effect(), substances, seed);

            var individualEffects = MockIndividualEffectsGenerator.Create(individuals, substances, random);

            var reference = substances.First();
            var cumulativeRisks = MockIndividualEffectsGenerator.ComputeCumulativeIndividualEffects(
                individuals,
                individualEffects,
                reference
            );

            var section = new HazardExposureSection();
            section.Summarize(
                individualEffects,
                cumulativeRisks,
                HealthEffectType.Risk,
                substances,
                reference,
                hazardCharacterisations,
                riskMetricType: RiskMetricType.MarginOfExposure,
                riskMetricCalculationType: RiskMetricCalculationType.RPFWeighted,
                90,
                1,
                10,
                10,
                2.5,
                97.5
            );

            Assert.AreEqual(substances.Count + 1, section.HazardExposureRecords.Count);
            Assert.IsTrue(!double.IsNaN(section.HazardExposureRecords[0].MeanExposure));
            Assert.IsTrue(!double.IsNaN(section.HazardExposureRecords[0].MedianHc));
            Assert.IsTrue(!double.IsNaN(section.HazardExposureRecords[0].LowerExposure));
            Assert.IsTrue(double.IsNaN(section.HazardExposureRecords[0].LowerExposure_UncLower));
            Assert.IsTrue(!double.IsNaN(section.HazardExposureRecords[0].UpperExposure));
            Assert.IsTrue(double.IsNaN(section.HazardExposureRecords[0].UpperExposure_UncUpper));

            AssertIsValidView(section);
            RenderView(section, filename: "TestSummarizeMoeNominal.html");
        }

        /// <summary>
        /// Summarize uncertain, create chart, test HazardExposureSection view
        /// </summary>
        [TestMethod]
        public void HazardExposureSection_TestSummarizeMoeUncertain() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(5);
            var individuals = MockIndividualsGenerator.Create(100, 1, random);

            var hazardCharacterisations = MockHazardCharacterisationModelsGenerator.Create(new Effect(), substances, seed);

            var individualEffects = MockIndividualEffectsGenerator.Create(individuals, substances, random);

            var reference = substances.First();
            var cumulativeRisks = MockIndividualEffectsGenerator.ComputeCumulativeIndividualEffects(
                individuals,
                individualEffects,
                reference
            );

            var section = new HazardExposureSection();
            section.Summarize(
                individualEffects,
                cumulativeRisks,
                HealthEffectType.Risk,
                substances,
                reference,
                hazardCharacterisations,
                riskMetricType: RiskMetricType.MarginOfExposure,
                riskMetricCalculationType: RiskMetricCalculationType.RPFWeighted,
                90,
                1,
                10,
                10,
                2.5,
                97.5
            );

            for (int i = 0; i < 100; i++) {
                var substanceIndividualEffectsUncertains = MockIndividualEffectsGenerator
                    .CreateUncertain(substances, individualEffects, random);
                var cumulativeRiskUncertains = MockIndividualEffectsGenerator
                    .ComputeCumulativeIndividualEffects(individuals, individualEffects, reference);

                section.SummarizeUncertainty(
                    substanceIndividualEffectsUncertains,
                    cumulativeRiskUncertains,
                    hazardCharacterisations,
                    substances,
                    reference,
                    riskMetricType: RiskMetricType.MarginOfExposure,
                    riskMetricCalculationType: RiskMetricCalculationType.RPFWeighted,
                    2.5,
                    97.5
                );
            }

            Assert.AreEqual(substances.Count + 1, section.HazardExposureRecords.Count);
            Assert.IsTrue(!double.IsNaN(section.HazardExposureRecords[0].MeanExposure));
            Assert.IsTrue(!double.IsNaN(section.HazardExposureRecords[0].MedianHc));
            Assert.IsTrue(!double.IsNaN(section.HazardExposureRecords[0].LowerExposure));
            Assert.IsTrue(!double.IsNaN(section.HazardExposureRecords[0].LowerExposure_UncLower));
            Assert.IsTrue(!double.IsNaN(section.HazardExposureRecords[0].UpperExposure));
            Assert.IsTrue(!double.IsNaN(section.HazardExposureRecords[0].UpperExposure_UncUpper));

            AssertIsValidView(section);
            RenderView(section, filename: "TestSummarizeMoeUncertain.html");
        }

        /// <summary>
        /// Summarize nominal, create chart, test HazardExposureSection view.
        /// </summary>
        [TestMethod]
        public void HazardExposureSection_TestSummarizeHiNominal() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(5);
            var individuals = MockIndividualsGenerator.Create(100, 1, random);

            var hazardCharacterisations = MockHazardCharacterisationModelsGenerator.Create(new Effect(), substances, seed);

            var individualEffects = MockIndividualEffectsGenerator.Create(individuals, substances, random);

            var reference = substances.First();
            var cumulativeRisks = MockIndividualEffectsGenerator.ComputeCumulativeIndividualEffects(
                individuals,
                individualEffects,
                reference
            );

            var section = new HazardExposureSection();
            section.Summarize(
                individualEffects,
                cumulativeRisks,
                HealthEffectType.Risk,
                substances,
                reference,
                hazardCharacterisations,
                riskMetricType: RiskMetricType.MarginOfExposure,
                riskMetricCalculationType: RiskMetricCalculationType.RPFWeighted,
                90,
                1,
                10,
                10,
                2.5,
                97.5
            );

            Assert.AreEqual(substances.Count + 1, section.HazardExposureRecords.Count);
            Assert.IsTrue(!double.IsNaN(section.HazardExposureRecords[0].MeanExposure));
            Assert.IsTrue(!double.IsNaN(section.HazardExposureRecords[0].MedianHc));
            Assert.IsTrue(!double.IsNaN(section.HazardExposureRecords[0].LowerExposure));
            Assert.IsTrue(double.IsNaN(section.HazardExposureRecords[0].LowerExposure_UncLower));
            Assert.IsTrue(!double.IsNaN(section.HazardExposureRecords[0].UpperExposure));
            Assert.IsTrue(double.IsNaN(section.HazardExposureRecords[0].UpperExposure_UncUpper));

            AssertIsValidView(section);
            RenderView(section, filename: "TestSummarizeHiNominal.html");
        }

        /// <summary>
        /// Summarize uncertain, create chart, test HazardExposureSection view
        /// </summary>
        [TestMethod]
        public void HazardExposureSection_TestSummarizeHiUncertain() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(5);
            var individuals = MockIndividualsGenerator.Create(100, 1, random);

            var hazardCharacterisations = MockHazardCharacterisationModelsGenerator.Create(new Effect(), substances, seed);

            var individualEffects = MockIndividualEffectsGenerator.Create(individuals, substances, random);

            var reference = substances.First();
            var cumulativeRisks = MockIndividualEffectsGenerator.ComputeCumulativeIndividualEffects(
                individuals,
                individualEffects,
                reference
            );

            var section = new HazardExposureSection();
            section.Summarize(
                individualEffects,
                cumulativeRisks,
                HealthEffectType.Risk,
                substances,
                reference,
                hazardCharacterisations,
                riskMetricType: RiskMetricType.MarginOfExposure,
                riskMetricCalculationType: RiskMetricCalculationType.RPFWeighted,
                90,
                1,
                10,
                10,
                2.5,
                97.5
            );

            for (int i = 0; i < 100; i++) {
                var substanceIndividualEffectsUncertains = MockIndividualEffectsGenerator
                    .CreateUncertain(substances, individualEffects, random);
                var cumulativeRisksUncertains = MockIndividualEffectsGenerator
                    .ComputeCumulativeIndividualEffects(individuals, individualEffects, reference);

                section.SummarizeUncertainty(
                    substanceIndividualEffectsUncertains,
                    cumulativeRisksUncertains,
                    hazardCharacterisations,
                    substances,
                    reference,
                    riskMetricType: RiskMetricType.MarginOfExposure,
                    riskMetricCalculationType: RiskMetricCalculationType.RPFWeighted,
                    2.5,
                    97.5
                );
            }

            Assert.AreEqual(substances.Count + 1, section.HazardExposureRecords.Count);
            Assert.IsTrue(!double.IsNaN(section.HazardExposureRecords[0].MeanExposure));
            Assert.IsTrue(!double.IsNaN(section.HazardExposureRecords[0].MedianHc));
            Assert.IsTrue(!double.IsNaN(section.HazardExposureRecords[0].LowerExposure));
            Assert.IsTrue(!double.IsNaN(section.HazardExposureRecords[0].LowerExposure_UncLower));
            Assert.IsTrue(!double.IsNaN(section.HazardExposureRecords[0].UpperExposure));
            Assert.IsTrue(!double.IsNaN(section.HazardExposureRecords[0].UpperExposure_UncUpper));

            AssertIsValidView(section);
            RenderView(section, filename: "TestSummarizeHiUncertain.html");
        }
    }
}
