using MCRA.General;
using MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.TargetExposuresCalculators;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;

namespace MCRA.Simulation.Test.UnitTests.Calculators.MixtureCalculation {

    /// <summary>
    /// Exposure matrix builder tests.
    /// </summary>
    [TestClass]
    public class ExposureMatrixBuilderTests {

        /// <summary>
        /// Test exposure matrix calculaation for acute aggregate individual day exposures.
        /// </summary>
        [TestMethod]
        [DataRow(ExposureApproachType.RiskBased)]
        [DataRow(ExposureApproachType.ExposureBased)]
        [DataRow(ExposureApproachType.UnweightedExposures)]
        public void ExposureMatrixBuilder_TestCreateAcuteAggregate(
            ExposureApproachType approachType
        ) {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = FakeSubstancesGenerator.Create(4);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(100, 2, false, random);
            var routes = new[] { ExposureRoute.Dermal, ExposureRoute.Oral, ExposureRoute.Inhalation };
            var kineticConversionFactors = FakeKineticModelsGenerator.CreateAbsorptionFactors(substances, 1);
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Liver);
            var kineticModelCalculators = FakeKineticModelsGenerator.CreateAbsorptionFactorKineticModelCalculators(
                substances,
                kineticConversionFactors,
                targetUnit
            );
            var targetExposuresCalculator = new InternalTargetExposuresCalculator(kineticModelCalculators);
            var externalExposuresUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay);

            var individualDayExposures = FakeAggregateIndividualDayExposuresGenerator
                .Create(
                    individualDays,
                    substances,
                    routes,
                    kineticModelCalculators,
                    externalExposuresUnit,
                    targetUnit,
                    random
                );
            var rpfs = substances.ToDictionary(r => r, r => 1D);
            var memberships = substances.ToDictionary(r => r, r => 1D);

            var builder = new ExposureMatrixBuilder(
                substances: substances,
                relativePotencyFactors: rpfs,
                membershipProbabilities: memberships,
                exposureType: ExposureType.Acute,
                isPerPerson: false,
                exposureApproachType: approachType,
                totalExposureCutOff: 0,
                ratioCutOff: 0
            );
            var result = builder.Compute(individualDayExposures, null, targetUnit);

