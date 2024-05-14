using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.TargetExposuresCalculators;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.TargetExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, TargetExposures, ExposureByCompound, ByCompound
    /// </summary>
    [TestClass]
    public class TotalDistributionCompoundSectionTests : SectionTestBase {
        /// <summary>
        /// Summarize (uncertainty) acute dietary, test TotalDistributionCompoundSection view
        /// </summary>
        [TestMethod]
        public void TotalDistributionCompoundSection_TestDietaryAcute() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.MockFoods("Apple", "Pear", "Bananas");
            var substances = MockSubstancesGenerator.Create(3);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var exposures = MockDietaryIndividualDayIntakeGenerator.Create(individualDays, foods, substances, 0.5, true, random);

            var section = new TotalDistributionCompoundSection();
            section.Summarize(exposures, substances, rpfs, memberships, ExposureType.Acute, 25, 75, 2.5, 97.5, false);
            Assert.AreEqual(100D, section.Records.Sum(c => c.ContributionPercentage), .001);
            Assert.AreEqual(substances.Count, section.Records.Count);

            section.SummarizeUncertainty(exposures, substances, rpfs, memberships, ExposureType.Acute, false);
            Assert.AreEqual(3, section.Records.SelectMany(c => c.Contributions).Count());
            AssertIsValidView(section);
        }
        /// <summary>
        /// Summarize (uncertainty) chronic dietary, test TotalDistributionCompoundSection view
        /// </summary>
        [TestMethod]
        public void TotalDistributionCompoundSection_TestDietaryChronic() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.MockFoods("Apple", "Pear", "Bananas");
            var substances = MockSubstancesGenerator.Create(3);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var exposures = MockDietaryIndividualDayIntakeGenerator.Create(individualDays, foods, substances, 0.5, true, random);

            var section = new TotalDistributionCompoundSection();
            section.Summarize(exposures, substances, rpfs, memberships, ExposureType.Chronic, 25, 75, 2.5, 97.5, false);

            Assert.AreEqual(100D, section.Records.Sum(c => c.ContributionPercentage), .001);
            Assert.AreEqual(substances.Count, section.Records.Count);

            section.SummarizeUncertainty(exposures, substances, rpfs, memberships, ExposureType.Chronic, false);
            Assert.AreEqual(3, section.Records.SelectMany(c => c.Contributions).Count());
        }

        /// <summary>
        /// Summarize (uncertainty) chronic dietary and nondietary aggregate
        /// </summary>
        [TestMethod]
        public void TotalDistributionCompoundSection_TestAggregateChronic() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var allRoutes = new List<ExposurePathType>() { ExposurePathType.Dermal, ExposurePathType.Oral, ExposurePathType.Inhalation };
            var exposureRoutes = allRoutes.Where(r => random.NextDouble() > .5).ToList();
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(100, 2, false, random);
            var substances = MockSubstancesGenerator.Create(random.Next(1, 4));
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var kineticConversionFactors = MockKineticModelsGenerator.CreateAbsorptionFactors(substances, .1);
            var kineticModelCalculators = MockKineticModelsGenerator.CreateAbsorptionFactorKineticModelCalculators(substances, kineticConversionFactors);
            var externalExposuresUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Liver);
            var aggregateIndividualExposures = FakeAggregateIndividualExposuresGenerator.Create(
                individualDays,
                substances,
                exposureRoutes,
                kineticModelCalculators,
                externalExposuresUnit,
                targetUnit,
                random
            );

            var section = new TotalDistributionCompoundSection();
            section.Summarize(
                aggregateIndividualExposures,
                null,
                rpfs,
                memberships,
                kineticConversionFactors,
                substances,
                25,
                75,
                2.5,
                97.5,
                externalExposuresUnit);
            if (aggregateIndividualExposures.Any(r => r.IsPositiveTargetExposure(targetUnit.Target))) {
                Assert.AreEqual(100D, section.Records.Sum(c => c.ContributionPercentage), .001);
            }
            Assert.AreEqual(substances.Count, section.Records.Count);

            section.SummarizeUncertainty(
                aggregateIndividualExposures,
                null,
                rpfs,
                memberships,
                kineticConversionFactors,
                substances,
                externalExposuresUnit
            );
            Assert.AreEqual(substances.Count, section.Records.SelectMany(c => c.Contributions).Count());
        }
        /// <summary>
        /// Summarize (uncertainty) acute dietary and nondietary aggregate
        /// </summary>
        [TestMethod]
        public void TotalDistributionCompoundSection_TestAggregateAcute() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var allRoutes = new List<ExposurePathType>() { ExposurePathType.Dermal, ExposurePathType.Oral, ExposurePathType.Inhalation };
            var exposureRoutes = allRoutes.Where(r => random.NextDouble() > .5).ToList();
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(100, 2, false, random);
            var substances = MockSubstancesGenerator.Create(random.Next(1, 4));
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var kineticConversionFactors = MockKineticModelsGenerator.CreateAbsorptionFactors(substances, .1);
            var kineticModelCalculators = MockKineticModelsGenerator.CreateAbsorptionFactorKineticModelCalculators(substances, kineticConversionFactors);
            var targetExposuresCalculator = new InternalTargetExposuresCalculator(kineticModelCalculators);
            var externalExposuresUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Liver);
            var aggregateIndividualDayExposures = FakeAggregateIndividualDayExposuresGenerator
                .Create(
                    individualDays,
                    substances,
                    exposureRoutes,
                    targetExposuresCalculator,
                    externalExposuresUnit,
                    targetUnit,
                    random
                );

            var section = new TotalDistributionCompoundSection();
            section.Summarize(
                null,
                aggregateIndividualDayExposures,
                rpfs,
                memberships,
                kineticConversionFactors,
                substances,
                25,
                75,
                2.5,
                97.5,
                externalExposuresUnit);
            if (aggregateIndividualDayExposures.Any(r => r.IsPositiveTargetExposure(targetUnit.Target))) {
                Assert.AreEqual(100D, section.Records.Sum(c => c.ContributionPercentage), .001);
            }
            Assert.AreEqual(substances.Count, section.Records.Count);

            section.SummarizeUncertainty(
                null,
                aggregateIndividualDayExposures,
                rpfs,
                memberships,
                kineticConversionFactors,
                substances,
                externalExposuresUnit
            );
            Assert.AreEqual(substances.Count, section.Records.SelectMany(c => c.Contributions).Count());
        }
    }
}
