using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.TargetExposuresCalculators;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.MixtureCalculation {
    /// <summary>
    /// MixtureCalculation calculator
    /// </summary>
    [TestClass]
    public class ExposureMatrixBuilderTests {

        /// <summary>
        /// Test exposure based acute data matrix
        /// </summary>
        [TestMethod]
        public void ExposureMatrixBuilder_TestAcuteExposureBased() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(4);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(100, 2, false, random);
            var exposureRoutes = new List<ExposureRouteType>() { ExposureRouteType.Dietary, ExposureRouteType.Dermal, ExposureRouteType.Oral, ExposureRouteType.Inhalation };
            var absorptionFactors = MockKineticModelsGenerator.CreateAbsorptionFactors(substances, 1);
            var kineticModelCalculators = MockKineticModelsGenerator.CreateAbsorptionFactorKineticModelCalculators(substances, absorptionFactors);
            var targetExposuresCalculator = new InternalTargetExposuresCalculator(kineticModelCalculators);

            var externalExposuresUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay);
            var individualDayExposures = MockAggregateIndividualDayIntakeGenerator
                .Create(
                    individualDays,
                    substances,
                    exposureRoutes,
                    targetExposuresCalculator,
                    externalExposuresUnit,
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
                exposureApproachType: ExposureApproachType.RiskBased,
                totalExposureCutOff: 0,
                ratioCutOff: 0
            );
            var targetExposureUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.mgPerGBWPerDay);
            var result = builder.Compute(individualDayExposures, null, targetExposureUnit);

            var positivesCount = individualDayExposures.Count(r => r.TotalConcentrationAtTarget(rpfs, memberships, false) > 0);
            Assert.AreEqual(substances.Count, result.Exposures.RowDimension);
            Assert.AreEqual(positivesCount, result.Exposures.ColumnDimension);
        }

        /// <summary>
        /// Test risk based acute data matrix
        /// </summary>
        [TestMethod]
        public void ExposureMatrixBuilder_TestAcuteRiskBased() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(4);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(100, 2, false, random);
            var exposureRoutes = new List<ExposureRouteType>() { ExposureRouteType.Dietary, ExposureRouteType.Dermal, ExposureRouteType.Oral, ExposureRouteType.Inhalation };
            var absorptionFactors = MockKineticModelsGenerator.CreateAbsorptionFactors(substances, 1);
            var kineticModelCalculators = MockKineticModelsGenerator.CreateAbsorptionFactorKineticModelCalculators(substances, absorptionFactors);
            var targetExposuresCalculator = new InternalTargetExposuresCalculator(kineticModelCalculators);
            var targetExposureUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.mgPerGBWPerDay);
            var externalExposuresUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay);
            var individualDayExposures = MockAggregateIndividualDayIntakeGenerator.Create(
                individualDays,
                substances,
                exposureRoutes,
                targetExposuresCalculator,
                externalExposuresUnit,
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
                exposureApproachType: ExposureApproachType.RiskBased,
                totalExposureCutOff: 0,
                ratioCutOff: 0
            );
            var result = builder.Compute(individualDayExposures, null, targetExposureUnit);
            var positivesCount = individualDayExposures.Count(r => r.TotalConcentrationAtTarget(rpfs, memberships, false) > 0);
            Assert.AreEqual(substances.Count, result.Exposures.RowDimension);
            Assert.AreEqual(positivesCount, result.Exposures.ColumnDimension);
        }

        /// <summary>
        /// Test empty data matrix chronic
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(Exception), AllowDerivedTypes = true)]
        public void ExposureMatrixBuilder_TestChronicEmpty() {
            var substances = MockSubstancesGenerator.Create(4);
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
            var result = builder.Compute(null, individualDayExposures, null);
        }
    }
}