            var positivesCount = individualDayExposures
                .Count(r => r.IsPositiveTargetExposure(targetUnit.Target));
            Assert.AreEqual(substances.Count, result.Exposures.RowDimension);
            Assert.AreEqual(positivesCount, result.Exposures.ColumnDimension);
        }

        /// <summary>
        /// Test risk based acute data matrix
        /// </summary>
        [TestMethod]
        [DataRow(ExposureApproachType.RiskBased)]
        [DataRow(ExposureApproachType.ExposureBased)]
        [DataRow(ExposureApproachType.UnweightedExposures)]
        public void ExposureMatrixBuilder_TestCreateChronicAggregate(
            ExposureApproachType approachType
        ) {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = FakeSubstancesGenerator.Create(4);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(100, 2, false, random);
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Liver);
            var individualExposures = FakeAggregateIndividualExposuresGenerator.Create(
                individualDays,
                substances,
                targetUnit,
                random
            );
            var rpfs = substances.ToDictionary(r => r, r => 1D);
            var memberships = substances.ToDictionary(r => r, r => 1D);

            var builder = new ExposureMatrixBuilder(
                substances: substances,
                relativePotencyFactors: rpfs,
                membershipProbabilities: memberships,
                exposureType: ExposureType.Chronic,
                isPerPerson: false,
                exposureApproachType: approachType,
                totalExposureCutOff: 0,
                ratioCutOff: 0
            );
            var result = builder.Compute(null, individualExposures, targetUnit);
            var positivesCount = individualExposures
                .Count(r => r.IsPositiveTargetExposure(targetUnit.Target));
            Assert.AreEqual(substances.Count, result.Exposures.RowDimension);
            Assert.AreEqual(positivesCount, result.Exposures.ColumnDimension);
        }

        /// <summary>
        /// Test risk based acute data matrix
        /// </summary>
        [TestMethod]
        [DataRow(ExposureApproachType.RiskBased)]
        [DataRow(ExposureApproachType.ExposureBased)]
        [DataRow(ExposureApproachType.UnweightedExposures)]
        public void ExposureMatrixBuilder_TestCreateAcuteDietary(
            ExposureApproachType approachType
        ) {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = FakeSubstancesGenerator.Create(4);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(100, 2, false, random);
            var foodsAsMeasured = FakeFoodsGenerator.Create(3);
            var dietaryIndividualDayIntakes = FakeDietaryIndividualDayIntakeGenerator.Create(individualDays, foodsAsMeasured, substances, 0, true, random);
            var dietaryExposureUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerGBWPerDay);
            var rpfs = substances.ToDictionary(r => r, r => 1D);
            var memberships = substances.ToDictionary(r => r, r => 1D);

            var builder = new ExposureMatrixBuilder(
                substances: substances,
                relativePotencyFactors: rpfs,
                membershipProbabilities: memberships,
                exposureType: ExposureType.Acute,
                isPerPerson: false,
                exposureApproachType: approachType,
                totalExposureCutOff: 0,
                ratioCutOff: 0
            );
            var targetUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
            var result = builder.Compute(dietaryIndividualDayIntakes, targetUnit);
            var positivesCount = dietaryIndividualDayIntakes.Count(r => r.IsPositiveIntake());
            Assert.AreEqual(substances.Count, result.Exposures.RowDimension);
            Assert.AreEqual(positivesCount, result.Exposures.ColumnDimension);
        }


        /// <summary>
        /// Test risk based chronic data matrix
        /// </summary>
        [TestMethod]
        [DataRow(ExposureApproachType.RiskBased)]
        [DataRow(ExposureApproachType.ExposureBased)]
        [DataRow(ExposureApproachType.UnweightedExposures)]
        public void ExposureMatrixBuilder_TestCreateChronicDietary(
            ExposureApproachType approachType
        ) {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = FakeSubstancesGenerator.Create(4);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(100, 2, false, random);
            var foodsAsMeasured = FakeFoodsGenerator.Create(3);
            var dietaryIndividualDayIntakes = FakeDietaryIndividualDayIntakeGenerator.Create(individualDays, foodsAsMeasured, substances, 0, true, random);
            var dietaryExposureUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerGBWPerDay);
            var rpfs = substances.ToDictionary(r => r, r => 1D);
            var memberships = substances.ToDictionary(r => r, r => 1D);

            var builder = new ExposureMatrixBuilder(
                substances: substances,
                relativePotencyFactors: rpfs,
                membershipProbabilities: memberships,
                exposureType: ExposureType.Chronic,
                isPerPerson: false,
                exposureApproachType: approachType,
                totalExposureCutOff: 0,
                ratioCutOff: 0
            );
            var targetUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
            var result = builder.Compute(dietaryIndividualDayIntakes, targetUnit);
            var positivesCount = dietaryIndividualDayIntakes.Count(r => r.IsPositiveIntake());
            Assert.AreEqual(substances.Count, result.Exposures.RowDimension);
            Assert.AreEqual(positivesCount / 2, result.Exposures.ColumnDimension);
        }

        /// <summary>
        /// Test risk based acute data matrix
        /// </summary>
        [TestMethod]
        [DataRow(ExposureApproachType.RiskBased)]
        [DataRow(ExposureApproachType.ExposureBased)]
        [DataRow(ExposureApproachType.UnweightedExposures)]
        public void ExposureMatrixBuilder_TestCreateAcuteHBM(
            ExposureApproachType approachType
        ) {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = FakeSubstancesGenerator.Create(4);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(100, 2, false, random);
            var samplingMethod = FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod();
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.mgPerL, BiologicalMatrix.Blood);
            var hbmIndividualDayConcentrations = new List<HbmIndividualDayCollection> { FakeHbmIndividualDayConcentrationsGenerator
                .Create(individualDays, substances, samplingMethod, targetUnit, random) };

            var rpfs = substances.ToDictionary(r => r, r => 1D);
            var memberships = substances.ToDictionary(r => r, r => 1D);

            var builder = new ExposureMatrixBuilder(
                substances: substances,
                relativePotencyFactors: rpfs,
                membershipProbabilities: memberships,
                exposureType: ExposureType.Acute,
                isPerPerson: false,
                exposureApproachType: approachType,
                totalExposureCutOff: 0,
                ratioCutOff: 0
            );
            var result = builder.Compute(hbmIndividualDayConcentrations, null);
            var positivesCount = hbmIndividualDayConcentrations
                .SelectMany(r => r.HbmIndividualDayConcentrations)
                .Select(c => c.IsPositiveExposure())
                .Count();
            Assert.AreEqual(substances.Count, result.Exposures.RowDimension);
            Assert.AreEqual(positivesCount, result.Exposures.ColumnDimension);
        }

        /// <summary>
        /// Test risk based chronic data matrix
        /// </summary>
        [TestMethod]
        [DataRow(ExposureApproachType.RiskBased)]
        [DataRow(ExposureApproachType.ExposureBased)]
        [DataRow(ExposureApproachType.UnweightedExposures)]
        public void ExposureMatrixBuilder_TestCreateChronicHBM(
            ExposureApproachType approachType
        ) {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = FakeSubstancesGenerator.Create(4);
            var individuals = FakeIndividualsGenerator.Create(100, 2, random);
            var samplingMethod = FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod();
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.mgPerL, BiologicalMatrix.Blood);
            var hbmIndividualConcentrations = FakeHbmIndividualConcentrationsGenerator
                .Create(individuals, substances, samplingMethod, targetUnit, random);

            var rpfs = substances.ToDictionary(r => r, r => 1D);
            var memberships = substances.ToDictionary(r => r, r => 1D);

            var builder = new ExposureMatrixBuilder(
                substances: substances,
                relativePotencyFactors: rpfs,
                membershipProbabilities: memberships,
                exposureType: ExposureType.Chronic,
                isPerPerson: false,
                exposureApproachType: approachType,
                totalExposureCutOff: 0,
                ratioCutOff: 0
            );
            var result = builder.Compute(null, hbmIndividualConcentrations);
            var positivesCount = hbmIndividualConcentrations
                .SelectMany(r => r.HbmIndividualConcentrations)
                .Select(c => c.IsPositiveExposure())
                .Count();
            Assert.AreEqual(substances.Count, result.Exposures.RowDimension);
            Assert.AreEqual(positivesCount, result.Exposures.ColumnDimension);
        }

        /// <summary>
        /// Test empty data matrix chronic. Should throw an exception.
        /// </summary>
        [TestMethod]
        public void ExposureMatrixBuilder_TestChronicEmpty() {
            var substances = FakeSubstancesGenerator.Create(4);
            var rpfs = substances.ToDictionary(r => r, r => 1D);
            var memberships = substances.ToDictionary(r => r, r => 1D);
            var individualDayExposures = new List<AggregateIndividualExposure>();
            var builder = new ExposureMatrixBuilder(
                substances: substances,
                relativePotencyFactors: rpfs,
                membershipProbabilities: memberships,
                exposureType: ExposureType.Chronic,
                isPerPerson: false,
                exposureApproachType: ExposureApproachType.RiskBased,
                totalExposureCutOff: 0,
                ratioCutOff: 0
            );
            Assert.ThrowsException<Exception>(() => builder.Compute(
                null,
                individualDayExposures,
                TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay)
                )
            );
        }
    }
}
