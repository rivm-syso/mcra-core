using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.Simulation.Calculators.KineticModelCalculation.LinearDoseAggregationCalculation;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.KineticModelCalculation {

    [TestClass]
    public class LinearDoseAggregationCalculatorTests {

        /// <summary>
        ///  Linear dose aggregation: calculates individual target exposures based on absorption factors.
        /// </summary>
        [TestMethod]
        public void LinearDoseAggregationCalculatorTests_TestAbsorptionFactorModel() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var routes = new HashSet<ExposurePathType>() { ExposurePathType.Dermal, ExposurePathType.Oral };
            var individuals = MockIndividualsGenerator.Create(10, 2, random);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var substances = MockSubstancesGenerator.Create(1);
            var substance = substances.First();

            var factors = new Dictionary<ExposurePathType, double>() {
                { ExposurePathType.Oral, 1 },
                { ExposurePathType.Dermal, .1 },
                { ExposurePathType.Inhalation, .1 },
            };
            var calculator = new LinearDoseAggregationCalculator(substance, factors);
            var externalExposures = MockExternalExposureGenerator
                .CreateExternalIndividualDayExposures(individualDays, substances, routes, seed);
            var externalExposuresUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay);
            var result = calculator
                .CalculateIndividualDayTargetExposures(
                    externalExposures,
                    routes,
                    externalExposuresUnit,
                    new List<TargetUnit> { TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL) },
                    new ProgressState(),
                    random
                );
            Assert.AreEqual(result.Count, individualDays.Count);
        }

        /// <summary>
        /// Linear dose: calculates reverse dose based on absorption factors.
        /// </summary>
        [DataRow(10, DoseUnit.ugPerL, ExposurePathType.Oral, 1, 10)]
        [DataRow(10, DoseUnit.mgPerL, ExposurePathType.Oral, 1, 10000)]
        [DataRow(10, DoseUnit.ugPerL, ExposurePathType.Dermal, 0.1, 100)]
        [DataRow(10, DoseUnit.ugPerL, ExposurePathType.Inhalation, 0.1, 100)]
        [TestMethod]
        public void LinearDoseAggregationCalculatorTests_TestReverse(
            double internalDose,
            DoseUnit internalDoseUnit,
            ExposurePathType route,
            double factor,
            double expectedExternalDose
        ) {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(1);
            var substance = substances.First();
            var factors = new Dictionary<ExposurePathType, double>() {
                { route, factor }
            };
            var calculator = new LinearDoseAggregationCalculator(substance, factors);
            var externalExposuresUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
            var individual = MockIndividualsGenerator.CreateSingle();
            var externalDose = calculator
                .Reverse(
                    individual,
                    internalDose,
                    TargetUnit.FromInternalDoseUnit(internalDoseUnit),
                    route,
                    externalExposuresUnit,
                    ExposureType.Chronic,
                    random
                );

            Assert.AreEqual(expectedExternalDose, externalDose);
        }

        /// <summary>
        /// Linear dose: calculates reverse dose based on absorption factors.
        /// </summary>
        [DataRow(10, ExposurePathType.Oral, ExternalExposureUnit.ugPerKgBWPerDay, 0.1, 1)]
        [DataRow(10, ExposurePathType.Oral, ExternalExposureUnit.mgPerKgBWPerDay, 0.1, 0.001)]
        [DataRow(10, ExposurePathType.Dermal, ExternalExposureUnit.ugPerKgBWPerDay, 0.1, 1)]
        [DataRow(10, ExposurePathType.Inhalation, ExternalExposureUnit.ugPerKgBWPerDay, 0.5, 5)]
        [TestMethod]
        public void LinearDoseAggregationCalculatorTests_TestForward(
            double externalDose,
            ExposurePathType route,
            ExternalExposureUnit externalExposureUnit,
            double factor,
            double expectedInternalDose
        ) {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(1);
            var substance = substances.First();
            var factors = new Dictionary<ExposurePathType, double>() {
                { route, factor }
            };
            var calculator = new LinearDoseAggregationCalculator(substance, factors);
            var externalExposuresUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
            var individual = MockIndividualsGenerator.CreateSingle();
            var internalDose = calculator
                .Forward(
                    individual,
                    externalDose,
                    route,
                    externalExposuresUnit,
                    TargetUnit.FromExternalExposureUnit(externalExposureUnit),
                    ExposureType.Chronic,
                    random
                );

            Assert.AreEqual(expectedInternalDose, internalDose);
        }
    }
}
