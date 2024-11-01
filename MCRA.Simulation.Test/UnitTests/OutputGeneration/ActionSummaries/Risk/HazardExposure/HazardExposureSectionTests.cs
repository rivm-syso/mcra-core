using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using MCRA.Utils.Statistics;
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
            var individuals = FakeIndividualsGenerator.Create(100, 1, random);

            var targetUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerGBWPerDay, ExposureRoute.Oral);
            var hazardCharacterisations = MockHazardCharacterisationModelsGenerator.Create(new Effect(), substances, seed);
            var hazardCharacterisationCollection = new HazardCharacterisationModelCompoundsCollection() {
                TargetUnit = targetUnit,
                HazardCharacterisationModels = hazardCharacterisations
            };
            var hazardCharacterisationCollections = new List<HazardCharacterisationModelCompoundsCollection> { hazardCharacterisationCollection };
            var individualEffects = MockIndividualEffectsGenerator.Create(individuals, substances, random);

            var reference = substances.First();
            var cumulativeRisks = MockIndividualEffectsGenerator.ComputeCumulativeIndividualEffects(
                individuals,
                individualEffects,
                reference
            );
            var individualEffectsBySubstanceCollections = new List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> IndividualEffects)> {
                (targetUnit.Target, individualEffects)
            };
            var section = new HazardExposureSection();
            section.Summarize(
                new List<ExposureTarget>() { targetUnit.Target },
                individualEffectsBySubstanceCollections,
                cumulativeRisks,
                HealthEffectType.Risk,
                substances,
                hazardCharacterisationCollections,
                hazardCharacterisations[reference],
                riskMetricType: RiskMetricType.HazardExposureRatio,
                riskMetricCalculationType: RiskMetricCalculationType.SumRatios,
                confidenceInterval: 90,
                threshold: 1,
                numberOfLabels: 10,
                uncertaintyLowerBound: 2.5,
                uncertaintyUpperBound: 97.5,
                skipPrivacySensitiveOutputs: false,
                isCumulative: true
            );

            Assert.AreEqual(substances.Count + 1, section.HazardExposureRecords.SelectMany(c => c.Records).Count());
            Assert.IsTrue(!double.IsNaN(section.HazardExposureRecords[0].Records[0].MeanExposure));
            Assert.IsTrue(!double.IsNaN(section.HazardExposureRecords[0].Records[0].MedianHc));
            Assert.IsTrue(!double.IsNaN(section.HazardExposureRecords[0].Records[0].LowerExposure));
            Assert.IsTrue(double.IsNaN(section.HazardExposureRecords[0].Records[0].LowerExposure_UncLower));
            Assert.IsTrue(!double.IsNaN(section.HazardExposureRecords[0].Records[0].UpperExposure));
            Assert.IsTrue(double.IsNaN(section.HazardExposureRecords[0].Records[0].UpperExposure_UncUpper));

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
            var individuals = FakeIndividualsGenerator.Create(100, 1, random);
            var targetUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerGBWPerDay, ExposureRoute.Oral);
            var hazardCharacterisations = MockHazardCharacterisationModelsGenerator.Create(new Effect(), substances, seed);
            var hazardCharacterisationCollection = new HazardCharacterisationModelCompoundsCollection() {
                TargetUnit = targetUnit,
                HazardCharacterisationModels = hazardCharacterisations
            };
            var hazardCharacterisationCollections = new List<HazardCharacterisationModelCompoundsCollection> { hazardCharacterisationCollection };
            var individualEffects = MockIndividualEffectsGenerator.Create(individuals, substances, random);

            var reference = substances.First();
            var cumulativeRisks = MockIndividualEffectsGenerator.ComputeCumulativeIndividualEffects(
                individuals,
                individualEffects,
                reference
            );
            var individualEffectsBySubstanceCollections = new List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> IndividualEffects)> {
                (targetUnit.Target, individualEffects)
            };
            var section = new HazardExposureSection();
            section.Summarize(
                new List<ExposureTarget>() { targetUnit.Target },
                individualEffectsBySubstanceCollections,
                cumulativeRisks,
                HealthEffectType.Risk,
                substances,
                hazardCharacterisationCollections,
                hazardCharacterisations[reference],
                riskMetricType: RiskMetricType.HazardExposureRatio,
                riskMetricCalculationType: RiskMetricCalculationType.SumRatios,
                confidenceInterval: 90,
                threshold: 1,
                numberOfLabels: 10,
                uncertaintyLowerBound: 2.5,
                uncertaintyUpperBound: 97.5,
                skipPrivacySensitiveOutputs: false,
                isCumulative: true
            );

            for (int i = 0; i < 100; i++) {
                var substanceIndividualEffectsUncertains = MockIndividualEffectsGenerator
                    .CreateUncertain(substances, individualEffects, random);
                var cumulativeRiskUncertains = MockIndividualEffectsGenerator
                    .ComputeCumulativeIndividualEffects(individuals, individualEffects, reference);
                var individualEffectsBySubstanceCollectionsUncertain = new List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> IndividualEffects)> {
                (targetUnit.Target , substanceIndividualEffectsUncertains)
            };
                section.SummarizeUncertainty(
                    individualEffectsBySubstanceCollections.Select(c => c.Target).ToList(),
                    individualEffectsBySubstanceCollectionsUncertain,
                    cumulativeRiskUncertains,
                    hazardCharacterisationCollections,
                    substances,
                    hazardCharacterisations[reference],
                    riskMetricCalculationType: RiskMetricCalculationType.RPFWeighted,
                    riskMetricType: RiskMetricType.HazardExposureRatio,
                    2.5,
                    97.5,
                    true
                );
            }

            Assert.AreEqual(substances.Count + 1, section.HazardExposureRecords.SelectMany(c => c.Records).Count());
            Assert.IsTrue(!double.IsNaN(section.HazardExposureRecords[0].Records[0].MeanExposure));
            Assert.IsTrue(!double.IsNaN(section.HazardExposureRecords[0].Records[0].MedianHc));
            Assert.IsTrue(!double.IsNaN(section.HazardExposureRecords[0].Records[0].LowerExposure));
            Assert.IsTrue(!double.IsNaN(section.HazardExposureRecords[0].Records[0].LowerExposure_UncLower));
            Assert.IsTrue(!double.IsNaN(section.HazardExposureRecords[0].Records[0].UpperExposure));
            Assert.IsTrue(!double.IsNaN(section.HazardExposureRecords[0].Records[0].UpperExposure_UncUpper));

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
            var individuals = FakeIndividualsGenerator.Create(100, 1, random);
            var targetUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerGBWPerDay, ExposureRoute.Oral);
            var hazardCharacterisations = MockHazardCharacterisationModelsGenerator.Create(new Effect(), substances, seed);
            var hazardCharacterisationCollection = new HazardCharacterisationModelCompoundsCollection() {
                TargetUnit = targetUnit,
                HazardCharacterisationModels = hazardCharacterisations
            };
            var hazardCharacterisationCollections = new List<HazardCharacterisationModelCompoundsCollection> { hazardCharacterisationCollection };
            var individualEffects = MockIndividualEffectsGenerator.Create(individuals, substances, random);

            var reference = substances.First();
            var cumulativeRisks = MockIndividualEffectsGenerator.ComputeCumulativeIndividualEffects(
                individuals,
                individualEffects,
                reference
            );
            var individualEffectsBySubstanceCollections = new List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> IndividualEffects)> {
                (targetUnit.Target, individualEffects)
            };
            var section = new HazardExposureSection();
            section.Summarize(
                new List<ExposureTarget>() { targetUnit.Target },
                individualEffectsBySubstanceCollections,
                cumulativeRisks,
                HealthEffectType.Risk,
                substances,
                hazardCharacterisationCollections,
                hazardCharacterisations[reference],
                riskMetricType: RiskMetricType.HazardExposureRatio,
                riskMetricCalculationType: RiskMetricCalculationType.SumRatios,
                confidenceInterval: 90,
                threshold: 1,
                numberOfLabels: 10,
                uncertaintyLowerBound: 2.5,
                uncertaintyUpperBound: 97.5,
                skipPrivacySensitiveOutputs: false,
                isCumulative: true
            );

            Assert.AreEqual(substances.Count + 1, section.HazardExposureRecords.SelectMany(c => c.Records).Count());
            Assert.IsTrue(!double.IsNaN(section.HazardExposureRecords[0].Records[0].MeanExposure));
            Assert.IsTrue(!double.IsNaN(section.HazardExposureRecords[0].Records[0].MedianHc));
            Assert.IsTrue(!double.IsNaN(section.HazardExposureRecords[0].Records[0].LowerExposure));
            Assert.IsTrue(double.IsNaN(section.HazardExposureRecords[0].Records[0].LowerExposure_UncLower));
            Assert.IsTrue(!double.IsNaN(section.HazardExposureRecords[0].Records[0].UpperExposure));
            Assert.IsTrue(double.IsNaN(section.HazardExposureRecords[0].Records[0].UpperExposure_UncUpper));

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
            var individuals = FakeIndividualsGenerator.Create(100, 1, random);
            var targetUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerGBWPerDay, ExposureRoute.Oral);
            var hazardCharacterisations = MockHazardCharacterisationModelsGenerator.Create(new Effect(), substances, seed);
            var hazardCharacterisationCollection = new HazardCharacterisationModelCompoundsCollection() {
                TargetUnit = targetUnit,
                HazardCharacterisationModels = hazardCharacterisations
            };
            var hazardCharacterisationCollections = new List<HazardCharacterisationModelCompoundsCollection> { hazardCharacterisationCollection };
            var individualEffects = MockIndividualEffectsGenerator.Create(individuals, substances, random);

            var reference = substances.First();
            var cumulativeRisks = MockIndividualEffectsGenerator.ComputeCumulativeIndividualEffects(
                individuals,
                individualEffects,
                reference
            );
            var individualEffectsBySubstanceCollections = new List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> IndividualEffects)> {
                (targetUnit.Target, individualEffects)
            };
            var section = new HazardExposureSection();
            section.Summarize(
                new List<ExposureTarget>() { targetUnit.Target },
                individualEffectsBySubstanceCollections,
                cumulativeRisks,
                HealthEffectType.Risk,
                substances,
                hazardCharacterisationCollections,
                hazardCharacterisations[reference],
                riskMetricType: RiskMetricType.HazardExposureRatio,
                riskMetricCalculationType: RiskMetricCalculationType.SumRatios,
                confidenceInterval: 90,
                threshold: 1,
                numberOfLabels: 10,
                uncertaintyLowerBound: 2.5,
                uncertaintyUpperBound: 97.5,
                skipPrivacySensitiveOutputs: false,
                isCumulative: true
            );

            for (int i = 0; i < 100; i++) {
                var substanceIndividualEffectsUncertains = MockIndividualEffectsGenerator
                    .CreateUncertain(substances, individualEffects, random);
                var cumulativeRisksUncertains = MockIndividualEffectsGenerator
                    .ComputeCumulativeIndividualEffects(individuals, individualEffects, reference);
                var individualEffectsBySubstanceCollectionsUncertain = new List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> IndividualEffects)> {
                (targetUnit.Target, substanceIndividualEffectsUncertains)
            };
                section.SummarizeUncertainty(
                    individualEffectsBySubstanceCollections.Select(c => c.Target).ToList(),
                    individualEffectsBySubstanceCollectionsUncertain,
                    cumulativeRisksUncertains,
                    hazardCharacterisationCollections,
                    substances,
                    hazardCharacterisations[reference],
                    riskMetricCalculationType: RiskMetricCalculationType.RPFWeighted,
                    riskMetricType: RiskMetricType.HazardExposureRatio,
                    2.5,
                    97.5,
                    true
                );
            }

            Assert.AreEqual(substances.Count + 1, section.HazardExposureRecords.SelectMany(c => c.Records).Count());
            Assert.IsTrue(!double.IsNaN(section.HazardExposureRecords[0].Records[0].MeanExposure));
            Assert.IsTrue(!double.IsNaN(section.HazardExposureRecords[0].Records[0].MedianHc));
            Assert.IsTrue(!double.IsNaN(section.HazardExposureRecords[0].Records[0].LowerExposure));
            Assert.IsTrue(!double.IsNaN(section.HazardExposureRecords[0].Records[0].LowerExposure_UncLower));
            Assert.IsTrue(!double.IsNaN(section.HazardExposureRecords[0].Records[0].UpperExposure));
            Assert.IsTrue(!double.IsNaN(section.HazardExposureRecords[0].Records[0].UpperExposure_UncUpper));

            AssertIsValidView(section);
            RenderView(section, filename: "TestSummarizeHiUncertain.html");
        }
    }
}
